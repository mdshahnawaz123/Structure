using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class CategoryExtn
    {
        public static IList<Category> GetCategories(this Document doc)
        {
            var categoriesList = new List<Category>();

            var cat = doc.Settings.Categories.Cast<Category>()
                .Where(c => c.CategoryType == CategoryType.Model)
                .Where(x=>!x.IsTagCategory)
                .ToList();
            foreach (var c in cat)
            {
                categoriesList.Add(c);
            }
            return categoriesList;

        }

    }
}
