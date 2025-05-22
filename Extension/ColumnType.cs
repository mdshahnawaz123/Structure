using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class ColumnType
    {
        public static IList<FamilySymbol> GetColumnTypes(this Document doc)
        {

            var columnTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsElementType()
                .ToElements()
                .Cast<FamilySymbol>()
                .Where(fs => fs != null && fs.IsActive)
                .ToList();

            return columnTypes;
        }
    }
}
