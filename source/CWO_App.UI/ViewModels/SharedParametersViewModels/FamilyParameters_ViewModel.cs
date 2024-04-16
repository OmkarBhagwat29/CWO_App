using CWO_App.UI.Models;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.SharedParametersViewModels
{
    public sealed partial class FamilyParameters_ViewModel: ObservableObject
    {
        private readonly ILogger _logger;
        public IWindowService WindowService;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private FamilyParametersModel _model;

        private string lastOpenedFolder = "";

        [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParametersData = [];
        [ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familiesData = [];


        [ObservableProperty]
        private bool _allParametersSelected;

        public FamilyParameters_ViewModel(ILogger<FamilyParameters_ViewModel> logger, IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            // Subscribe to the WindowOpened event
            WindowService.WindowOpened += OnWindowOpened;
        }

        partial void OnAllParametersSelectedChanged(bool oldValue, bool newValue)
        {
            foreach (var item in this.SharedParametersData)
            {
                item.IsSelected = oldValue;
            }
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            //get shared parameters 
            _externalHandler.Raise((uiApp) =>
            {
                var app = uiApp.Application;
                var definitionFile = app.OpenSharedParameterFile();
                if (definitionFile == null)
                {
                    _logger.LogInformation("NO shared parameter file found in the Project");
                    return;
                }
                
                _model = new FamilyParametersModel(definitionFile);
                _model.SetParameters();

                this.SharedParametersData.Clear();
                _model.AllParameters
                .ForEach(d => this.SharedParametersData.Add(new SharedParameterDataRow()
                {
                    IsSelected = true,
                    ParameterGroup = d.groupName,
                    ParameterName = d.parameterName
                }));

            });
        }

    }
}
