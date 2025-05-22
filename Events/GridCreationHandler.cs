using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Structure.Events
{
    public class GridCreationHandler : IExternalEventHandler
    {
        public Document doc { get; set; } = null!;
        public IList<Line>? linesToUse { get; set; } = new List<Line>();

        private HashSet<string> usedNames = new HashSet<string>();

        public void Execute(UIApplication app)
        {
            if (doc == null || linesToUse == null || linesToUse.Count == 0)
                return;

            try
            {
                using (Transaction tx = new Transaction(doc, "Create Grid From CAD"))
                {
                    tx.Start();

                    int letterCounter = 0;  // For A, B, ..., Z, AA, AB...
                    int numberCounter = 1;  // For 1, 2, 3...

                    foreach (var line in linesToUse)
                    {
                        var gridLine = Line.CreateBound(line.GetEndPoint(0), line.GetEndPoint(1));
                        var grid = Grid.Create(doc, gridLine);

                        string name;
                        if (Math.Abs(gridLine.Direction.X) > Math.Abs(gridLine.Direction.Y))
                        {
                            // Horizontal-like (assign letters)
                            do
                            {
                                name = GetLetterName(letterCounter++);
                            } while (usedNames.Contains(name));
                        }
                        else
                        {
                            // Vertical-like (assign numbers)
                            do
                            {
                                name = numberCounter.ToString();
                                numberCounter++;
                            } while (usedNames.Contains(name));
                        }

                        usedNames.Add(name);
                        grid.Name = name;
                    }

                    tx.Commit();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Grid Creation Error", e.Message);
            }
        }

        // Converts 0 → A, 1 → B, ..., 25 → Z, 26 → AA, 27 → AB, etc.
        private string GetLetterName(int index)
        {
            var sb = new StringBuilder();
            index++;  // Shift to 1-based indexing

            while (index > 0)
            {
                index--;
                sb.Insert(0, (char)('A' + (index % 26)));
                index /= 26;
            }

            return sb.ToString();
        }

        public string GetName() => "Grid Creation Handler";
    }
}
