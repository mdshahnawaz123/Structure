using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Private.InfoCenter;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Structure.Command
{
    public class SlabDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            


            //Let try to put the dimension on the slab

            var sel = uidoc.Selection.PickObjects(ObjectType.Face, new foundationselection(),
                "Select the Foundation Slab");

            var faces = sel.Select(x => doc.GetElement(x)).OfType<Face>().ToList();
            return Result.Succeeded;
        }

        public class foundationselection : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                // Allow structural floors marked as foundation
                if (elem is Floor floor)
                {
                    // Check if it's a structural floor AND a foundation
                    return floor.FloorType.IsFoundationSlab && floor.FloorType.Name.ToLower().Contains("foundation");
                }

                // Allow structural foundation family instances (like isolated footings)
                if (elem is FamilyInstance fi &&
                    fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralFoundation)
                {
                    return true;
                }

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true; // Allows face or edge reference
            }
        }
    }
}
