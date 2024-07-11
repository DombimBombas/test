using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using test;

[Transaction(TransactionMode.Manual)]
public class BreakColumnsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;
        Document doc = uiDoc.Document;

        List<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();

        List<FamilyInstance> columns = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilyInstance))
            .OfCategory(BuiltInCategory.OST_Columns)
            .WhereElementIsNotElementType()
            .Cast<FamilyInstance>()
            .ToList();

        List<ColumnToBreak> columnsToBreak = new List<ColumnToBreak>();

        foreach (var column in columns)
        {
            Level baseLevel = doc.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Level;
            Level topLevel = doc.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId()) as Level;

            int baseIndex = levels.FindIndex(l => l.Id == baseLevel.Id);
            int topIndex = levels.FindIndex(l => l.Id == topLevel.Id);

            columnsToBreak.Add(new ColumnToBreak
            {
                ElementId = column.Id.IntegerValue,
                PartsCount = topIndex - baseIndex
            });
        }

        BreakColumnsView breakColumnsView = new BreakColumnsView(columnsToBreak);
        breakColumnsView.ShowDialog();

        using (Transaction trans = new Transaction(doc, "Break Columns"))
        {
            trans.Start();

            foreach (var columnToBreak in columnsToBreak)
            {
                FamilyInstance column = doc.GetElement(new ElementId(columnToBreak.ElementId)) as FamilyInstance;

                Level baseLevel = doc.GetElement(column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Level;
                int baseIndex = levels.FindIndex(l => l.Id == baseLevel.Id);

                for (int i = 1; i <= columnToBreak.PartsCount; i++)
                {
                    ElementId newColumnId = ElementTransformUtils.CopyElement(doc, column.Id, XYZ.Zero).First();
                    FamilyInstance newColumn = doc.GetElement(newColumnId) as FamilyInstance;

                    newColumn.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).Set(levels[baseIndex + i - 1].Id);
                    newColumn.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(levels[baseIndex + i].Id);
                }

                column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(levels[baseIndex + 1].Id);
            }

            trans.Commit();
        }

        return Result.Succeeded;
    }
}
