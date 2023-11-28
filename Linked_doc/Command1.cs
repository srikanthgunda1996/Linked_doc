#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using forms = System.Windows.Forms;
using System.Windows.Forms;

#endregion

namespace Linked_doc
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;
            FilteredElementCollector rvtlinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkType));

            Document linkeddoc = null;
            RevitLinkInstance rvtlinkInstance = null;



            foreach(RevitLinkType revitlnk in rvtlinks)
            {
                if(revitlnk.GetLinkedFileStatus() == LinkedFileStatus.Loaded && revitlnk.Name == "RAA_Int_Module_04_Sample 01_2022.rvt")
                {

                   rvtlinkInstance = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkInstance)).Where(x => x.GetTypeId() == revitlnk.Id).First() as RevitLinkInstance;
                   linkeddoc = rvtlinkInstance.GetLinkDocument();
                }
            }

            FilteredElementCollector linkelements = new FilteredElementCollector(linkeddoc).OfCategory(BuiltInCategory.OST_Rooms);


            Level docLevel = doc.ActiveView.GenLevel;
            using(Transaction t = new Transaction(doc, "Linked doc"))
            {
                t.Start();
                List<SpatialElement> docspaces = new List<SpatialElement>();
                int count = 0;
                foreach (Room curRoom in linkelements)
                {
                    LocationPoint curLoc = curRoom.Location as LocationPoint;

                    SpatialElement curSpace = doc.Create.NewSpace(docLevel, new UV(curLoc.Point.X, curLoc.Point.Y));
                    Parameter curSpacecomm = curSpace.LookupParameter("Comments");
                    curSpace.Name = curRoom.Name;
                    curSpace.Number = curRoom.Number;
                    curSpacecomm.Set(curRoom.LookupParameter("Comments").AsString());
                    docspaces.Add(curSpace);
                    count++;
                }
                TaskDialog.Show("Spaces", $"No. of rooms got copied are {count}");
                t.Commit();
            }
            
            // Your code goes here


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
