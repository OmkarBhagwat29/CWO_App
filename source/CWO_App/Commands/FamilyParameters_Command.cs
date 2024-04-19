



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
    public class FamilyParameters_Command : ExternalCommand
    {
        public override void Execute()
        {
			try
			{
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                Host.GetService<FamilyParametersShowWindow>().Execute();
            }
			catch
			{
                
			}
        }

        public static void CreateFamilyParametersButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"Family\nParameters", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Add and Delete Shared Parameters to families",
                LargeImage = ImageUtils.LoadImage(assembly, "icon_32x32.png")
            });

        }
    }
}
