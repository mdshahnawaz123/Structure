using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Structure
{
    public static class PileExtension
    {
        public static List<Element> GetElements(this Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .WhereElementIsNotElementType()
                .ToList();
        }
    }
}