﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DArbitraryCoordinate.Utils
{
    public static class PromptUtils
    {
        public static Point3d? PromptPoint(string promptMessage)
        {
            // Get the current database and start the Transaction Manager
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the start point
            pPtOpts.Message = $"\n{promptMessage}: ";

            Point3d? point3d = null;
            using (AutocadDocumentService.LockActiveDocument())
            {
                PromptPointResult pPtRes = AutocadDocumentService.Editor.GetPoint(pPtOpts);
                point3d = pPtRes.Value;

                if (pPtRes.Status == PromptStatus.Cancel)
                {
                    return null;
                }
            }

            return point3d;
        }
    }
}
