using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICopyGroup
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Selected group elements");
            Element element = doc.GetElement(reference);
            Group group = element as Group;

            XYZ point = uidoc.Selection.PickPoint("Select point");

            Transaction ts = new Transaction(doc);
            ts.Start();
            doc.Create.PlaceGroup(point, group.GroupType);
            ts.Commit();

            return Result.Succeeded;
        }
    }
}
