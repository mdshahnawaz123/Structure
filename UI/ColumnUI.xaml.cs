using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Structure.Events;
using Structure.Extension;

namespace Structure.UI
{
    public partial class ColumnUI : Window
    {
        private readonly UIDocument uidoc;
        private readonly Document doc;

        private ImportInstance? selectedImportInstance;

        // ✅ External Event setup
        private readonly ColumnHandler handler;
        private readonly ExternalEvent externalEvent;

        public ColumnUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            // ✅ Initialize handler and event
            handler = new ColumnHandler();
            externalEvent = ExternalEvent.Create(handler);

            // Load levels
            var levels = LevelExtension.GetLevels(doc);
            CBBaseLevel.ItemsSource = levels;
            CBTopLevel.ItemsSource = levels;
            CBBaseLevel.DisplayMemberPath = "Name";
            CBTopLevel.DisplayMemberPath = "Name";

            // Load column types
            var colTypes = ColumnType.GetColumnTypes(doc);
            foreach (var fs in colTypes)
            {
                CBColumnType.Items.Add(fs.Name);
            }
        }

        private void PickedCad_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedCad = uidoc.Selection.PickObject(ObjectType.Element, new ICADSelection(), "Select a CAD file");
                var cadElement = doc.GetElement(selectedCad);

                if (cadElement is ImportInstance importInstance)
                {
                    selectedImportInstance = importInstance;

                    var cadGeometry = new CadGeometry();
                    var layerNames = cadGeometry.GetCadGeometry(importInstance);

                    if (layerNames != null && layerNames.Any())
                    {
                        CBLayer.ItemsSource = layerNames;
                    }
                    else
                    {
                        TaskDialog.Show("CAD Layers", "No layers found in selected CAD.");
                    }
                }
            }
            catch
            {
                TaskDialog.Show("Error", "Select the CAD layer.");
            }
        }

        private void ColumnCreation_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectedImportInstance == null)
            {
                TaskDialog.Show("Error", "Please select a CAD file first.");
                return;
            }

            string? selectedLayer = CBLayer.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedLayer))
            {
                TaskDialog.Show("Error", "Please select a CAD layer.");
                return;
            }

            string? selectedColumnTypeName = CBColumnType?.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedColumnTypeName))
            {
                TaskDialog.Show("Error", "Please select a column type.");
                return;
            }

            Level? baseLevel = CBBaseLevel.SelectedItem as Level;
            Level? topLevel = CBTopLevel.SelectedItem as Level;
            if (baseLevel == null || topLevel == null)
            {
                TaskDialog.Show("Error", "Please select both base and top levels.");
                return;
            }

            var selectedType = ColumnType.GetColumnTypes(doc)
                .FirstOrDefault(ct => ct.Name == selectedColumnTypeName);
            if (selectedType == null)
            {
                TaskDialog.Show("Error", "Selected column type not found.");
                return;
            }

            var cadGeometry = new CadGeometry();
            var polyLines = cadGeometry.GetPolyLine(selectedImportInstance, selectedLayer);
            if (polyLines == null || polyLines.Count == 0)
            {
                TaskDialog.Show("Error", $"No polylines found on layer '{selectedLayer}'.");
                return;
            }

            // ✅ Pass all values to handler
            handler.SelectedColumnType = selectedType;
            handler.BaseLevel = baseLevel;
            handler.TopLevel = topLevel;
            handler.PolyLines = polyLines;
            handler.doc = doc;

            // ✅ Raise safely inside Revit API context
            externalEvent.Raise();
        }

        public class ICADSelection : ISelectionFilter
        {
            public bool AllowElement(Element elem) => elem is ImportInstance;
            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
