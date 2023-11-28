#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;

#endregion

namespace Linked_doc
{
    [Transaction(TransactionMode.Manual)]
    public class Command3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;
            Document openedDoc = null;

            foreach (Document curDoc in uiapp.Application.Documents)
            {
                if(curDoc.PathName.Contains("RAA_Int_Module_04_Sample 03_2022"))
                {
                    openedDoc = curDoc as Document;
                    break;
                }
            }
            List<BuiltInCategory> categories = new List<BuiltInCategory>();
            categories.Add(BuiltInCategory.OST_Walls);
            categories.Add(BuiltInCategory.OST_GenericModel);
            ElementMulticategoryFilter multicategory = new ElementMulticategoryFilter(categories);
            FilteredElementCollector elemnts = new FilteredElementCollector(openedDoc).WherePasses(multicategory).WhereElementIsNotElementType();

            Transaction t = new Transaction(doc);
                {
                    t.Start("Insert Walls & Generic Models");
                    List<ElementId> elemids = elemnts.Select(e => e.Id).ToList();
                    ElementTransformUtils.CopyElements(openedDoc, elemids,doc,null,null);
                    TaskDialog.Show("count of walls & generic models", $"No. of Walls & Generic Models {elemnts.Count()}");

                    t.Commit();
                    t.Dispose();
                }

                return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
