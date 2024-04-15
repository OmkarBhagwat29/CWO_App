



using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using Serilog.Core;

namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Template_Command : ExternalCommand
    {

        public override void Execute()
        {
			try
			{
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;


			}
			catch
			{
                
			}
        }
    }
}
