/* ........................................................................
 * copyright 2019 Laurent Dupuis
 * ........................................................................
 * < This program is free software: you can redistribute it and/or modify
 * < it under the terms of the GNU General Public License as published by
 * < the Free Software Foundation, either version 3 of the License, or
 * < (at your option) any later version.
 * < 
 * < This program is distributed in the hope that it will be useful,
 * < but WITHOUT ANY WARRANTY; without even the implied warranty of
 * < MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * < GNU General Public License for more details.
 * < 
 * < You should have received a copy of the GNU General Public License
 * < along with this program.  If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.
 * ........................................................................
 *
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WellKnownLib
{
    class WktParser
    {
        static readonly Regex RxKeywords = new Regex(@"^\s*(?<word>((POINT)|(LINESTRING)|(POLYGON)|(MULTIPOINT)|(MULTILINESTRING)|(MULTIPOLYGON)|(GEOMETRYCOLLECTION)|(EMPTY)))", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex RxSymbol = new Regex(@"^\s*(?<symbol>[(),])", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex RxNumber = new Regex(@"^\s*(?<number>-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex RxWhitespace = new Regex(@"^\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        readonly string _text;
        int position = 0;

        enum TokenType
        {
            POINT,
            LINESTRING,
            POLYGON,
            MULTIPOINT,
            MULTILINESTRING,
            MULTIPOLYGON,
            GEOMETRYCOLLECTION,
            EMPTY,
            xComma,
            xOpenParenthesis,
            xCloseParenthesis,
            xNumber,

            xEOF
        }


        public WktParser(string text)
        {
            _text = text;
        }


        Match CheckMatchAdvance(Regex rx)
        {
            var m = rx.Match(_text, position, _text.Length - position);
            if (m.Success)
            {
                position = m.Index + m.Length;
            }
            return m;
        }

        bool CheckForOpenParenthesis()
        {
            var m = RxSymbol.Match(_text, position, _text.Length - position);
            if (!m.Success) return false;

            var text = m.Groups["symbol"].Value;
            return text[0] == '(';
        }

        TokenType GetNextToken()
        {
            var m = CheckMatchAdvance(RxWhitespace);
            if (m.Success) return TokenType.xEOF;

            m = CheckMatchAdvance(RxSymbol);
            if (m.Success)
            {
                var text = m.Groups["symbol"].Value;
                switch (text[0])
                {
                    case ',': return TokenType.xComma;
                    case '(': return TokenType.xOpenParenthesis;
                    case ')': return TokenType.xCloseParenthesis;
                    default:
                        throw new ArgumentException();
                }
            }

            m = CheckMatchAdvance(RxKeywords);
            if (m.Success)
            {
                var text = m.Groups["word"].Value;
                return (TokenType)Enum.Parse(typeof(TokenType), text, true);
            }

            throw new Exception("Invalid text");
        }

        void ExpectToken(TokenType type)
        {
            var token = GetNextToken();
            if (token != type) throw new Exception("Invalid text");
        }

        bool CheckOpenOrEmpty()
        {
            var open = GetNextToken();
            if (open == TokenType.EMPTY) return true;
            if (open != TokenType.xOpenParenthesis) throw new Exception("Invalid text");
            return false;
        }

        Point[] DecodePointString()
        {
            var points = new List<Point>();
            for (; ; )
            {
                points.Add(ReadPoint());

                var separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");
            }
            return points.ToArray();
        }

        Point ReadPoint()
        {

            var m = CheckMatchAdvance(RxNumber);
            if (!m.Success) throw new Exception("Invalid text");

            var valX = double.Parse(m.Groups["number"].Value, System.Globalization.CultureInfo.InvariantCulture);

            m = CheckMatchAdvance(RxNumber);
            if (!m.Success) throw new Exception("Invalid text");

            var valY = double.Parse(m.Groups["number"].Value, System.Globalization.CultureInfo.InvariantCulture);

            return new Point(valX, valY);
        }

        WknPoint DecodePoint()
        {
            ExpectToken(TokenType.xOpenParenthesis);
            var p = ReadPoint();
            ExpectToken(TokenType.xCloseParenthesis);

            return new WknPoint(p);
        }

        WknLineString DecodeLineString()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknLineString(new Point[0]);
            }

            var points = DecodePointString();
            return new WknLineString(points);
        }

        WknPolygon DecodePolygon()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknPolygon(new LinearRing[0]);
            }

            ExpectToken(TokenType.xOpenParenthesis);
            var rings = new List<LinearRing>();
            for (; ; )
            {
                var ring = DecodePointString();
                rings.Add(new LinearRing(ring));

                var separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");

                ExpectToken(TokenType.xOpenParenthesis);
            }

            return new WknPolygon(rings.ToArray());
        }

        WknMultiPoint DecodeMultiPoint()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknMultiPoint(new WknPoint[0]);
            }

            var points = new List<WknPoint>();
            for (; ; )
            {
                Point p;
                if (CheckForOpenParenthesis())
                {
                    ExpectToken(TokenType.xOpenParenthesis);
                    p = ReadPoint();
                    ExpectToken(TokenType.xCloseParenthesis);
                }
                else
                {
                    p = ReadPoint();
                }
                points.Add(new WknPoint(p));

                var separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");
            }

            return new WknMultiPoint(points.ToArray());
        }

        WknMultiLineString DecodeMultiLineString()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknMultiLineString(new WknLineString[0]);
            }

            ExpectToken(TokenType.xOpenParenthesis);
            var strings = new List<WknLineString>();
            for (; ; )
            {
                var @string = DecodePointString();
                strings.Add(new WknLineString(@string));

                var separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");

                ExpectToken(TokenType.xOpenParenthesis);
            }

            return new WknMultiLineString(strings.ToArray());
        }


        WknMultiPolygon DecodeMultiPolygon()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknMultiPolygon(new WknPolygon[0]);
            }

            ExpectToken(TokenType.xOpenParenthesis);
            var polies = new List<WknPolygon>();

            TokenType separator;
            for(; ; )
            {
                ExpectToken(TokenType.xOpenParenthesis);
                var rings = new List<LinearRing>();
                for (; ; )
                {
                    var ring = DecodePointString();
                    rings.Add(new LinearRing(ring));

                    separator = GetNextToken();
                    if (separator == TokenType.xCloseParenthesis) break;
                    if (separator != TokenType.xComma) throw new Exception("Invalid text");

                    ExpectToken(TokenType.xOpenParenthesis);
                }

                polies.Add(new WknPolygon(rings.ToArray()));

                separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");

                ExpectToken(TokenType.xOpenParenthesis);
            }

            return new WknMultiPolygon(polies.ToArray());
        }

        public WknGeometryCollection DecodeGeometryCollection()
        {
            if (CheckOpenOrEmpty())
            {
                return new WknGeometryCollection(new WknShape[0]);
            }

            var shapes = new List<WknShape>();
            for (; ; )
            {
                var shape = DecodeShape();
                shapes.Add(shape);

                var separator = GetNextToken();
                if (separator == TokenType.xCloseParenthesis) break;
                if (separator != TokenType.xComma) throw new Exception("Invalid text");
            }

            return new WknGeometryCollection(shapes.ToArray());
        }

        public WknShape DecodeShape()
        {
            var token = GetNextToken();
            switch(token)
            {
            case TokenType.POINT: return DecodePoint();
            case TokenType.LINESTRING: return DecodeLineString();
            case TokenType.POLYGON: return DecodePolygon();
            case TokenType.MULTIPOINT: return DecodeMultiPoint();
            case TokenType.MULTILINESTRING: return DecodeMultiLineString();
            case TokenType.MULTIPOLYGON: return DecodeMultiPolygon();
            case TokenType.GEOMETRYCOLLECTION: return DecodeGeometryCollection();
            }

            throw new Exception("Invalid text");
        }

    }
}
