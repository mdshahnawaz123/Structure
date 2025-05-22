using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class FamilyInstance
    {
        public static IList<T> GetFamilySymbols<T>(this Document doc,BuiltInCategory category)
        where T : FamilySymbol
        {
            var collector = new FilteredElementCollector(doc)
                .OfCategory(category)
                .OfClass(typeof(T))
                .Cast<T>()
                .ToList();
            return collector;
        }
    }
}
