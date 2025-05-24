using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Structure.Events;

namespace Structure.UI
{
    public partial class PCCUI : Window
    {
        private Document doc;
        private UIDocument uidoc;
        private ExternalEvent externalEvent;
        private PCCHandler handler;

        private List<PlanarFace> selectedFaces = new();
        private List<Element> selectedFoundations = new();

        public PCCUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            handler = new PCCHandler();
            externalEvent = ExternalEvent.Create(handler);

            InitializeComboBox();
        }

        private void SelectFoundation_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var references = uidoc.Selection.PickObjects(ObjectType.Face, new FoundationAndSlabSelectionFilter());
                if (references == null || references.Count == 0) return;

                selectedFaces.Clear();
                selectedFoundations.Clear();

                foreach (var reference in references)
                {
                    var face = doc.GetElement(reference).GetGeometryObjectFromReference(reference) as PlanarFace;
                    if (face == null || !face.FaceNormal.Normalize().IsAlmostEqualTo(XYZ.BasisZ.Negate()))
                        continue;

                    selectedFaces.Add(face);
                    selectedFoundations.Add(doc.GetElement(reference));
                }

                TaskDialog.Show("Info", $"{selectedFaces.Count} bottom face(s) selected.");
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Selection failed: {exception.Message}");
            }
        }

        private void CreatePCC(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(PCCOfset.Text, out double offset))
                {
                    TaskDialog.Show("Input Error", "Please enter a valid numeric offset.");
                    return;
                }

                if (PCCType.SelectedItem is not FloorType selectedType)
                {
                    TaskDialog.Show("Selection Error", "Please select a PCC floor type.");
                    return;
                }

                if (selectedFaces.Count == 0)
                {
                    TaskDialog.Show("Selection Error", "No foundation faces selected.");
                    return;
                }

                handler.SetData(doc, selectedFaces, selectedFoundations, selectedType, offset);
                externalEvent.Raise();
            }
            catch (Exception exception)
            {
                TaskDialog.Show("PCC Error", $"Failed to create PCC: {exception.Message}");
            }
        }

        private void InitializeComboBox()
        {
            PCCOfset.Text = string.Empty;
            PCCType.Items.Clear();

            var foundationTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .WhereElementIsElementType()
                .Cast<FloorType>()
                .ToList();

            PCCType.ItemsSource = foundationTypes;
            PCCType.DisplayMemberPath = "Name";
            PCCType.SelectedIndex = 0;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        private class FoundationAndSlabSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem?.Category == null)
                    return false;

                // Safely access the BuiltInCategory directly
                BuiltInCategory? builtInCategory = elem.Category.BuiltInCategory;

                // Ensure it’s a known enum value
                if (!builtInCategory.HasValue)
                    return false;

                // Match only foundation and slab categories
                return builtInCategory == BuiltInCategory.OST_StructuralFoundation
                       || builtInCategory == BuiltInCategory.OST_Floors;
            }

            public bool AllowReference(Reference reference, XYZ position) => true;
        }
    }
}
