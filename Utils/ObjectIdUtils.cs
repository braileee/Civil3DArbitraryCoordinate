using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace Civil3DArbitraryCoordinate.Utils
{
    public static class ObjectIdUtils<T>
    {
        public static List<T> ConvertToType(List<ObjectId> objectIds, Transaction transaction)
        {
            List<T> elements = new List<T>();

            foreach (ObjectId objectId in objectIds)
            {
                System.Type type = typeof(T);
                if (objectId.ObjectClass.DxfName.Equals(Autodesk.AutoCAD.Runtime.RXObject.GetClass(type).DxfName))
                {
                    T element = (T)(object)transaction.GetObject(objectId, OpenMode.ForWrite);
                    elements.Add(element);
                }
            }

            return elements;
        }
    }
}
