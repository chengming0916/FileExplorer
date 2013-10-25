using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FileExplorer.Defines;

namespace FileExplorer.UserControls
{
    public static class ListViewColumnUtils
    {
        public static GridViewColumn createColumn(ListView listView, ListViewColumnInfo colInfo)
        {
            DataTemplate dt = null;
            if (colInfo.TemplateKey != null)
                dt = listView.FindResource(colInfo.TemplateKey) as DataTemplate;
            else
            {
                dt = new DataTemplate();
                FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
                label.SetBinding(TextBlock.TextProperty, new Binding(colInfo.ValuePath));
                if (!(String.IsNullOrEmpty(colInfo.TooltipPath)))
                    label.SetValue(TextBlock.ToolTipProperty, new Binding(colInfo.TooltipPath));
                label.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
                label.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);

                dt.VisualTree = label;
            }
            return new GridViewColumn()
            {
                Header = colInfo.Header,
                CellTemplate = dt,
                Width = colInfo.Width
            };
        }

        public static void AddColumn(ListView listView, GridViewColumnCollection columnCol, ListViewColumnInfo[] colInfos)
        {
            foreach (var colInfo in colInfos)
            {
                if (columnCol.Any(c => c.Header.Equals(colInfo.Header))) //Prevent re-add.
                    break;
                else columnCol.Add(createColumn(listView, colInfo));
            }
        }

        public static void UpdateColumn(ListView listView, ListViewColumnInfo[] colInfos)
        {
            GridViewColumnCollection columnCol = getColumnCols(listView);
            if (columnCol != null)
            {
                columnCol.Clear();
                AddColumn(listView, columnCol, colInfos);
            }
        }

        private static GridViewColumnCollection getColumnCols(ListView listView)
        {
            if (listView.View is GridView)
                return (listView.View as GridView).Columns;
            else
                if (listView.View is VirtualWrapPanelView)
                    return (listView.View as VirtualWrapPanelView).Columns;
                else if (listView.View is VirtualStackPanelView)
                    return (listView.View as VirtualStackPanelView).Columns;
            return null;
        }

        /// <summary>
        /// Apply header template to GridViewColumns depending if it's selected and ascending.
        /// </summary>
        public static void UpdateSortSymbol(ListView listView, ListViewColumnInfo sortCol,
            ListSortDirection sortDirection = ListSortDirection.Ascending,
            string normalHeaderTemplate = "NormHeaderTemplate",
            string ascHeaderTemplate = "AscHeaderTemplate",
            string descHeaderTemplate = "DescHeaderTemplate")
        {            
            GridViewColumnCollection columns = getColumnCols(listView);

            if (columns == null)
                return;

            DataTemplate ascTemplate = listView.TryFindResource(ascHeaderTemplate) as DataTemplate;
            DataTemplate descTemplate = listView.TryFindResource(descHeaderTemplate) as DataTemplate;
            DataTemplate normTemplate = listView.TryFindResource(normalHeaderTemplate) as DataTemplate;

            //handle if sortCol = null

            GridViewHeaderRowPresenter presenter = UITools.FindVisualChild<GridViewHeaderRowPresenter>(listView);

            Func<TextBlock, bool> filter = tb =>
                {
                    var value = tb.GetValue(TextBlock.TextProperty);
                    var match = sortCol.Header != null && sortCol.Header.Equals(value);
                    return match;
                };
            GridViewColumnHeader foundHeader = null;

            TextBlock lookup = UITools.FindVisualChild<TextBlock>(presenter, filter);
            if (lookup != null)
                foundHeader = lookup.FindAncestor<GridViewColumnHeader>();
            IEnumerable<GridViewColumnHeader> headers = UITools.FindAllVisualChildren<GridViewColumnHeader>(presenter);

            if (ascTemplate != null && descTemplate != null && normTemplate != null)
                foreach (var curHeader in headers)
                {                    
                    if (curHeader.Equals(foundHeader))
                    {
                        if (sortDirection == ListSortDirection.Ascending)
                            ListViewEx.SetColumnHeaderSortDirection(curHeader, -1);
                        else ListViewEx.SetColumnHeaderSortDirection(curHeader, 1);
                    }
                    else
                        ListViewEx.SetColumnHeaderSortDirection(curHeader, 0);
                }
        }
    }
}
