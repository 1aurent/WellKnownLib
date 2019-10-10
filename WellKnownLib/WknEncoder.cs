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
using System.IO;

namespace WellKnownLib
{
    public class WknEncoder
    {
        const byte LittleEndian = 1;


        static void AppendPoint(BinaryWriter wrt, WknPoint point)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknPoint);
            wrt.Write(point.P.X);
            wrt.Write(point.P.Y);
        }

        static void AppendLineString(BinaryWriter wrt, WknLineString lineString)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknLineString);
            wrt.Write((uint)lineString.Points.Length);
            for (var i = 0; i < lineString.Points.Length; ++i)
            {
                var p = lineString.Points[i];
                wrt.Write(p.X);
                wrt.Write(p.Y);
            }
        }

        static void AppendPolygon(BinaryWriter wrt, WknPolygon polygon)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknPolygon);
            wrt.Write((uint)polygon.Rings.Length);
            for (var i = 0; i < polygon.Rings.Length; ++i)
            {
                var ring = polygon.Rings[i];
                wrt.Write((uint)ring.Points.Length);
                for (var j = 0; j < ring.Points.Length; ++j)
                {
                    wrt.Write(ring.Points[j].X);
                    wrt.Write(ring.Points[j].Y);
                }
            }
        }

        static void AppendMultiPoint(BinaryWriter wrt, WknMultiPoint mpoints)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknMultiPoint);
            wrt.Write((uint)mpoints.Points.Length);
            for (var i = 0; i < mpoints.Points.Length; ++i)
            {
                AppendPoint(wrt, mpoints.Points[i]);
            }
        }

        static void AppendMultiLineString(BinaryWriter wrt, WknMultiLineString mlinestrings)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknMultiLineString);
            wrt.Write((uint)mlinestrings.LineString.Length);
            for (var i = 0; i < mlinestrings.LineString.Length; ++i)
            {
                AppendLineString(wrt, mlinestrings.LineString[i]);
            }
        }

        static void AppendMultiPolygon(BinaryWriter wrt, WknMultiPolygon mpolygons)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknMultiPolygon);
            wrt.Write((uint)mpolygons.Polygons.Length);
            for (var i = 0; i < mpolygons.Polygons.Length; ++i)
            {
                AppendPolygon(wrt, mpolygons.Polygons[i]);
            }
        }

        static void AppendGeometryCollection(BinaryWriter wrt, WknGeometryCollection shapes)
        {
            wrt.Write(LittleEndian);
            wrt.Write((uint)WknGeometryType.WknGeometryCollection);
            wrt.Write((uint)shapes.Shapes.Length);
            for (var i = 0; i < shapes.Shapes.Length; ++i)
            {
                AppendShape(wrt, shapes.Shapes[i]);
            }
        }

        static public void AppendShape(BinaryWriter wrt, WknShape shape)
        {
            switch (shape.Type)
            {
                case WknGeometryType.WknPoint:
                    AppendPoint(wrt, (WknPoint)shape);
                    break;
                case WknGeometryType.WknLineString:
                    AppendLineString(wrt, (WknLineString)shape);
                    break;
                case WknGeometryType.WknPolygon:
                    AppendPolygon(wrt, (WknPolygon)shape);
                    break;
                case WknGeometryType.WknMultiPoint:
                    AppendMultiPoint(wrt, (WknMultiPoint)shape);
                    break;
                case WknGeometryType.WknMultiLineString:
                    AppendMultiLineString(wrt, (WknMultiLineString)shape);
                    break;
                case WknGeometryType.WknMultiPolygon:
                    AppendMultiPolygon(wrt, (WknMultiPolygon)shape);
                    break;
                case WknGeometryType.WknGeometryCollection:
                    AppendGeometryCollection(wrt, (WknGeometryCollection)shape);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        static public byte[] ConvertToBinary(WknShape shape)
        {
            using (var mem = new MemoryStream())
            {
                using (var wrt = new BinaryWriter(mem))
                {
                    AppendShape(wrt, shape);
                }

                return mem.ToArray();
            }
        }

        static public string ConvertToText(WknShape shape)
        {
            return shape.ToString();
        }

    }
}
