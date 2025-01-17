using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DArbitraryCoordinate.Models
{
    public class PipeArbitraryPoint
    {
        private PipeArbitraryPoint()
        {
        }

        public Pipe Pipe { get; private set; }
        public Point2d PickedPoint { get; private set; }
        public Point3d PipeCenterPoint { get; private set; }
        public double RatioFromStartPoint { get; private set; }

        public static PipeArbitraryPoint Create(Pipe pipe, Point2d pickedPoint)
        {
            Point2d pipeProjectedPoint = pipe.GetClosestPoint2d(pickedPoint);
            double ratio = pipe.GetDistance2dRatioFromStart(pipeProjectedPoint);
            double elevation = pipe.GetElevationAtPoint(pipeProjectedPoint);

            Point3d pipeProjectedPoint3d = new Point3d(pipeProjectedPoint.X, pipeProjectedPoint.Y, elevation);

            return new PipeArbitraryPoint
            {
                Pipe = pipe,
                PickedPoint = pickedPoint,
                PipeCenterPoint = pipeProjectedPoint3d,
                RatioFromStartPoint = ratio
            };
        }
    }
}
