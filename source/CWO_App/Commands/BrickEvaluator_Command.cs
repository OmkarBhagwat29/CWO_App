



using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CWO_App.UI.Commands;
using CWO_App.UI.ViewModels;
using Nice3point.Revit.Toolkit.External;
using RevitCore.Utils;
using RevitCore.Extensions;
using System.Reflection;

namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class BrickEvaluator_Command : ExternalCommand
    {
        public override void Execute()
        {
			try
			{
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                Host.GetService<BrickEvaluatorShowWindow>().Execute();
            }
			catch
			{
                
			}
        }

        public static void CreateBrickEvaluatorButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"Brick\nEvaluator", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Brick Evaluator",
                LargeImage = ImageUtils.LoadImage(assembly, "BrickCalculator_32x32.png")
            });

        }
    }
}
