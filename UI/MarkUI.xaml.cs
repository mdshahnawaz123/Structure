using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.Events;
using Structure.Extension;

namespace Structure.UI
{
    public partial class MarkUI : Window
    {
        private Document? doc;
        private UIDocument? uidoc;
        private ExternalEvent? externalEvent;
        private MarkHandler? markHandler;

        public MarkUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            markHandler = new MarkHandler();
            externalEvent = ExternalEvent.Create(markHandler);

            ElementTypeCombbox.SelectionChanged += ElementTypeCombbox_OnSelectionChanged;

            LoadCategoryList();
        }

        private void TagCreate(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedCategory = ElementTypeCombbox.SelectedItem as Category;
                if (selectedCategory == null)
                {
                    TaskDialog.Show("Error", "Please select a valid category.");
                    return;
                }

                var selectedParameter = DataTypeCombbox.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(selectedParameter))
                {
                    TaskDialog.Show("Error", "Please select a valid parameter.");
                    return;
                }

                var prefix = TextBox.Text;
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    TaskDialog.Show("Error", "Please enter a valid prefix.");
                    return;
                }

                markHandler.doc = doc;
                markHandler.Categories = new List<Category> { selectedCategory };
                markHandler.Prefix = prefix;
                markHandler.ParameterName = selectedParameter;

                externalEvent?.Raise();
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", exception.Message);
            }
        }

        private void LoadCategoryList()
        {
            ElementTypeCombbox.Items.Clear();
            DataTypeCombbox.Items.Clear();
            TextBox.Text = string.Empty;

            var categories = Extension.CategoryExtn.GetCategories(doc)
                .OrderBy(c => c.Name);

            ElementTypeCombbox.ItemsSource = categories;
            ElementTypeCombbox.DisplayMemberPath = "Name";
        }

        private void ElementTypeCombbox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTypeCombbox.ItemsSource = null;

            var selectedCategory = ElementTypeCombbox.SelectedItem as Category;
            if (selectedCategory == null || doc == null)
                return;

            var element = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategoryId(selectedCategory.Id)
                .FirstOrDefault();

            if (element != null)
            {
                var parameterNames = element.GetWritableStringParameterNames();
                if (parameterNames.Any())
                {
                    DataTypeCombbox.ItemsSource = parameterNames;
                    DataTypeCombbox.SelectedIndex = 0;
                }
                else
                {
                    TaskDialog.Show("Info", "No writable string parameters found for this category.");
                }
            }
            else
            {
                TaskDialog.Show("Info", "No elements found for selected category.");
            }
        }
    }
}
