using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Structure.Events;
using Structure.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Structure.UI
{
    public partial class DoorStiffenerUI : Window
    {
        private Document doc;
        private UIDocument uidoc;
        private ExternalEvent externalEvent;
        private DoorStiffenerHandler handler;
        private List<Reference> selectedDoors;

        public DoorStiffenerUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            handler = new DoorStiffenerHandler();
            externalEvent = ExternalEvent.Create(handler);

            info();
        }

        private void SelectDoor(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDoorType = DoorTypeComboBox.SelectedItem as FamilySymbol;
                if (selectedDoorType == null)
                {
                    TaskDialog.Show("Selection Error", "Please select a door type first.");
                    return;
                }

                selectedDoors = uidoc.Selection
                    .PickObjects(ObjectType.Element, new DoorSelection(selectedDoorType.Id), "Select matching doors")
                    .ToList();

                if (selectedDoors == null || selectedDoors.Count == 0)
                {
                    TaskDialog.Show("Door Selection Error", "No matching doors selected.");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Selection Error", $"Failed to select doors: {ex.Message}");
            }
        }

        private void CreateStiffener(object sender, RoutedEventArgs e)
        {
            try
            {
                var botLevel = BottomLevelComboBox.SelectedItem as Level;
                var topLevel = TopLevelComboBox.SelectedItem as Level;
                var colType = ColumnTypeComboBox.SelectedItem as FamilySymbol;
                var selectedDoorType = DoorTypeComboBox.SelectedItem as FamilySymbol;
                bool isStructural = ColumnCheckBox.IsChecked == true;

                if (!double.TryParse(OffsetInput.Text, out double offsetMm))
                {
                    TaskDialog.Show("Input Error", "Invalid offset value.");
                    return;
                }

                if (botLevel == null || topLevel == null || colType == null || selectedDoorType == null)
                {
                    TaskDialog.Show("Input Error", "Please select all inputs.");
                    return;
                }

                if (selectedDoors == null || selectedDoors.Count == 0)
                {
                    TaskDialog.Show("Selection Error", "No doors selected.");
                    return;
                }

                // Pass all values to the handler
                handler.doc = doc;
                handler.BottomLvel = botLevel;
                handler.TopLvel = topLevel;
                handler.familySymbol = colType;
                handler.offset = offsetMm;
                handler.UseStructuralColumn = isStructural;
                handler.SelDoor = selectedDoors;
                handler.DoorTypes = selectedDoorType;

                externalEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Create Stiffener Error", $"Failed to create stiffeners: {ex.Message}");
            }
        }

        public class DoorSelection : ISelectionFilter
        {
            private ElementId allowedTypeId;

            public DoorSelection(ElementId typeId)
            {
                allowedTypeId = typeId;
            }

            public bool AllowElement(Element elem)
            {
                return elem.Category != null &&
                       elem.Category.Id == new ElementId(BuiltInCategory.OST_Doors) &&
                       elem.GetTypeId() == allowedTypeId;
            }

            public bool AllowReference(Reference reference, XYZ position) => true;
        }

        public void info()
        {
            BottomLevelComboBox.Items.Clear();
            TopLevelComboBox.Items.Clear();
            ColumnTypeComboBox.Items.Clear();
            DoorTypeComboBox.Items.Clear();
            OffsetInput.Text = string.Empty;

            var levels = Extension.LevelExtension.GetLevels(doc);

            // Get both structural and architectural column types
            var allColumnTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .Where(fs => fs != null &&
                             (fs.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns) ||
                              fs.Category.Id == new ElementId(BuiltInCategory.OST_Columns)))
                .ToList();

            // Get door types
            var doorTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .Where(fs => fs != null)
                .ToList();

            BottomLevelComboBox.ItemsSource = levels;
            TopLevelComboBox.ItemsSource = levels;
            ColumnTypeComboBox.ItemsSource = allColumnTypes;
            DoorTypeComboBox.ItemsSource = doorTypes;

            BottomLevelComboBox.DisplayMemberPath = "Name";
            TopLevelComboBox.DisplayMemberPath = "Name";
            ColumnTypeComboBox.DisplayMemberPath = "Name";
            DoorTypeComboBox.DisplayMemberPath = "Name";
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
