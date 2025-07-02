using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using OfficeOpenXml;

namespace Structure.UI
{
    public partial class MasterClashUI : Window
    {
        private Document? doc;
        private UIDocument? uidoc;
        private List<ClashReport> allClashReports = new();

        public MasterClashUI(Document? doc)
        {
            InitializeComponent();
            this.doc = doc!;
            this.uidoc = new UIDocument(doc!);

            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            helper.Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
        }

        private void ImportExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileDialog = new OpenFileDialog()
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    Title = "Select Excel File"
                };

                if (fileDialog.ShowDialog() == true)
                {
                    string filePath = fileDialog.FileName;

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets[0];

                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        MessageBox.Show("The selected worksheet is empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    allClashReports.Clear();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var elementId = worksheet.Cells[row, 1].Text;
                        var description = worksheet.Cells[row, 2].Text;
                        var status = worksheet.Cells[row, 3].Text;
                        var clashPoint = worksheet.Cells[row, 4].Text;
                        var image = worksheet.Cells[row, 5].Text;

                        if (!string.IsNullOrWhiteSpace(elementId))
                        {
                            allClashReports.Add(new ClashReport
                            {
                                ElementId = elementId,
                                Description = description,
                                Status = status,
                                ClashPoint = clashPoint,
                                Image = image
                            });
                        }
                    }

                    ClashReportsListView.ItemsSource = allClashReports;
                    MessageBox.Show("Excel data imported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchElement(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                ClashReportsListView.ItemsSource = allClashReports;
                return;
            }

            var filtered = allClashReports
                .Where(c => c.ElementId != null && c.ElementId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ClashReportsListView.ItemsSource = filtered;
        }

        private void ClashReportsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClashReportsListView.SelectedItem is ClashReport selectedReport)
            {
                try
                {
                    if (int.TryParse(selectedReport.ElementId, out int id))
                    {
                        ElementId elementId = new ElementId(id);
                        Element? element = doc?.GetElement(elementId);

                        if (element != null)
                        {
                            uidoc!.Selection.SetElementIds(new List<ElementId> { elementId });
                            uidoc.ShowElements(element);
                        }
                        else
                        {
                            MessageBox.Show($"Element with ID {id} not found in the current document.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Element ID format.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error selecting element: {ex.Message}");
                }
            }
        }
    }

    public class ClashReport
    {
        public string ElementId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ClashPoint { get; set; }
        public string Image { get; set; }
    }
}
