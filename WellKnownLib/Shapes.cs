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
using System.Text;

namespace WellKnownLib
{
    public class Point
    {
        public Point(double x, double y) { X = x; Y = y; }

        public double X { get; private set; }
        public double Y { get; private set; }


        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1}", X, Y);
        }
    }

    public class LinearRing
    {
        public LinearRing(Point[] points) { Points = points; }
        public Point[] Points { get; private set; }
    }


    public enum WknGeometryType : uint
    {
        WknPoint = 1,
        WknLineString = 2,
        WknPolygon = 3,
        WknMultiPoint = 4,
        WknMultiLineString = 5,
        WknMultiPolygon = 6,
        WknGeometryCollection = 7
    };

    public abstract class WknShape
    {
        public abstract WknGeometryType Type { get; }
    }

    public class WknPoint : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknPoint; } }

        public WknPoint(Point p) { P = p; }
        public Point P { get; private set; }

        public override string ToString()
        {
            return $"POINT( {P} )";
        }
    }

    public class WknLineString : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknLineString; } }

        public WknLineString(Point[] points) { Points = points; }
        public Point[] Points { get; private set; }

        public override string ToString()
        {
            if (Points.Length == 0) return "LINESTRING EMPTY";

            var sb = new StringBuilder("LINESTRING(");

            for(var i=0;i< Points.Length;++i)
            {
                if (i != 0) sb.Append(",");
                sb.Append(Points[i]);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class WknPolygon : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknPolygon; } }

        public WknPolygon(LinearRing[] rings) { Rings = rings; }
        public LinearRing[] Rings { get; private set; }

        public override string ToString()
        {
            if (Rings.Length == 0) return "POLYGON EMPTY";

            var sb = new StringBuilder("POLYGON(");

            for (var j = 0; j < Rings.Length; ++j)
            {
                if (j != 0) sb.Append(",");
                var points = Rings[j].Points;
                sb.Append("(");
                for (var i = 0; i < points.Length; ++i)
                {
                    if (i != 0) sb.Append(",");
                    sb.Append(points[i]);
                }
                sb.Append(")");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class WknMultiPoint : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknMultiPoint; } }

        public WknMultiPoint(WknPoint[] points) { Points = points; }
        public WknPoint[] Points { get; private set; }


        public override string ToString()
        {
            if (Points.Length == 0) return "MULTIPOINT EMPTY";

            var sb = new StringBuilder("MULTIPOINT(");

            for (var i = 0; i < Points.Length; ++i)
            {
                if (i != 0) sb.Append(",");
                sb.Append(Points[i].P);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class WknMultiLineString : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknMultiLineString; } }

        public WknMultiLineString(WknLineString[] lineString) { LineString = lineString; }
        public WknLineString[] LineString { get; private set; }

        public override string ToString()
        {
            if (LineString.Length == 0) return "MULTILINESTRING EMPTY";

            var sb = new StringBuilder("MULTILINESTRING(");

            for (var j = 0; j < LineString.Length; ++j)
            {
                if (j != 0) sb.Append(",");
                sb.Append("(");
                var points = LineString[j].Points;
                for (var i = 0; i < points.Length; ++i)
                {
                    if (i != 0) sb.Append(",");
                    sb.Append(points[i]);
                }
                sb.Append(")");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class WknMultiPolygon : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknMultiPolygon; } }

        public WknMultiPolygon(WknPolygon[] polygons) { Polygons = polygons; }
        public WknPolygon[] Polygons { get; private set; }

        public override string ToString()
        {
            if (Polygons.Length == 0) return "MULTIPOLYGON EMPTY";

            var sb = new StringBuilder("MULTIPOLYGON(");

            for (var k = 0; k < Polygons.Length; ++k)
            {
                if (k != 0) sb.Append(",");
                sb.Append("(");
                var rings = Polygons[k].Rings;
                for (var j = 0; j < rings.Length; ++j)
                {
                    if (j != 0) sb.Append(",");
                    sb.Append("(");
                    var points = rings[j].Points;
                    for (var i = 0; i < points.Length; ++i)
                    {
                        if (i != 0) sb.Append(",");
                        sb.Append(points[i]);
                    }
                    sb.Append(")");
                }
                sb.Append(")");
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class WknGeometryCollection : WknShape
    {
        public override WknGeometryType Type { get { return WknGeometryType.WknGeometryCollection; } }

        public WknGeometryCollection(WknShape[] shapes) { Shapes = shapes; }
        public WknShape[] Shapes { get; private set; }


        public override string ToString()
        {
            if (Shapes.Length == 0) return "GEOMETRYCOLLECTION EMPTY";

            var sb = new StringBuilder("GEOMETRYCOLLECTION(");
            for(var i=0;i<Shapes.Length;++i)
            {
                if (i != 0) sb.Append(",");
                sb.Append(Shapes[i].ToString());
            }
            sb.Append(")");

            return sb.ToString();
        }

    }

}
