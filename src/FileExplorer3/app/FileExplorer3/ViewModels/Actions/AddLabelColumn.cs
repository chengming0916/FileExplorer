using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class AddLabelColumn : IResult
    {
        #region Cosntructor

        public AddLabelColumn(string columnHeader, string columnValuePath, string columnTooltipPath)
        {
            _columnHeader = columnHeader;
            _columnValuePath = columnValuePath;
            _columnTooltipPath = columnTooltipPath;
        }

        #endregion

        #region Methods


        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            FrameworkElement ele = context.Source as FrameworkElement;
            Exception ex = null;
            while (ele != null)
            {
                var found = UITools.FindVisualChildByName<ListView>(ele, "Items");
                if (found != null)
                {
                    if (found.View is GridView)
                    {
                        var gView = (found.View as GridView);
                        var dt = new DataTemplate();
                        FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
                        label.SetBinding(TextBlock.TextProperty, new Binding(_columnValuePath));
                        if (!(String.IsNullOrEmpty(_columnTooltipPath)))
                            label.SetValue(TextBlock.ToolTipProperty, new Binding(_columnTooltipPath));
                        dt.VisualTree = label;
                        gView.Columns.Add(new GridViewColumn()
                        {
                            Header = _columnHeader,
                            CellTemplate = dt
                        });
                    }

                    break;
                }
                ele = ele.Parent as FrameworkElement;
            }

            if (ele == null)
                ex = new Exception("Cannot find ListView named Items in FileList control");

            Completed(this, new ResultCompletionEventArgs() { Error = ex });
        }

        #endregion

        #region Data

        string _columnValuePath, _columnTooltipPath;
        string _columnHeader;

        #endregion

        #region Public Properties

        #endregion
    }
}
