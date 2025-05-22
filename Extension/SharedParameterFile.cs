using System;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Structure
{
    public static class SharedParameterFile
    {
        public static void SetSharedParameter(this Application app)
        {
            var path = app.SharedParametersFilename;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                TaskDialog.Show("Shared Parameters",
                    "Shared parameter file is not set or does not exist. Choose a location to create one.");

                var dlg = new SaveFileDialog
                {
                    Title = "Shared Parameter File",
                    Filter = "Text files (*.txt)|*.txt",
                    FileName = "MySharedParameters.txt",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dlg.ShowDialog() == true)
                {
                    try
                    {
                        File.Create(dlg.FileName).Close();          // create & close
                        app.SharedParametersFilename = dlg.FileName; // register
                        TaskDialog.Show("Success", $"Shared parameter file set:\n{dlg.FileName}");
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", ex.Message);
                    }
                }
            }
            else
            {
                TaskDialog.Show("Shared Parameters", $"Shared parameter file found:\n{path}");
            }
        }
    }
}