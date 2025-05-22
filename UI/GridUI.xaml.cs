using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.Events;
using Structure.Extension;
using Autodesk.Revit.UI.Selection;

namespace Structure.UI
{
    public partial class GridUI : Window
    {
        private readonly Document doc;
        private readonly UIDocument uidoc;
        private ImportInstance? importInstance;
        private readonly GridCreationHandler gridCreationHandler;
        private readonly ExternalEvent externalEvent;

        public GridUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;
            gridCreationHandler = new GridCreationHandler();
            externalEvent = ExternalEvent.Create(gridCreationHandler);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Picked_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Select a CAD ImportInstance");
                if (pickedRef == null) return;

                var element = doc.GetElement(pickedRef);
                if (element is not ImportInstance selectedImport)
                {
                    MessageBox.Show("Selected element is not a CAD ImportInstance.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                importInstance = selectedImport;

                // Populate ComboBox with CAD layers
                var cadGeometry = new CadGeometry();
                var layerNames = cadGeometry.GetCadGeometry(importInstance);

                CBLayer.Items.Clear();
                foreach (var layerName in layerNames)
                {
                    CBLayer.Items.Add(layerName);
                }

                if (CBLayer.Items.Count > 0)
                    CBLayer.SelectedIndex = 0;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User cancelled selection
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GridCreation_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (importInstance == null)
                {
                    MessageBox.Show("No ImportInstance selected. Please select a valid CAD ImportInstance.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CBLayer.SelectedItem is not string selectedLayer || string.IsNullOrEmpty(selectedLayer))
                {
                    MessageBox.Show("No layer selected. Please choose a CAD layer from the dropdown.", "Missing Layer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cadGeometry = new CadGeometry();
                var linesToUse = cadGeometry.GetPline(importInstance, selectedLayer);

                if (linesToUse == null || linesToUse.Count == 0)
                {
                    MessageBox.Show("No polylines found on the selected layer.", "No Geometry", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                gridCreationHandler.doc = doc;
                gridCreationHandler.linesToUse = linesToUse;
                externalEvent.Raise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create grid: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
