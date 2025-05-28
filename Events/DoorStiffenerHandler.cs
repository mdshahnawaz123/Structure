using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Events
{
    public class DoorStiffenerHandler : IExternalEventHandler
    {
        public Level TopLvel { get; set; }
        public Level BottomLvel { get; set; }
        public FamilySymbol familySymbol { get; set; }
        public Document doc { get; set; }
        public List<Reference> SelDoor { get; set; }
        public bool UseStructuralColumn { get; set; }
        public double offset { get; set; }
        public FamilySymbol DoorTypes { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                // Validate input
                if (doc == null || familySymbol == null || TopLvel == null || BottomLvel == null || SelDoor == null)
                {
                    TaskDialog.Show("Event Handler Error", "Missing input parameters. Please check the form.");
                    return;
                }

                // Activate the column family symbol
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                double offsetFeet = offset / 304.8; // Convert mm to feet

                using (Transaction trans = new Transaction(doc, "Place Door Stiffener Columns"))
                {
                    trans.Start();

                    foreach (var doorRef in SelDoor)
                    {
                        Element doorElem = doc.GetElement(doorRef);
                        if (doorElem is FamilyInstance door && door.Location is LocationPoint doorLocation)
                        {
                            // Filter by selected door type if provided
                            if (DoorTypes != null && door.Symbol.Id != DoorTypes.Id)
                                continue;

                            // Get the wall that hosts this door
                            Wall hostWall = door.Host as Wall;
                            if (hostWall == null)
                                continue;

                            // Get wall direction vector
                            XYZ wallDirection = GetWallDirection(hostWall);
                            if (wallDirection == null)
                                continue;

                            XYZ doorOrigin = doorLocation.Point;

                            // Calculate column positions along wall direction
                            XYZ point1 = doorOrigin + wallDirection * offsetFeet;
                            XYZ point2 = doorOrigin - wallDirection * offsetFeet;

                            // Place columns with proper rotation
                            PlaceColumn(point1, hostWall);
                            PlaceColumn(point2, hostWall);
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Door Handler Error", $"Failed to place columns: {ex.Message}");
            }
        }

        private XYZ GetWallDirection(Wall wall)
        {
            try
            {
                // Get wall location curve
                LocationCurve wallLocation = wall.Location as LocationCurve;
                if (wallLocation?.Curve == null)
                    return null;

                Curve wallCurve = wallLocation.Curve;

                // Get the direction vector of the wall
                XYZ wallDirection;

                if (wallCurve is Line line)
                {
                    wallDirection = line.Direction;
                }
                else
                {
                    // For non-linear walls, get direction at start point
                    wallDirection = wallCurve.ComputeDerivatives(0, false).BasisX;
                }

                // Normalize the vector and ensure it's in the XY plane
                wallDirection = new XYZ(wallDirection.X, wallDirection.Y, 0).Normalize();

                return wallDirection;
            }
            catch
            {
                return null;
            }
        }

        private void PlaceColumn(XYZ location, Wall hostWall)
        {
            try
            {
                // Create the column
                FamilyInstance column = doc.Create.NewFamilyInstance(
                    location,
                    familySymbol,
                    BottomLvel,
                    UseStructuralColumn ? StructuralType.Column : StructuralType.NonStructural
                );

                // Set top level
                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM)?.Set(TopLvel.Id);

                // Optional: Rotate column to align with wall if needed
                // This depends on your column family orientation requirements
                if (hostWall != null)
                {
                    XYZ wallDirection = GetWallDirection(hostWall);
                    if (wallDirection != null)
                    {
                        // Calculate rotation angle if column needs to be rotated
                        // This example assumes column should align with wall direction
                        double rotationAngle = Math.Atan2(wallDirection.Y, wallDirection.X);

                        // Only rotate if there's a significant angle difference
                        if (Math.Abs(rotationAngle) > 0.01) // ~0.5 degrees tolerance
                        {
                            Line rotationAxis = Line.CreateBound(
                                location,
                                location + XYZ.BasisZ
                            );
                            ElementTransformUtils.RotateElement(
                                doc,
                                column.Id,
                                rotationAxis,
                                rotationAngle
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Column Placement Error", $"Failed to place column at location: {ex.Message}");
            }
        }

        public string GetName()
        {
            return "Door Stiffener Column Placement Handler";
        }
    }
}