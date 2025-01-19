using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Civil3DArbitraryCoordinate
{
    public static class PipeExtensions
    {
        public static Point2d GetClosestPoint2d(this Pipe pipe, Point2d point)
        {
            Point2d pipeStartPoint2d = new Point2d(pipe.StartPoint.X, pipe.StartPoint.Y);
            Point2d pipeEndPoint2d = new Point2d(pipe.EndPoint.X, pipe.EndPoint.Y);

            Vector2d pipeVector = pipeStartPoint2d.GetVectorTo(pipeEndPoint2d);
            Vector2d pointVector = pipeStartPoint2d.GetVectorTo(point);

            double scalarProjection = pointVector.DotProduct(pipeVector) / pipeVector.LengthSqrd;

            Point2d projectedPoint = pipeStartPoint2d + (pipeVector * scalarProjection);

            return projectedPoint;
        }

        public static double GetDistance2dRatioFromStart(this Pipe pipe, Point2d pointToCheck)
        {
            Point3d startPoint = pipe.StartPoint;
            Point3d endPoint = pipe.EndPoint;

            Point2d startPoint2d = new Point2d(startPoint.X, startPoint.Y);
            Point2d endPoint2d = new Point2d(endPoint.X, endPoint.Y);

            double totalLength2d = startPoint2d.GetDistanceTo(endPoint2d);
            double checkedLength = startPoint2d.GetDistanceTo(pointToCheck);

            if (checkedLength > totalLength2d)
            {
                return 1;
            }

            return checkedLength / totalLength2d;
        }

        public static double GetElevationAtPoint(this Pipe pipe, Point2d pointToCheck)
        {
            double ratio = GetDistance2dRatioFromStart(pipe, pointToCheck);

            Point2d startPoint2d = new Point2d(pipe.StartPoint.X, pipe.StartPoint.Y);
            Point2d endPoint2d = new Point2d(pipe.EndPoint.X, pipe.EndPoint.Y);

            double elevation = pipe.StartPoint.Z + ratio * (pipe.EndPoint.Z - pipe.StartPoint.Z);

            return elevation;
        }

        public static double OuterDiameterOrHeight(this Pipe oPipe)
        {
            if(oPipe == null)
            {
                return 0;
            }

            if (oPipe.CrossSectionalShape == SweptShapeType.Circular)
            {
                return oPipe.OuterDiameterOrWidth;
            }
            else
            {
                return oPipe.OuterHeight;
            }
        }

        public static double InnerDiameterOrHeight(this Pipe oPipe)
        {
            if (oPipe == null)
            {
                return 0;
            }

            if (oPipe.CrossSectionalShape == SweptShapeType.Circular)
            {
                return oPipe.InnerDiameterOrWidth;
            }
            else
            {
                return oPipe.InnerHeight;
            }
        }
    }
}
