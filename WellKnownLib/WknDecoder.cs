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

namespace WellKnownLib
{
    public static class WknDecoder
    {
        static WknPoint ParsePoint(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknPoint) throw new Exception("Invalid object type");
            pos += 5;

            var point = new Point(
                BitConverter.ToDouble(wkb, pos),
                BitConverter.ToDouble(wkb, pos + 8)
            );
            pos += 16;
            return new WknPoint(point);
        }

        static WknLineString ParseLineString(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknLineString) throw new Exception("Invalid object type");
            var nbPoints = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var points = new Point[nbPoints];
            for (var i = 0; i < nbPoints; ++i)
            {
                points[i] = new Point(
                    BitConverter.ToDouble(wkb, pos),
                    BitConverter.ToDouble(wkb, pos + 8)
                );
                pos += 16;
            }

            return new WknLineString(points);
        }

        static WknPolygon ParsePolygon(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknPolygon) throw new Exception("Invalid object type");
            var nbRings = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var rings = new LinearRing[nbRings];
            for (var r = 0; r < nbRings; ++r)
            {
                var nbPoints = BitConverter.ToUInt32(wkb, pos); pos += 4;
                var points = new Point[nbPoints];
                for (var i = 0; i < nbPoints; ++i)
                {
                    points[i] = new Point(
                        BitConverter.ToDouble(wkb, pos),
                        BitConverter.ToDouble(wkb, pos + 8)
                    );
                    pos += 16;
                }
                rings[r] = new LinearRing(points);
            }
            return new WknPolygon(rings);
        }

        static WknMultiPoint ParseMultiPoint(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknMultiPoint) throw new Exception("Invalid object type");
            var nbPoints = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var points = new WknPoint[nbPoints];
            for (var i = 0; i < nbPoints; ++i)
            {
                points[i] = ParsePoint(wkb, ref pos);
            }

            return new WknMultiPoint(points);
        }

        static WknMultiLineString ParseMultiLineString(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknMultiLineString) throw new Exception("Invalid object type");
            var nbLineStrings = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var lineStrings = new WknLineString[nbLineStrings];
            for (var i = 0; i < nbLineStrings; ++i)
            {
                lineStrings[i] = ParseLineString(wkb, ref pos);
            }

            return new WknMultiLineString(lineStrings);
        }

        static WknMultiPolygon ParseMultiPolygon(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknMultiPolygon) throw new Exception("Invalid object type");
            var nbPolygons = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var polygons = new WknPolygon[nbPolygons];
            for (var r = 0; r < nbPolygons; ++r)
            {
                polygons[r] = ParsePolygon(wkb, ref pos);
            }

            return new WknMultiPolygon(polygons);
        }

        static WknGeometryCollection ParseGeometryCollection(byte[] wkb, ref int pos)
        {
            if (wkb[pos] != 1) throw new Exception("Sorry, only Little Endian format is supported");
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            if (type != (uint)WknGeometryType.WknGeometryCollection) throw new Exception("Invalid object type");
            var nbShapes = BitConverter.ToUInt32(wkb, pos + 5);
            pos += 9;

            var shapes = new WknShape[nbShapes];
            for (var r = 0; r < nbShapes; ++r)
            {
                shapes[r] = ParseShape(wkb, ref pos);
            }

            return new WknGeometryCollection(shapes);
        }


        static public WknShape ParseShape(byte[] wkb, ref int pos)
        {
            var type = BitConverter.ToUInt32(wkb, pos + 1);
            switch (type)
            {
                case (uint)WknGeometryType.WknPoint:
                    return ParsePoint(wkb, ref pos);
                case (uint)WknGeometryType.WknLineString:
                    return ParseLineString(wkb, ref pos);
                case (uint)WknGeometryType.WknPolygon:
                    return ParsePolygon(wkb, ref pos);

                case (uint)WknGeometryType.WknMultiPoint:
                    return ParseMultiPoint(wkb, ref pos);
                case (uint)WknGeometryType.WknMultiLineString:
                    return ParseMultiLineString(wkb, ref pos);
                case (uint)WknGeometryType.WknMultiPolygon:
                    return ParseMultiPolygon(wkb, ref pos);

                case (uint)WknGeometryType.WknGeometryCollection:
                    return ParseGeometryCollection(wkb, ref pos);

                default:
                    throw new Exception("Unsupported type");
            }
        }

        /// <summary>
        /// Parse a Well Known Binary Blob and returns a set of Shapes
        /// </summary>
        /// <param name="wkb"></param>
        /// <returns></returns>
        static public WknShape ParseFromBinary(byte[] wkb)
        {
            var pos = 0;
            return ParseShape(wkb, ref pos);
        }

        static public WknShape ParseFromText(string wkt)
        {
            var parser = new WktParser(wkt);
            return parser.DecodeShape();
        }
    }
}
