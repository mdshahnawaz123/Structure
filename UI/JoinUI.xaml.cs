using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ElementJoin;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Structure.UI
{
    public partial class JoinUI : Window
    {
        private readonly Document doc;
        private readonly UIDocument uidoc;

        private readonly JoinExternalEventHandler joinHandler;
        private readonly ExternalEvent joinEvent;

        private readonly UnjoinExternalEventHandler unjoinHandler;
        private readonly ExternalEvent unjoinEvent;

        private readonly SwitchJoinOrderExternalEventHandler switchHandler;
        private readonly ExternalEvent switchEvent;

        public JoinUI(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;

            joinHandler = new JoinExternalEventHandler();
            joinEvent = ExternalEvent.Create(joinHandler);

            unjoinHandler = new UnjoinExternalEventHandler();
            unjoinEvent = ExternalEvent.Create(unjoinHandler);

            switchHandler = new SwitchJoinOrderExternalEventHandler();
            switchEvent = ExternalEvent.Create(switchHandler);

            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = doc.Settings.Categories
                .Cast<Category>()
                .Where(c => c.CategoryType == CategoryType.Model && !c.IsTagCategory)
                .OrderBy(c => c.Name)
                .ToList();

            CBCategory1.ItemsSource = categories;
            CBCategory2.ItemsSource = categories;

            CBCategory1.DisplayMemberPath = "Name";
            CBCategory2.DisplayMemberPath = "Name";
        }

        private void Join_OnClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection(out Category cat1, out Category cat2)) return;

            joinHandler.Doc = doc;
            joinHandler.Category1 = cat1;
            joinHandler.Category2 = cat2;
            joinEvent.Raise();
        }

        private void UnJoin_OnClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection(out Category cat1, out Category cat2)) return;

            unjoinHandler.Doc = doc;
            unjoinHandler.Category1 = cat1;
            unjoinHandler.Category2 = cat2;
            unjoinEvent.Raise();
        }

        private void SwitchJoinOrder_OnClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelection(out Category cat1, out Category cat2)) return;

            switchHandler.Doc = doc;
            switchHandler.Category1 = cat1;
            switchHandler.Category2 = cat2;
            switchEvent.Raise();
        }

        private bool ValidateSelection(out Category category1, out Category category2)
        {
            category1 = CBCategory1.SelectedItem as Category;
            category2 = CBCategory2.SelectedItem as Category;

            if (category1 == null || category2 == null)
            {
                TaskDialog.Show("Warning", "Please select both categories.");
                return false;
            }

            return true;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
