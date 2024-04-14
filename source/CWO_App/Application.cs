
using CWO_App.Commands;
using Nice3point.Revit.Toolkit.External;

namespace CWO_App
{
    /// <summary>
    /// Application Entry Point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            try
            {
                Host.Start();

                //Create Tab
                string tabName = "CWO APP";
                Application.CreateRibbonTab(tabName);

                string parameterPanelName = "Parameters";

               var panel = Application.CreatePanel(parameterPanelName, tabName);

                BrickEvaluator_Command.CreateBrickEvaluatorButton(panel);
            }
            catch
            {

            }

        }

        public override void OnShutdown()
        {
            Host.Stop();
        }

    }
}
