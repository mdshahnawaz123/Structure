using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Structure.App
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // === LOGIN FLOW ===
                var login = new Structure.UI.Loginform();
                bool skipLogin = false;

                try
                {
                    skipLogin = login.TryAutoLoginAndClose();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Login Error", $"Auto-login failed: {ex.Message}");
                    return Result.Failed;
                }

                if (!skipLogin)
                {
                    bool? result = null;

                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            result = login.ShowDialog();
                        });
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Login Error", $"Login dialog failed: {ex.Message}");
                        return Result.Failed;
                    }

                    if (result != true)
                    {
                        TaskDialog.Show("Login Cancelled", "Login was cancelled by the user.");
                        return Result.Cancelled;
                    }
                }

                // === CREATE RIBBON TAB ===
                const string tabName = "BIM Digital Design";
                try { application.CreateRibbonTab(tabName); } catch { /* Tab might already exist */ }

                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                #region CAD Automation Panel
                var cadPanel = SafeCreatePanel(application, tabName, "CAD Automation");
                AddButtonSafe(cadPanel, "Floor Automation", "Floor", "Structure.Command.FloorCommand",
                    "Generates a structural floor based on predefined parameters.", "Floor.png", assemblyPath);

                AddButtonSafe(cadPanel, "Grid Automation", "Grid", "Structure.Command.GridCommand",
                    "Generates a structural grid system.", "Grid.png", assemblyPath);

                AddButtonSafe(cadPanel, "Beam Automation", "Beam", "Structure.Command.BeamCommand",
                    "Generates a structural beam system.", "Beam.png", assemblyPath);

                AddButtonSafe(cadPanel, "Wall Automation", "Wall", "Structure.Command.WallCommand",
                    "Generates a structural wall system.", "Wall.png", assemblyPath);

                AddButtonSafe(cadPanel, "Column Automation", "Column", "Structure.Command.ColumnCommand",
                    "Places structural columns in the project.", "Column.png", assemblyPath);

                cadPanel.AddSeparator();
                #endregion

                #region Model Automation Panel
                var modelPanel = SafeCreatePanel(application, tabName, "Model Automation");

                AddButtonSafe(modelPanel, "Join Automation", "Join/Unjoin", "Structure.Command.JoinCommand",
                    "Joins or unjoins structural elements.", "Join.png", assemblyPath);

                AddButtonSafe(modelPanel, "Coordinates Automation", "Coordinates", "Structure.Command.CoordinatesCommand",
                    "Generates pile coordinates based on predefined rules.", "Coordinate.png", assemblyPath);

                AddButtonSafe(modelPanel, "PCC Automation", "PCC", "Structure.Command.PCommand",
                    "Generates Foundation PCC based on predefined rules.", "PCC.png", assemblyPath);

                AddButtonSafe(modelPanel, "WorkSet Automation", "Workset", "Structure.Command.WorkSetCommand",
                    "Assigns worksets based on predefined rules.", "WorkSet.png", assemblyPath);

                AddButtonSafe(modelPanel, "Door Stiffener Automation", "Stiffener", "Structure.Command.DoorStiffenerCommand",
                    "Creates a door stiffener according to predefined rules.", "DoorStiffner.png", assemblyPath);

                AddButtonSafe(modelPanel, "ChatGPT AI", "ChatGPT", "Structure.Command.ChatGPTCommand",
                    "Chat with your AI assistant.", "AI.png", assemblyPath);
                #endregion

                #region Dimension Automation Panel
                var dimPanel = SafeCreatePanel(application, tabName, "Dimension Automation");

                AddButtonSafe(dimPanel, "Grid Dimension Automation", "Dim-Grid", "Structure.Command.GridDimension",
                    "Create grid-to-grid dimensions as per predefined rules.", "Dim.png", assemblyPath);

                AddButtonSafe(dimPanel, "Column Dimension Automation", "Dim-Column", "Structure.Command.ColumnDimension",
                    "Create column-to-grid dimensions as per predefined rules.", "Dim.png", assemblyPath);
                #endregion

                #region Parameter Automation Panel
                var paramPanel = SafeCreatePanel(application, tabName, "Parameter Automation");

                AddButtonSafe(paramPanel, "Mark Automation", "Mark", "Structure.Command.MarkCommand",
                    "Assigns mark parameters according to predefined rules.", "Mark.png", assemblyPath);
                #endregion

                #region Data Visualizer Panel
                var dataPanel = SafeCreatePanel(application, tabName, "Data Visualizer");

                AddButtonSafe(dataPanel, "Foundation Data Visualization", "Foundation", "Structure.Command.FoundationData",
                    "Visualizes foundation data based on defined rules.", "Data.png", assemblyPath);
                #endregion

                #region Data Export Panel
                var exportPanel = SafeCreatePanel(application, tabName, "Data Export");

                AddButtonSafe(exportPanel, "Export Data to Excel", "Export", "Structure.Command.FoundationData",
                    "Exports foundation data to Excel.", "XML.png", assemblyPath);
                #endregion

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Startup Error", $"Unexpected error occurred: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                TaskDialog.Show("Goodbye", "See you next time.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Shutdown Error", $"Error during shutdown: {ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Safely creates a ribbon panel, catching any exceptions.
        /// </summary>
        private RibbonPanel SafeCreatePanel(UIControlledApplication app, string tabName, string panelName)
        {
            try
            {
                return app.CreateRibbonPanel(tabName, panelName);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Panel Error", $"Failed to create panel '{panelName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Safely creates and adds a PushButton to a panel.
        /// </summary>
        private void AddButtonSafe(RibbonPanel panel, string name, string text, string className, string tooltip, string imageName, string assemblyPath)
        {
            try
            {
                var buttonData = new PushButtonData(name, text, assemblyPath, className);
                PushButton button = panel.AddItem(buttonData) as PushButton;

                if (button != null)
                {
                    button.ToolTip = tooltip;
                    button.LargeImage = new BitmapImage(new Uri($"pack://application:,,,/Structure;component/Resources/{imageName}"));
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Button Error", $"Failed to create button '{text}': {ex.Message}");
            }
        }
    }
}
