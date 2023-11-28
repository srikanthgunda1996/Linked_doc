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

#endregion

namespace Linked_doc
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            FilteredElementCollector docspaces = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Revit File";
            openFileDialog.Filter = "Revit files (*.rvt)|*.rvt";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                TaskDialog.Show("Selection", "No file selected");
            string revitFile = openFileDialog.FileName;
            UIDocument selecteddoc = uiapp.OpenAndActivateDocument(revitFile);
            Document Uidoc = selecteddoc.Document;
            List<GroupType> groups = new FilteredElementCollector(Uidoc).OfCategory(BuiltInCategory.OST_IOSModelGroups).Cast<GroupType>().ToList();


            Transaction t = new Transaction(doc);
                {
                    t.Start("Insert Group");
                    List<ElementId> groupids = groups.Select(e => e.Id).ToList();
                    ElementTransformUtils.CopyElements(Uidoc, groupids,doc,null,null);
                    List<GroupType> curgroups = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_IOSModelGroups).Cast<GroupType>().ToList();
                    int count = 0;
                    foreach (SpatialElement curSpace in docspaces)
                    {
                        LocationPoint curLoc = curSpace.Location as LocationPoint;
                        XYZ insPoint = new XYZ(curLoc.Point.X, curLoc.Point.Y, curLoc.Point.Z);
                        foreach (GroupType group in curgroups)
                        {
                            if (group.Name == curSpace.LookupParameter("Comments").AsString())
                            {
                                doc.Create.PlaceGroup(insPoint, group);
                                count++;
                            }

                        }
                    }


                    t.Commit();
                    uiapp.OpenAndActivateDocument(doc.PathName);
                    Uidoc.Close();
                    TaskDialog.Show("Groups", $"No.of Groups got copied are {count}");
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
