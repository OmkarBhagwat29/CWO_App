using CWO_App.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using CWO_App.UI.Utils;
using System.Diagnostics;


namespace CWO_App.UI.Commands
{
    /// <summary>
    ///     Command entry point invoked from the Revit AddIn Application
    /// </summary>
    public class BrickEvaluatorShowWindow(IServiceProvider serviceProvider)
    {
        public void Execute()
        {
            
            if (WindowController.Focus<BrickEvaluator_View>()) return;

            var view = serviceProvider.GetService<BrickEvaluator_View>();
            WindowController.Show(view, Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
