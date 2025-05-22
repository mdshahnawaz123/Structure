using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Extension
{
    public class LevelExtension
    {
        public static IList<Level> GetLevels(Document doc)
        {

            IList<Level> levels = new List<Level>();

            try
            {
                var lvl = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Level>()
                    .ToList();
                foreach (var level in lvl)
                {
                    levels.Add(level);
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Error",$"Level did not found {e.Message}");
            }

            return levels;
        }

    }
}
