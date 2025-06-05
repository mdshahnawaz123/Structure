using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.IO.Packaging;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;


namespace Structure.UI
{
    /// <summary>
    /// Interaction logic for SheetExportUI.xaml
    /// </summary>
    public partial class SheetExportUI : Window
    {
        private Document doc;
        private  UIDocument uidoc;
        private string selectedFolderPath;
        public SheetExportUI(Document doc,UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;
        }

        private void SaveLocation(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                selectedFolderPath = dialog.SelectedPath;
                MessageBox.Show($"Selected folder:\n{selectedFolderPath}");
            }
        }



        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            

            try
            {
                var openfile = new OpenFolderDialog();

                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (string.IsNullOrEmpty(selectedFolderPath))
                {
                    MessageBox.Show("Please select a folder to save the sheets.");
                    return;
                }





                MessageBox.Show("Sheets exported successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting sheets: {ex.Message}");
            }
        }
    }
}
