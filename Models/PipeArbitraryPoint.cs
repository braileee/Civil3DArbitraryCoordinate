using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Civil3DArbitraryCoordinate.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public Point3d PipeOuterBottomPoint
        {
            get
            {
                return new Point3d(PipeCenterPoint.X, PipeCenterPoint.Y, PipeCenterPoint.Z - Pipe.OuterDiameterOrHeight() / 2);
            }
        }

        public Point3d PipeInnerBottomPoint
        {
            get
            {
                return new Point3d(PipeCenterPoint.X, PipeCenterPoint.Y, PipeCenterPoint.Z - Pipe.InnerDiameterOrHeight() / 2);
            }
        }

        public Point3d PipeInnerTopPoint
        {
            get
            {
                return new Point3d(PipeCenterPoint.X, PipeCenterPoint.Y, PipeCenterPoint.Z + Pipe.InnerDiameterOrHeight() / 2);
            }
        }

        public Point3d PipeOuterTopPoint
        {
            get
            {
                return new Point3d(PipeCenterPoint.X, PipeCenterPoint.Y, PipeCenterPoint.Z + Pipe.OuterDiameterOrHeight() / 2);
            }
        }

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

        public double GetElevationValue(int accurracy, ElevationType elevationType)
        {
            switch (elevationType)
            {
                case ElevationType.OuterBottom:
                    return Math.Round(PipeOuterBottomPoint.Z, accurracy);
                case ElevationType.InnerBottom:
                    return Math.Round(PipeInnerBottomPoint.Z, accurracy);
                case ElevationType.Center:
                    return Math.Round(PipeCenterPoint.Z, accurracy);
                case ElevationType.InnerTop:
                    return Math.Round(PipeInnerTopPoint.Z, accurracy);
                case ElevationType.OuterTop:
                    return Math.Round(PipeOuterTopPoint.Z, accurracy);
                default:
                    break;
            }

            return 0;
        }
    }
}
