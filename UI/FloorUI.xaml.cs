using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Structure.Events;
using Structure.Extension;

namespace Structure.UI
{
    public partial class FloorUI : Window
    {
        private readonly Document doc;
        private readonly UIDocument uidoc;
        private readonly ExternalEvent? externalEvent;
        private readonly FloorHandler? handler;
        private ImportInstance? selectedImportInstance;

        public FloorUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            handler = new FloorHandler();
            externalEvent = ExternalEvent.Create(handler);

            info();
        }

        private void CreateSlab_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SlabTypeComboBox.SelectedItem == null ||
                    ReferenceLevelComboBox.SelectedItem == null ||
                    LayerNameComboBox.SelectedItem == null)
                {
                    TaskDialog.Show("Error", "Please select all required inputs.");
                    return;
                }

                var selectedLayer = LayerNameComboBox.SelectedItem.ToString();
                var selectedSlabType = SlabTypeComboBox.SelectedItem as FloorType;
                var selectedLevel = ReferenceLevelComboBox.SelectedItem as Level;

                if (handler == null || externalEvent == null || selectedImportInstance == null)
                {
                    TaskDialog.Show("Error", "Missing handler or CAD selection.");
                    return;
                }

                // Filter PolyLines manually by layer
                var cadHelper = new CadGeometry();
                var allPolylines = cadHelper.GetPolyLine(selectedImportInstance, selectedLayer);
                var matchingPolylines = new List<PolyLine>();

                foreach (var poly in allPolylines)
                {
                    var style = doc.GetElement(poly.GraphicsStyleId) as GraphicsStyle;
                    if (style != null && style.GraphicsStyleCategory.Name == selectedLayer)
                    {
                        matchingPolylines.Add(poly);
                    }
                }

                if (matchingPolylines.Count == 0)
                {
                    TaskDialog.Show("Warning", $"No polylines found in layer: {selectedLayer}");
                    return;
                }

                handler.SlabType = selectedSlabType;
                handler.ReferenceLevel = selectedLevel;
                handler.LayerName = selectedLayer;
                handler.doc = doc;
                handler.PolyLines = matchingPolylines;

                externalEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to create slab: {ex.Message}");
            }
        }

        private void PickCadLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = uidoc.Selection.PickObject(ObjectType.Element, "Select a CAD Import");
                selectedImportInstance = doc.GetElement(sel) as ImportInstance;

                if (selectedImportInstance != null)
                {
                    var cadHelper = new CadGeometry();
                    var layerNames = cadHelper.GetCadGeometry(selectedImportInstance);

                    LayerNameComboBox.Items.Clear();
                    foreach (var layer in layerNames)
                    {
                        LayerNameComboBox.Items.Add(layer);
                    }

                    if (layerNames.Count > 0)
                    {
                        LayerNameComboBox.SelectedItem = layerNames.First();
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Selected element is not a valid CAD import.");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to pick CAD: {ex.Message}");
            }
        }

        private void info()
        {
            LayerNameComboBox.Items.Clear();
            SlabTypeComboBox.Items.Clear();
            ReferenceLevelComboBox.Items.Clear();

            var levels = LevelExtension.GetLevels(doc);
            var floorTypes = FoundationExtension.GetFloorTypes(doc);

            SlabTypeComboBox.ItemsSource = floorTypes;
            SlabTypeComboBox.DisplayMemberPath = "Name";

            ReferenceLevelComboBox.ItemsSource = levels;
            ReferenceLevelComboBox.DisplayMemberPath = "Name";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
