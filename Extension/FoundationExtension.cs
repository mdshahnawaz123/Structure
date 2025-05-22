using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Extension
{
    public static class FoundationExtension 
    {
        public static IList<Element> GetFoundation(this Document doc)
        {
            IList<Element> foundationElements = new List<Element>();
            foundationElements.Clear();
            try
            {
                var collector = new FilteredElementCollector(doc);
                foundationElements = collector
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                    .WhereElementIsNotElementType()
                    .ToList();
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", $"Foundation did not found {e.Message}");
            }
            return foundationElements;
        }

        public static IList<Element> GetCurrentFoundation(this Document doc)
        {
            List<Element> foundationElements = new List<Element>();

            foundationElements.Clear();

            var activeView = doc.ActiveView;
            try
            {

                var fdn = new FilteredElementCollector(doc,activeView.Id)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                    .WhereElementIsNotElementType()
                    .ToList();

                foundationElements.AddRange(fdn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return foundationElements;
        }

        public static IList<Element> GetSelectedFoundation(this Document doc)
        {
            List<Element> founElements = new List<Element>();

            try
            {
                



            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return founElements;
        }

        public static IList<FloorType> GetFloorTypes(this Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .WhereElementIsElementType()
                .Cast<FloorType>()
                .ToList();
        }
    }
}
