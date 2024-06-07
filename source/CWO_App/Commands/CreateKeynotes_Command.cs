



using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CWO_App.UI.Commands;
using CWO_App.UI.Models.Keynotes;
using DocumentFormat.OpenXml.Spreadsheet;
using Nice3point.Revit.Toolkit.External;
using RevitCore.Extensions;
using RevitCore.Utils;
using Serilog.Core;
using System.IO;
using System.Reflection;

namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateKeynotes_Command : ExternalCommand
    {

        public override void Execute()
        {
			try
			{
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                Host.GetService<KeynotesCreationShowWindow>().Execute();

            }
			catch
			{
                
			}
        }

        public static void CreateKeynotesCreationButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"Keynotes\nCreation", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = $"Create Keynote file from Uniclass and specifications." +
                $"\nEnable user to set keynote parameter to multiple families",
                LargeImage = ImageUtils.LoadImage(assembly, "Keynote_32x32.png")
                
            });

        }
    }

}
