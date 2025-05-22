using Autodesk.Revit.DB;
using System.Linq;

namespace Structure
{
    public static class BasePointUtils
    {
        public static Transform GetSurveyPointTransform(Document doc)
        {
            BasePoint survey = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>()
                .FirstOrDefault(p => !p.IsShared);

            return survey != null ? Transform.CreateTranslation(survey.Position) : Transform.Identity;
        }

        public static Transform GetProjectBasePointTransform(Document doc)
        {
            BasePoint project = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>()
                .FirstOrDefault(p => p.IsShared);

            return project != null ? Transform.CreateTranslation(project.Position) : Transform.Identity;
        }
    }
}