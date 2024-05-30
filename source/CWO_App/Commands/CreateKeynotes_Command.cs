



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

                //string uniclassFile = @"C:\Users\Om\OneDrive - CW O'Brien Architects Limited\Projects\CWO_App\Keynotes\Uniclass2015_Combined.xlsx";
                //string specFile = @"C:\Users\Om\OneDrive - CW O'Brien Architects Limited\Projects\CWO_App\Keynotes\Architectural Specification (to convert).xlsx";
                //string keynoteFolder = @"C:\Users\Om\OneDrive - CW O'Brien Architects Limited\Projects\CWO_App\Keynotes\Testing";
                //string keynoteFileName = "Test_Me";

                //var keynoteModel = new KeynotesModel(uniclassFile, specFile, keynoteFolder, keynoteFileName);

                //var lines = keynoteModel.CreateKeynoteFile();


                //if (!File.Exists(keynoteModel.GetKeynoteFilePath()))
                //    return;

                //doc.UseTransaction(() => {

                //    doc.LoadKeynoteFile(keynoteModel.GetKeynoteFilePath());
                //});
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
                ToolTip = "Create Keynote file from Uniclass and specifications.\nEnable user to set keynote parameter to multiple families",
                LargeImage = ImageUtils.LoadImage(assembly, "icon_32x32.png")
            });

        }
    }

}
