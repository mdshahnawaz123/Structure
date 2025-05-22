using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Structure.UI
{
    public partial class PileCoordinatesUI : Window
    {
        private UIDocument? uidoc;
        private Document? doc;

        public PileCoordinatesUI(UIDocument uidoc)
        {
            InitializeComponent();
            this.uidoc = uidoc;
            this.doc = uidoc.Document;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            CreateAndBindSharedParameters(doc); // Ensure shared parameters exist

            List<Element> piles = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .WhereElementIsNotElementType()
                .ToList();

            // ✅ Get accurate project base point transform
            ProjectPosition projPos = doc.ActiveProjectLocation.GetProjectPosition(XYZ.Zero);
            Transform projectTransform = Transform.CreateTranslation(new XYZ(projPos.EastWest, projPos.NorthSouth, projPos.Elevation));

            // ✅ Get survey point transform
            Transform surveyTransform = doc.ActiveProjectLocation.GetTotalTransform();

            using (Transaction tx = new Transaction(doc, "Set Pile Coordinates"))
            {
                tx.Start();

                foreach (Element element in piles)
                {
                    if (element.Location is LocationPoint loc)
                    {
                        XYZ ptInternal = loc.Point;
                        XYZ ptProject = projectTransform.OfPoint(ptInternal);
                        XYZ ptSurvey = surveyTransform.OfPoint(ptInternal);

                        double xInt = UnitUtils.ConvertFromInternalUnits(ptInternal.X, UnitTypeId.Meters);
                        double yInt = UnitUtils.ConvertFromInternalUnits(ptInternal.Y, UnitTypeId.Meters);

                        double xProj = UnitUtils.ConvertFromInternalUnits(ptProject.X, UnitTypeId.Meters);
                        double yProj = UnitUtils.ConvertFromInternalUnits(ptProject.Y, UnitTypeId.Meters);

                        double xSurv = UnitUtils.ConvertFromInternalUnits(ptSurvey.X, UnitTypeId.Meters);
                        double ySurv = UnitUtils.ConvertFromInternalUnits(ptSurvey.Y, UnitTypeId.Meters);

                        SetParam(element, "Pile_X_Internal", xInt);
                        SetParam(element, "Pile_Y_Internal", yInt);
                        SetParam(element, "Pile_X_Project", xProj);
                        SetParam(element, "Pile_Y_Project", yProj);
                        SetParam(element, "Pile_X_Survey", xSurv);
                        SetParam(element, "Pile_Y_Survey", ySurv);
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show("Done", "Pile coordinates updated.");
            this.Close();
        }

        private void SetParam(Element el, string paramName, double value)
        {
            Parameter p = el.LookupParameter(paramName);
            if (p != null && !p.IsReadOnly)
            {
                p.Set(value);
            }
        }

        private void CreateAndBindSharedParameters(Document doc)
        {
             var app = doc.Application;
            DefinitionFile defFile = app.OpenSharedParameterFile();

            if (defFile == null)
            {
                TaskDialog.Show("Error", "Shared parameter file not set.");
                return;
            }

            DefinitionGroup group = defFile.Groups.get_Item("PileParameters") ?? defFile.Groups.Create("PileParameters");

            string[] names = new string[]
            {
                "Pile_X_Internal", "Pile_Y_Internal",
                "Pile_X_Project", "Pile_Y_Project",
                "Pile_X_Survey", "Pile_Y_Survey"
            };

            CategorySet catSet = app.Create.NewCategorySet();
            catSet.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_StructuralFoundation));

            InstanceBinding binding = app.Create.NewInstanceBinding(catSet);
            BindingMap map = doc.ParameterBindings;

            using (Transaction tx = new Transaction(doc, "Bind Shared Parameters"))
            {
                tx.Start();

                foreach (string name in names)
                {
                    Definition def = group.Definitions.get_Item(name);
                    if (def == null)
                    {
                        ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(name, SpecTypeId.Number);
                        def = group.Definitions.Create(options);
                    }

                    if (!map.Contains(def))
                        map.Insert(def, binding, GroupTypeId.IdentityData);
                }

                tx.Commit();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
