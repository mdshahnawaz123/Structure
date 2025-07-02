using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Document = Autodesk.Revit.DB.Document;

namespace Structure
{
    [Transaction(TransactionMode.Manual)]
    public class MasterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var app = doc.Application;

            try
            {
                // STEP 1: Ensure shared parameter file is set
                DefinitionFile sharedParamFile = app.OpenSharedParameterFile();
                if (sharedParamFile == null)
                {
                    message = "Shared Parameter File does not exist. Please set or create one in Revit first.";
                    return Result.Failed;
                }

                // STEP 2: Get or create parameter group
                string groupName = "Structure";
                DefinitionGroup group = null;

                // Safe way to get existing group
                foreach (DefinitionGroup existingGroup in sharedParamFile.Groups)
                {
                    if (existingGroup.Name == groupName)
                    {
                        group = existingGroup;
                        break;
                    }
                }

                // Create group if it doesn't exist
                if (group == null)
                {
                    group = sharedParamFile.Groups.Create(groupName);
                }

                // STEP 3: Get or create the parameter definition
                string paramName = "Wall Mark";
                Definition paramDef = null;

                // Safe way to get existing definition
                foreach (Definition existingDef in group.Definitions)
                {
                    if (existingDef.Name == paramName)
                    {
                        paramDef = existingDef;
                        break;
                    }
                }

                if (paramDef == null)
                {
                    ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(paramName, SpecTypeId.String.Text)
                    {
                        Visible = true
                    };
                    paramDef = group.Definitions.Create(options);
                }

                // STEP 4: Create a category set and add Wall category
                CategorySet catSet = app.Create.NewCategorySet();
                Category wallCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);

                if (wallCat == null)
                {
                    message = "Wall category not found.";
                    return Result.Failed;
                }

                catSet.Insert(wallCat);

                // STEP 5: Check if parameter is already bound
                BindingMap bindingMap = doc.ParameterBindings;
                Binding existingBinding = bindingMap.get_Item(paramDef);

                if (existingBinding != null)
                {
                    TaskDialog.Show("Info", $"Parameter '{paramName}' is already bound to categories.");
                    return Result.Succeeded;
                }

                // Create an instance binding and bind it to the category
                InstanceBinding binding = app.Create.NewInstanceBinding(catSet);

                using (Transaction tx = new Transaction(doc, "Bind Shared Parameter"))
                {
                    tx.Start();

                    bool success = false;

                    // For Revit 2025, use GroupTypeId instead of BuiltInParameterGroup
                    try
                    {
                        success = bindingMap.Insert(paramDef, binding, GroupTypeId.IdentityData);
                    }
                    catch (Exception)
                    {
                        // Fallback to other group types
                        try
                        {
                            success = bindingMap.Insert(paramDef, binding, GroupTypeId.Text);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                success = bindingMap.Insert(paramDef, binding, GroupTypeId.General);
                            }
                            catch (Exception ex)
                            {
                                message = $"Failed to bind parameter with any group type: {ex.Message}";
                                tx.RollBack();
                                return Result.Failed;
                            }
                        }
                    }

                    if (success)
                    {
                        tx.Commit();
                        TaskDialog.Show("Success", $"Shared parameter '{paramName}' successfully bound to Walls.");
                        return Result.Succeeded;
                    }
                    else
                    {
                        tx.RollBack();
                        message = "Failed to bind the shared parameter.";
                        return Result.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}";
                return Result.Failed;
            }
        }
    }
}