using CWO_App.UI.Utils;
using CWO_App.UI.Views.ApartmentParametersViews;
using CWO_App.UI.Views.ApartmentValidationViews;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Commands
{
    public class ApartmentParametersShowWindow(IServiceProvider serviceProvider)
    {
        public void Execute()
        {

            if (WindowController.Focus<ApartmentParameters_Window>()) return;

            var view = serviceProvider.GetService<ApartmentParameters_Window>();
            WindowController.Show(view, Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
