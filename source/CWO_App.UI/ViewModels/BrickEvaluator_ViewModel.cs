
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CWO_App.UI.ViewModels
{
    public sealed partial class BrickEvaluator_ViewModel(ILogger<BrickEvaluator_ViewModel> logger) : ObservableObject
    {
        [ObservableProperty] private string _selectedDimensionType;
        [ObservableProperty] private ObservableCollection<string> _dimensionTypes = new ObservableCollection<string>();


        [RelayCommand]
        private void SelectDimensions()
        {
            
        }



        //public BrickEvaluator_ViewModel()
        //{
        //    //this.SelectDimensionsBtn = new RelayCommand(OnSelectDimensions);
        //    //Messenger.Default.Register<DimensionTypesMessage>(this, SetDimensionTypes);
        //}

        //private void SetDimensionTypes(DimensionTypesMessage message)
        //{
        //    this.DimensionTypes.Clear();

        //    message.DimensionTypeNames.ForEach(name => this.DimensionTypes.Add(name));
        //    this.SelectedDimensionType = this.DimensionTypes[0];
        //}

        //void OnSelectDimensions()
        //{
        //    RequestEvaluate?.Invoke(this, new BrickTagsModel(BrickTagButtons.Select,this.SelectedDimensionType));
        //}
    }
}
