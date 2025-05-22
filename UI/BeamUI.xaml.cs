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
    public partial class BeamUI : Window
    {
        private Document? doc;
        private UIDocument? uidoc;
        private ImportInstance? importInstance;
        private BeamHandler? handler;
        private ExternalEvent? externalEvent;

        public BeamUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;
            handler = new BeamHandler();
            externalEvent = ExternalEvent.Create(handler);
            LoadInfo();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void PickCadButton(object sender, RoutedEventArgs e)
        {
            try
            {
                var selection = uidoc?.Selection.PickObject(ObjectType.Element, new CadSelectionFilter(), "Pick a CAD Layer");
                if (selection != null)
                {
                    importInstance = doc?.GetElement(selection.ElementId) as ImportInstance;
                    if (importInstance != null)
                    {
                        var geometry = new CadGeometry();
                        var layers = geometry.GetCadGeometry(importInstance);
                        LayerNameComboBox.ItemsSource = layers;
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Selected element is not a valid ImportInstance.");
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"CAD selection failed: {ex.Message}");
            }
        }

        private void CreateBeamButton(object sender, RoutedEventArgs e)
        {
            try
            {
                var level = ReferenceLevelComboBox.SelectedItem as Level;
                var beamType = BeamTypeComboBox.SelectedItem as FamilySymbol;
                var layerName = LayerNameComboBox.SelectedItem as string;

                if (level == null || beamType == null || layerName == null)
                {
                    TaskDialog.Show("Input Error", "Please select all required inputs.");
                    return;
                }

                handler.doc = doc;
                handler.RefLvl = level;
                handler.familySymbol = beamType;
                handler.LayerName = layerName;
                handler.importInstance = importInstance;

                var plines = new CadGeometry().GetPolyLine(importInstance, layerName);
                if (plines == null || !plines.Any())
                {
                    TaskDialog.Show("Warning", "No polylines found on selected layer.");
                    return;
                }

                handler.PLines = plines.ToList();
                externalEvent?.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Create Beam Error", $"Failed to initiate beam creation: {ex.Message}");
            }
        }

        private void LoadInfo()
        {
            BeamTypeComboBox.Items.Clear();
            LayerNameComboBox.Items.Clear();
            ReferenceLevelComboBox.Items.Clear();

            var beamTypes = Extension.FamilyInstance.GetFamilySymbols<FamilySymbol>(doc, BuiltInCategory.OST_StructuralFraming);
            BeamTypeComboBox.ItemsSource = beamTypes;
            BeamTypeComboBox.DisplayMemberPath = "Name";

            var levels = LevelExtension.GetLevels(doc);
            ReferenceLevelComboBox.ItemsSource = levels;
            ReferenceLevelComboBox.DisplayMemberPath = "Name";
        }

        private class CadSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem) => elem is ImportInstance;
            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
