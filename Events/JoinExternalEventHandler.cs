﻿using System.Diagnostics;
using System.Windows.Navigation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElementJoin
{
    public class JoinExternalEventHandler : IExternalEventHandler
    {
        public Document Doc;
        public Category Category1;
        public Category Category2;

        public void Execute(UIApplication app)
        {
            if (Doc == null || Category1 == null || Category2 == null)
                return;

            var elements1 = new FilteredElementCollector(Doc)
                .OfCategoryId(Category1.Id)
                .WhereElementIsNotElementType()
                .ToElements();

            var elements2 = new FilteredElementCollector(Doc)
                .OfCategoryId(Category2.Id)
                .WhereElementIsNotElementType()
                .ToElements();

            using (Transaction tx = new Transaction(Doc, "Join Elements"))
            {
                tx.Start();

                foreach (var element1 in elements1)
                {
                    foreach (var element2 in elements2)
                    {
                        try
                        {
                            if (!JoinGeometryUtils.AreElementsJoined(Doc, element1, element2))
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(Doc, element1, element2))
                                {
                                    JoinGeometryUtils.JoinGeometry(Doc, element1, element2);
                                }
                            }
                        }
                        catch
                        {
                            // Optional: handle join failure
                        }
                    }
                }

                tx.Commit();
            }
        }

        public string GetName()
        {
            return "Join Elements External Event";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

}
}