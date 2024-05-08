using CWO_App.UI.Utils;
using CWO_App.UI.Views.ApartmentValidationViews;
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
    public class ApartmentValidationShowWindow(IServiceProvider serviceProvider)
    {
        public void Execute()
        {

            if (WindowController.Focus<ApartmentValidation_Window>()) return;

            var view = serviceProvider.GetService<ApartmentValidation_Window>();
            WindowController.Show(view, Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
