



using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using CWO_App.UI.Commands;
using CWO_App.UI.Constants;
using CWO_App.UI.Models.ApartmentValidation;
using CWO_App.UI.Requirements;
using Nice3point.Revit.Extensions;
using Nice3point.Revit.Toolkit.External;
using RevitCore.Extensions;
using RevitCore.Extensions.PointInPoly;
using RevitCore.ResidentialApartments;
using RevitCore.Utils;
using Serilog.Core;
using System.Reflection;


namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class ApartmentValidation_Command : ExternalCommand
    {

        public override void Execute()
        {
            try
            {
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                Host.GetService<ApartmentValidationShowWindow>().Execute();

            }
            catch
            {

            }
        }

        public static void CreateApartmentValidationButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"Area & Width\nValidation", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Apartment validation using Standards",
                LargeImage = ImageUtils.LoadImage(assembly, "icon_32x32.png")
            });

        }
    }
}
