using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class AddLabelColumn : IResult
    {
        #region Cosntructor

        public AddLabelColumn(params ListViewColumnInfo[] colInfos)
        {
            _colInfos = colInfos;
        }

        #endregion

        #region Methods


        public event EventHandler<ResultCompletionEventArgs> Completed;

        private GridViewColumn createColumn(ListView found,ListViewColumnInfo colInfo)
        {
            DataTemplate dt = null;
            if (colInfo.TemplateKey != null)
                dt = found.FindResource(colInfo.TemplateKey) as DataTemplate;
            else
            {
                dt = new DataTemplate();
                FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
                label.SetBinding(TextBlock.TextProperty, new Binding(colInfo.ValuePath));
                if (!(String.IsNullOrEmpty(colInfo.TooltipPath)))
                    label.SetValue(TextBlock.ToolTipProperty, new Binding(colInfo.TooltipPath));
                dt.VisualTree = label;
            }
            return new GridViewColumn()
                {
                    Header = colInfo.Header,
                    CellTemplate = dt,
                    Width = colInfo.Width
                };
        }


        private void addColumn(ListView found, GridViewColumnCollection columnCol, ListViewColumnInfo[] colInfos)
        {
            foreach (var colInfo in colInfos)
            {
                if (columnCol.Any(c => c.Header.Equals(colInfo.Header))) //Prevent re-add.
                    break;
                else columnCol.Add(createColumn(found, colInfo));
            }
        }

        private void addColumn(ListView found, ListViewColumnInfo[] colInfos)
        {
            GridViewColumnCollection columnCol = null;

            if (found.View is GridView)
                columnCol = (found.View as GridView).Columns;
            else if (found.View is VirtualWrapPanelView)
                columnCol = (found.View as VirtualWrapPanelView).Columns;
            else if (found.View is VirtualStackPanelView)
                columnCol = (found.View as VirtualStackPanelView).Columns;

            if (columnCol != null)
                addColumn(found, columnCol, colInfos);
        }

        public void Execute(ActionExecutionContext context)
        {

            FrameworkElement ele = context.Source as FrameworkElement;
            Exception ex = null;
            while (ele != null)
            {
                var found = UITools.FindVisualChildByName<ListView>(ele, "Items");
                if (found != null)
                {
                    addColumn(found, _colInfos);
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

        ListViewColumnInfo[] _colInfos;

        #endregion

        #region Public Properties

        #endregion
    }
}
