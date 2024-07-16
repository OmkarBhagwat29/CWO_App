using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CWO_App.UI.Commands;
using Nice3point.Revit.Toolkit.External;
using RevitCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class ApartmentParameters_Command : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                Host.GetService<ApartmentParametersShowWindow>().Execute();

            }
            catch
            {

            }
        }

        public static void CreateApartmentParametersButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"Apartment\nParameters", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Set Apartment Parameters",
                LargeImage = ImageUtils.LoadImage(assembly, "icon_32x32.png")
            });

        }
    }
}
