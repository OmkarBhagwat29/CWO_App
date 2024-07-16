
using Autodesk.Revit.UI;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using System.Collections.ObjectModel;
using RevitCore.Extensions;
using CWO_App.UI.Models;
using RevitCore.Extensions.Selection;

namespace CWO_App.UI.ViewModels
{
    public sealed partial class BrickEvaluator_ViewModel: ObservableObject
    {
        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private readonly WallTagsModel _model = new();

        public ObservableCollection<BrickSpecial> brickSpecials { get; set; } = [];
        public ObservableCollection<BrickSpecial> history { get; set; } = [];

        private readonly ILogger _logger;
        public IWindowService WindowService;

        [ObservableProperty] private string _selectedDimensionType;
        [ObservableProperty] private ObservableCollection<string> _dimensionTypes = [];

        [ObservableProperty] private float _wallLength = 900;

        [ObservableProperty] private string _checkResultText = "Click on 'Check' to see results";

        public BrickEvaluator_ViewModel(ILogger<BrickEvaluator_ViewModel> logger, IWindowService windowService)
        {
            // Constructor initialization...
            _logger = logger;
            WindowService = windowService;

            // Subscribe to the WindowOpened event
            WindowService.WindowOpened += OnWindowOpened;
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            _externalHandler.Raise(uiApp=>SetLinearDimensionTypes(uiApp));
        }

        [RelayCommand]
        private async Task SelectDimensions()
        {
            await _asyncExternalHandler.RaiseAsync((uiApp) =>
            {

                _model.SelectedDimensions = uiApp.ActiveUIDocument.PickElements((e) =>
                  {
                      if (!(e is Dimension lD))
                          return false;

                      if (lD.Name != this.SelectedDimensionType) return false;

                      return true;
                  }, PickElementOptionFactory.CreateCurrentDocumentOption()).Cast<Dimension>();

                _logger.LogInformation("Selection Successful");
            });
        }

        [RelayCommand]
        private async Task GenerateAboveText()
        {
            await _asyncExternalHandler?.RaiseAsync((uiApp) => {

                uiApp.ActiveUIDocument.Document.UseTransaction(_model.GenerateDimensionAboveTags, "Above Tags Generated");
            });
        }

        [RelayCommand]
        public async Task Clear()
        {
            try
            {
                await _asyncExternalHandler.RaiseAsync(uiApp => {

                    var doc = uiApp.ActiveUIDocument.Document;

                    ICollection<ElementId> selectElemId = uiApp.ActiveUIDocument
                    .Selection.GetElementIds();
                    List<Element> selectedElems = new List<Element>();
                    List<Element> filteredElems = new List<Element>();
                    foreach (var item in selectElemId)
                    {
                        selectedElems.Add(doc.GetElement(item));
                    }
                    if (selectedElems.Count > 0)
                    {
                        filteredElems = selectedElems.Where(x => x.Category.Name == "Dimensions").ToList();
                    }

                    if (filteredElems.Count > 0)
                    {

                        using (Transaction t = new Transaction(doc, "Clear Dimensions"))
                        {
                            t.Start();
                            filteredElems.ForEach(ClearDim);
                            t.Commit();
                        }
                    }
                    else
                    {
                        TaskDialog.Show("test", $"No dimension of type '{this.SelectedDimensionType}' was found!");
                        //return Result.Succeeded;
                    }
                });

            }
            catch (Exception ex)
            {
                TaskDialog.Show("test", $"Error: {ex.Message}");
                //return Result.Failed;
            }
        }

        private void ClearDim(Element ele)
        {
            Dimension dim = ele as Dimension;
            if (dim.Segments.Size > 0)
            {
                foreach (DimensionSegment segment in dim.Segments)
                {
                    segment.Above = "";
                }
            }
            else
            {
                dim.Above = "";
            }
        }

        [RelayCommand]
        public async Task Run(string m)
        {
           
            try
            {

                await _asyncExternalHandler.RaiseAsync(uiApp => {

                    var doc = uiApp.ActiveUIDocument.Document;

                    ICollection<ElementId> selectElemId = uiApp.ActiveUIDocument
                    .Selection.GetElementIds();
                    List<Element> selectedElems = new List<Element>();
                    List<Element> filteredElems = new List<Element>();
                    foreach (var item in selectElemId)
                    {
                        selectedElems.Add(doc.GetElement(item));
                    }
                    if (selectedElems.Count > 0)
                    {
                        filteredElems = selectedElems.Where(x =>
                        x.Category.Name == "Dimensions" && x.Name == this.SelectedDimensionType).ToList();
                    }

                    if (filteredElems.Count > 0)
                    {
                        brickSpecials.Clear();
                        using (Transaction t = new Transaction(doc, "Update Dimensions"))
                        {
                            t.Start();
                            filteredElems.ForEach(CheckDim);
                            t.Commit();
                        }
                        //window.brickSpecials = brickSpecials;
                    }
                    else
                    {
                        TaskDialog.Show("test", $"No dimension of type '{this.SelectedDimensionType}' was found!");
                        //return Result.Succeeded;
                    }

                });

            }
            catch (Exception ex)
            {
                TaskDialog.Show("test", $"Error: {ex.Message}");
                //return Result.Failed;
            }
        }

        private void CheckDim(Element ele)
        {
            Dimension dim = ele as Dimension;
            if (dim.Segments.Size > 0)
            {
                foreach (DimensionSegment segment in dim.Segments)
                {
                    String valueStr = segment.ValueString.Contains(" ") ? segment.ValueString.Split(' ')[1] : segment.ValueString;
                    float value = float.Parse(valueStr);
                    string above = CheckValue(value);
                    segment.Above = above;
                    if (above == "BS")
                    {
                        if (!brickSpecials.Where(x => x.Value.Equals(value)).Any())
                            brickSpecials.Add(new BrickSpecial(value));
                    }
                }
            }
            else
            {
                String valueStr = dim.ValueString.Contains(" ") ? dim.ValueString.Split(' ')[1] : dim.ValueString;
                float value = float.Parse(valueStr);
                string above = CheckValue(value);
                dim.Above = above;
                if (above == "BS")
                {
                    if (!brickSpecials.Where(x => x.Value.Equals(value)).Any())
                        brickSpecials.Add(new BrickSpecial(value));
                }
            }
        }


        private string CheckValue(float num)
        {
            string result = "BS";
            float x = num % 112.5f;
            if (x == 102.5f) result = "CO-";
            else if (x == 0) result = "CO";
            else if (x == 10) result = "CO+";

            return result;
        }

        [RelayCommand]
        public void CheckNumber()
        {
            try
            {
                float f = this.WallLength;


                float brickDim = f % 112.5f; //65
                float minCOMinus = f - brickDim - 10.0f;
                float minCO = f - brickDim;
                float minCOPlus = f - brickDim + 10.0f;
                float maxCOMinus = minCOMinus + 112.5f; //65
                float maxCO = minCOMinus + 122.5f; //75
                float maxCOPlus = minCOMinus + 132.5f; //85

                string message;
                //window.checkResult.Inlines.Clear();
                this.CheckResultText = string.Empty;

                if (brickDim == 102.5f)
                {
                    message = $"{f} is a CO- brick dimension.{Environment.NewLine}CO = {f + 10.0f}{Environment.NewLine}CO+ = {f + 20.0f}{Environment.NewLine}Bricks = {(f + 10.0f) / 112.5f / 2.0}";
                }
                else if (brickDim == 0)
                {
                    message = $"{f} is a CO brick dimension.{Environment.NewLine}CO- = {f - 10.0f}{Environment.NewLine}CO+ = {f + 10.0f}{Environment.NewLine}Bricks = {f / 112.5f / 2.0}";
                }
                else if (brickDim == 10)
                {
                    message = $"{f} is a CO+ brick dimension.{Environment.NewLine}CO = {f - 10.0f}{Environment.NewLine}CO- = {f - 20.0f}{Environment.NewLine}Bricks = {(f - 10.0f) / 112.5f / 2.0}";
                }
                else
                {
                    message = $"{f} is a Brick Special.{Environment.NewLine}See below closest brick dimensions:{Environment.NewLine}{minCOMinus} / {minCO} / {minCOPlus}{Environment.NewLine}" +
                        $"-- or --{Environment.NewLine}{maxCOMinus} / {maxCO} / {maxCOPlus}{Environment.NewLine}Modulus = {brickDim}{Environment.NewLine}Difference = {Math.Abs(brickDim - 112.5f)}";
                }

                //window.checkResult.Inlines.Add(message);
                this.CheckResultText = message;
                AddToHistory(f);
            }
            catch
            {

            }

        }

        private void AddToHistory(float f)
        {
            if (!history.Where(x => x.Value.Equals(f)).Any())
            {
                if (history.Count > 5)
                {
                    history.RemoveAt(history.Count - 1);
                }
                history.Insert(0, new BrickSpecial(f));
            }
        }


        private void SetLinearDimensionTypes(UIApplication uiApp)
        {
            var dimensions = uiApp.ActiveUIDocument.Document
                .GetInstancesOfCategory(BuiltInCategory.OST_Dimensions, (e) => {

                    if (!(e is Dimension d))
                        return false;

                    return d.DimensionShape == DimensionShape.Linear;
                }).Cast<Dimension>();

            List<string> linearDimensionNames = dimensions.Select(e => e.Name).Distinct().ToList();

            if (linearDimensionNames == null || linearDimensionNames.Count == 0)
            {
#if DEBUG
                _logger.LogError("No Linear Dimensions Found in the Project.");
#else
_logger.LogError("No Linear Dimensions Found in the Project.");
TaskDialog.Show("Message", "No Linear Dimensions Found in the Project");
#endif
                return;
                
            }
            this.DimensionTypes.Clear();
            linearDimensionNames.ForEach(name=> this.DimensionTypes.Add(name));
            this.SelectedDimensionType = this.DimensionTypes.FirstOrDefault();

            WallTagsModel.AvailableDimensionTypes = dimensions;
        }

    }
}
