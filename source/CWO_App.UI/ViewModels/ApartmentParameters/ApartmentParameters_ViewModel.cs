using Autodesk.Revit.UI;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public sealed partial class ApartmentParameters_ViewModel : ObservableObject
    {
        private readonly ILogger _logger;
        public IWindowService WindowService;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        Element _roomElement;
        Element _areaElement;

        public ApartmentParameters_ViewModel(ILogger<ApartmentParameters_ViewModel> logger,
            IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            WindowService.WindowOpened += OnWindowOpened;
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            try
            {
                TaskDialog.Show("Apartment Parameters", "Hello Apartment Parameters!!!");
            }
            catch
            {

            }

        }
    }
}
