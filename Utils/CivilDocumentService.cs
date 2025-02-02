﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

namespace Civil3DArbitraryCoordinate.Utils
{
    public static class CivilDocumentService
    {
        public static CivilDocument CivilDocument
        {
            get
            {
                Database database = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
                return CivilDocument.GetCivilDocument(database);
            }
        }

    }
}
