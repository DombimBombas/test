using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace pilons
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            //вкладка
            string tabName = "Вкладка";
            application.CreateRibbonTab(tabName);

            //панель на вкладке
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Панель");

            //кнопка на панели
            string assemblyLocation = Assembly.GetExecutingAssembly().Location,
                   iconsDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";
            panel.AddItem(new PushButtonData(nameof(BreakColumnsCommand), "Удаление пилонов", assemblyLocation, typeof(BreakColumnsCommand).FullName)
            {
                LargeImage = new BitmapImage(new Uri(iconsDirectoryPath + "chees.ico"))
            });

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}