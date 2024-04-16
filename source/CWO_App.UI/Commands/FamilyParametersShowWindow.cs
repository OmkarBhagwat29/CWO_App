using CWO_App.UI.Utils;
using CWO_App.UI.Views;
using CWO_App.UI.Views.SharedParameterViews;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Commands
{
    public class FamilyParametersShowWindow(IServiceProvider serviceProvider)
    {
        public void Execute()
        {

            if (WindowController.Focus<FamilyParameters_Window>()) return;

            var view = serviceProvider.GetService<FamilyParameters_Window>();
            WindowController.Show(view, Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
