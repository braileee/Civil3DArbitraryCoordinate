using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DArbitraryCoordinate
{
    public static class LabelExtesions
    {
        public static void SetTextComponentOverride(this Label label, Transaction transaction, string oldText, string newText)
        {
            if (label == null)
            {
                return;
            }


            LabelStyle labelStyle = transaction.GetObject(label.StyleId, OpenMode.ForWrite, false, true) as LabelStyle;
            ObjectIdCollection componentIdCollection = labelStyle.GetComponents(LabelStyleComponentType.Text);

            Dictionary<ObjectId, string> componentIdsPerTextDictionary = new Dictionary<ObjectId, string>();

            foreach (ObjectId componentId in componentIdCollection)
            {
                LabelStyleTextComponent labelStyleTextComponent = transaction.GetObject(componentId, OpenMode.ForWrite, false, true) as LabelStyleTextComponent;

                if (labelStyleTextComponent.Text.Contents.Value.Contains(oldText))
                {
                    if (!componentIdsPerTextDictionary.ContainsKey(componentId))
                    {
                        componentIdsPerTextDictionary.Add(componentId, labelStyleTextComponent.Text.Contents.Value);
                    }

                }
            }

            foreach (KeyValuePair<ObjectId, string> componentIdPerText in componentIdsPerTextDictionary)
            {
                string text = componentIdPerText.Value.Replace(oldText, newText);
                label.SetTextComponentOverride(componentIdPerText.Key, text);
            }
        }
    }
}
