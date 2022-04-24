using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
        public Result Execute(ExternalCommandData _commandData, ref string message, ElementSet elements)
        {
            try
            {
                GroupPickFilter groupPickFilter = new GroupPickFilter();

                UIApplication uiapp = _commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, groupPickFilter, "Selected group elements");
                Element element = doc.GetElement(reference);
                Group group = element as Group;

                XYZ groupCenter = GetElementCenter(group);
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;

                XYZ point = uidoc.Selection.PickPoint("Select point");
                Room roomByPoint = GetRoomByPoint(doc, point);
                XYZ roomPointCenter = GetElementCenter(roomByPoint);
                XYZ insertCenter = offset + roomPointCenter;
                Transaction ts = new Transaction(doc);
                ts.Start("Copy Groups");
                doc.Create.PlaceGroup(insertCenter, group.GroupType);
                ts.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public XYZ GetElementCenter (Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }

        public Room GetRoomByPoint (Document doc, XYZ point)
        {
            FilteredElementCollector filter = new FilteredElementCollector(doc);
            filter.OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element element in filter)
            {
                Room room = element as Room;
                if (room!=null)
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
            }
            return null;
        }
    }
    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
            return true;
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
