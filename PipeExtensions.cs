using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DArbitraryCoordinate
{
    public static class PipeExtensions
    {
        public static Point3d GetClosestPoint(this Pipe pipe, Point3d point)
        {
            Point2d point2d = new Point2d(point.X, point.Y);
            Point3d projectedPoint = pipe.GetClosestPointTo(point, extend: true);

            return projectedPoint;
        }

    }
}
