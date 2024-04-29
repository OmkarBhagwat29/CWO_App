



using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitCore.AreaRoomAssociation;
using Serilog.Core;

namespace CWO_App.Commands
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class SelectAreaAndRoomBoundaries : ExternalCommand
    {

        public override void Execute()
        {
			try
			{
                var app = Application;
                var uiApp = ExternalCommandData.Application;
                var uiDoc = uiApp.ActiveUIDocument;
                var doc = uiDoc.Document;

                AreaRoomAssociation.CreateAreaRoomAssociationBySelection(uiDoc);


			}
			catch
			{
                
			}
        }
    }
}
