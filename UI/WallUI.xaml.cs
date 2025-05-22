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
using Structure.Extension;

namespace Structure.UI
{
    public partial class WallUI : Window
    {
        private readonly Document doc;
        private readonly UIDocument uidoc;

        private readonly ExternalEvent externalEvent;
        private readonly WallHandler wallHandler;

        private ImportInstance? _importInstance;

        public WallUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            wallHandler = new WallHandler();
            externalEvent = ExternalEvent.Create(wallHandler);

            InitUI();
        }

        private void PickedCad_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = uidoc.Selection.PickObject(ObjectType.Element, new CadImportSelectionFilter());

                _importInstance = doc.GetElement(sel) as ImportInstance;

                if (_importInstance == null)
                {
                    TaskDialog.Show("Error", "Selected element is not a CAD Import Instance.");
                    return;
                }

                SelectedLayerNameComboBox.Items.Clear();
                var cadGeometry = new CadGeometry();
                var layerNames = cadGeometry.GetCadGeometry(_importInstance);

                foreach (var layerName in layerNames)
                    SelectedLayerNameComboBox.Items.Add(layerName);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to pick CAD layer: {ex.Message}");
            }
        }

        private void CreateWall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedLayer = SelectedLayerNameComboBox.SelectedItem as string;
                var selectedWallType = WallTypeComboBox.SelectedItem as WallType;
                var selectedTopLevel = WallTopLevelComboBox.SelectedItem as Level;
                var selectedBottomLevel = WallBottomLevelComboBox.SelectedItem as Level;

                if (_importInstance == null || selectedLayer == null || selectedWallType == null || selectedTopLevel == null || selectedBottomLevel == null)
                {
                    TaskDialog.Show("Error", "Please make all selections.");
                    return;
                }

                // Get all polylines and filter by layer
                var cadGeometry = new CadGeometry();
                var allPolylines = cadGeometry.GetPolyLine(_importInstance, selectedLayer);

                var filteredPolylines = allPolylines
                    .Where(pl =>
                    {
                        var styleId = pl.GraphicsStyleId;
                        if (styleId == ElementId.InvalidElementId) return false;
                        var style = doc.GetElement(styleId) as GraphicsStyle;
                        return style?.GraphicsStyleCategory?.Name == selectedLayer;
                    })
                    .ToList();

                if (filteredPolylines.Count == 0)
                {
                    TaskDialog.Show("Info", $"No geometry found in selected layer: {selectedLayer}");
                    return;
                }

                // Set handler properties
                wallHandler.doc = doc;
                wallHandler.Toplevel = selectedTopLevel;
                wallHandler.Bottomlevel = selectedBottomLevel;
                wallHandler.SelectedWallType = selectedWallType;
                wallHandler.LayerName = new List<string> { selectedLayer };
                wallHandler.Pline = filteredPolylines;

                externalEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Wall creation failed: {ex.Message}");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void InitUI()
        {
            WallTopLevelComboBox.Items.Clear();
            WallBottomLevelComboBox.Items.Clear();
            WallTypeComboBox.Items.Clear();

            var lvls = LevelExtension.GetLevels(doc);
            WallTopLevelComboBox.ItemsSource = lvls;
            WallTopLevelComboBox.DisplayMemberPath = "Name";

            WallBottomLevelComboBox.ItemsSource = lvls;
            WallBottomLevelComboBox.DisplayMemberPath = "Name";

            var wallTypes = WallInfoExtension.GetWallType(doc);
            WallTypeComboBox.ItemsSource = wallTypes;
            WallTypeComboBox.DisplayMemberPath = "Name";
        }

        private class CadImportSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem) => elem is ImportInstance;
            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
