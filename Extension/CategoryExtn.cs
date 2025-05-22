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
        public static IList<Category> GetCategories(this Document doc, Categories categories)
        {
            var categoriesList = new List<Category>();

            var cat = doc.Settings.Categories;

            return null;

        }

    }
}
