using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CWO_App.UI.Models;
using CWO_App.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CWO_App.UI.ViewModels
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class BrickViewModel : IExternalCommand
    {
        UIDocument uidoc { get; set; }
        Document doc { get; set; }
        string mask = "2mm Arial Blue";
        public ObservableCollection<BrickSpecial> brickSpecials {get; set;}
        public ObservableCollection<BrickSpecial> history {get; set;}

        BrickWindow window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            brickSpecials = new ObservableCollection<BrickSpecial>();
            history = new ObservableCollection<BrickSpecial>();

            try
            {
                window = new BrickWindow(this);
                window.mask.Text = mask;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Topmost = true;

                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }
        }

        public void Clear()
        {
            try
            {
                ICollection<ElementId> selectElemId = uidoc.Selection.GetElementIds();
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
                        filteredElems.ForEach(clearDim);
                        t.Commit();
                    }
                }
                else
                {
                    TaskDialog.Show("test", $"No dimension of type '{mask}' was found!");
                    //return Result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("test", $"Error: {ex.Message}");
                //return Result.Failed;
            }
        }
        public void Run(string m) {
            mask = m;

            try
            {
                ICollection<ElementId> selectElemId = uidoc.Selection.GetElementIds();
                List<Element> selectedElems = new List<Element>();
                List<Element> filteredElems = new List<Element>();
                foreach (var item in selectElemId)
                {
                    selectedElems.Add(doc.GetElement(item));
                }
                if (selectedElems.Count > 0)
                {
                    filteredElems = selectedElems.Where(x => x.Category.Name == "Dimensions" && x.Name == mask).ToList();
                }

                if (filteredElems.Count > 0)
                {
                    brickSpecials.Clear();
                    using (Transaction t = new Transaction(doc, "Update Dimensions"))
                    {
                        t.Start();
                        filteredElems.ForEach(checkDim);
                        t.Commit();
                    }
                    //window.brickSpecials = brickSpecials;
                }
                else
                {
                    TaskDialog.Show("test", $"No dimension of type '{mask}' was found!");
                    //return Result.Succeeded;
                }


                //return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("test", $"Error: {ex.Message}");
                //return Result.Failed;
            }
        }

        public void clearDim(Element ele)
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

        public void checkDim(Element ele)
        {
            Dimension dim = ele as Dimension;
            if (dim.Segments.Size > 0)
            {
                foreach (DimensionSegment segment in dim.Segments)
                {
                    String valueStr = segment.ValueString.Contains(" ") ? segment.ValueString.Split(' ')[1] : segment.ValueString;
                    float value = float.Parse(valueStr);
                    string above = checkValue(value);
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
                string above = checkValue(value);
                dim.Above = above;
                if (above == "BS") 
                {
                    if (!brickSpecials.Where(x => x.Value.Equals(value)).Any())
                        brickSpecials.Add(new BrickSpecial(value));
                }
            }
        }

        public String checkValue(float num)
        {
            String result = "BS";
            float x = num % 112.5f;
            if (x == 102.5f) result = "CO-";
            else if (x == 0) result = "CO";
            else if (x == 10) result = "CO+";

            return result;
        }

        public void addToHistory(float f)
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

        public void CheckNumber(String s)
        {
            float f = float.Parse(s);
            float brickDim = f % 112.5f;
            float minCOMinus = f - brickDim - 10.0f;
            float minCO = f - brickDim;
            float minCOPlus = f - brickDim + 10.0f;
            float maxCOMinus = minCOMinus + 112.5f;
            float maxCO = minCOMinus + 122.5f;
            float maxCOPlus = minCOMinus + 132.5f;

            string message;
            window.checkResult.Inlines.Clear();

            if (brickDim == 102.5f)
            {
                message = $"{f} is a CO- brick dimension.{Environment.NewLine}CO = {f+10.0f}{Environment.NewLine}CO+ = {f+20.0f}{Environment.NewLine}Bricks = {(f+10.0f)/112.5f/2.0}";
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
                    $"-- or --{Environment.NewLine}{maxCOMinus} / {maxCO} / {maxCOPlus}{Environment.NewLine}Modulus = {brickDim}{Environment.NewLine}Difference = {Math.Abs(brickDim-112.5f)}";
            }

            window.checkResult.Inlines.Add(message);
            addToHistory(f);
        }
    }
}
