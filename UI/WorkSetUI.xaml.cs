using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.Events;

namespace Structure.UI
{
    public partial class WorkSetUI : Window
    {
        private Document doc;
        private UIDocument uidoc;
        private ExternalEvent externalEvent;
        private WorkSetHandler workSetHandler;

        public WorkSetUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            workSetHandler = new WorkSetHandler();
            externalEvent = ExternalEvent.Create(workSetHandler);

            CategoryComboBox.SelectionChanged += CategoryComboBox_OnSelectionChanged;

            LoadInfo();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void AssignBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CategoryComboBox.SelectedItem == null ||
                    ElementTypeComboBox.SelectedItem == null ||
                    WorkSetComboBox.SelectedItem == null)
                {
                    TaskDialog.Show("Error", "Please select a valid category, type, and workset.");
                    return;
                }

                var selectedCategory = CategoryComboBox.SelectedItem as Category;
                var selectedType = ElementTypeComboBox.SelectedItem as Element;
                var selectedWorkSet = WorkSetComboBox.SelectedItem as Workset;

                if (selectedCategory == null || selectedType == null || selectedWorkSet == null)
                    return;

                // Get all placed instances of this type
                var matchingElements = new FilteredElementCollector(doc)
                    .OfCategoryId(selectedCategory.Id)
                    .WhereElementIsNotElementType()
                    .Where(el => el.GetTypeId() == selectedType.Id)
                    .ToList();

                if (!matchingElements.Any())
                {
                    TaskDialog.Show("Info", "No instances found for the selected type.");
                    return;
                }

                workSetHandler.doc = doc;
                workSetHandler.uidoc = uidoc;
                workSetHandler.Categories = new List<Category> { selectedCategory };
                workSetHandler.ElementTypes = matchingElements;
                workSetHandler.WorkSets = new List<Workset> { selectedWorkSet };

                externalEvent.Raise();
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Failed to assign WorkSet: {exception.Message}");
            }
        }

        private void LoadInfo()
        {
            CategoryComboBox.Items.Clear();
            ElementTypeComboBox.ItemsSource = null;
            WorkSetComboBox.Items.Clear();

            // Load worksets
            var worksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToWorksets();

            WorkSetComboBox.ItemsSource = worksets;
            WorkSetComboBox.DisplayMemberPath = "Name";

            // Load categories
            var categories = Extension.CategoryExtn.GetCategories(doc)
                .Distinct()
                .OrderBy(x => x.Name);

            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "Name";
        }

        private void CategoryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedCategory = CategoryComboBox.SelectedItem as Category;
                if (selectedCategory == null)
                {
                    ElementTypeComboBox.ItemsSource = null;
                    return;
                }

                // Get all instances in the selected category
                var instances = new FilteredElementCollector(doc)
                    .OfCategoryId(selectedCategory.Id)
                    .WhereElementIsNotElementType()
                    .Where(x => x.GetTypeId() != ElementId.InvalidElementId)
                    .ToList();

                // Get unique type IDs used in instances
                var typeIds = instances
                    .Select(x => x.GetTypeId())
                    .Distinct()
                    .ToList();

                // Get element types that are used
                var usedTypes = new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .Where(x => typeIds.Contains(x.Id))
                    .Cast<Element>()
                    .GroupBy(x => x.Name)           // avoid duplicates by name
                    .Select(g => g.First())         // keep first of each
                    .OrderBy(x => x.Name)
                    .ToList();

                ElementTypeComboBox.ItemsSource = usedTypes;
                ElementTypeComboBox.DisplayMemberPath = "Name";
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", exception.Message);
            }
        }
    }
}
