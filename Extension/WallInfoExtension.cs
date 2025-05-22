using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class WallInfoExtension
    {
        public static IList<WallType> GetWallType(this Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .WhereElementIsElementType()
                .ToElements()
                .Cast<WallType>()
                .ToList();
        }
    }
}
