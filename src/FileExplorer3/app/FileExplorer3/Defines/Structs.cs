using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.UserControls;
using FileExplorer.ViewModels;

namespace FileExplorer.Defines
{
    public class ViewChangedEvent : ViewModelEvent
    {
        public string ViewMode { get; set; }
        public string OldViewMode { get; set; }

        public ViewChangedEvent(PropertyChangedBase sender, string viewMode, string oldViewMode)
            : base(sender)
        {
            ViewMode = viewMode;
            OldViewMode = oldViewMode;
        }
    }

    public class ViewModelEvent
    {
        public PropertyChangedBase Sender { get; private set; }

        public ViewModelEvent(PropertyChangedBase sender)
        {
            Sender = sender;
        }
    }


    public class SelectionChangedEvent : ViewModelEvent
    {
        public IEnumerable<IEntryViewModel> SelectedViewModels { get; private set; }
        public IEnumerable<IEntryModel> SelectedModels { get { return from vm in SelectedViewModels select vm.EntryModel; } }

        public SelectionChangedEvent(PropertyChangedBase sender, IEnumerable<IEntryViewModel> evms)
            : base(sender)
        {
            SelectedViewModels = evms.ToList();
        }
    }

    public class ListCompletedEvent : ViewModelEvent
    {
        public IEnumerable<IEntryViewModel> ListedViewModels { get; private set; }
        public IEnumerable<IEntryModel> ListedModels { get { return from vm in ListedViewModels select vm.EntryModel; } }

        public ListCompletedEvent(PropertyChangedBase sender, IEnumerable<IEntryViewModel> evms)
            : base(sender)
        {
            ListedViewModels = evms.ToList();
        }
    }
    


    public class FilterChangedEventArgs : RoutedEventArgs
    {
        public FilterChangedEventArgs(object source)
            : base(ListViewEx.FilterChangedEvent)
        {

        }
        public ColumnInfo ColumnInfo { get; set; }
        public ColumnFilter Filter { get; set; }
    }

    public static class ListViewColumnInfoExtension
    {
        public static ColumnInfo Find(this ColumnInfo[] cols, string valuePath)
        {
            foreach (var col in cols)
                if (col.ValuePath.Equals(valuePath) || col.Header.Equals(valuePath))
                    return col;
            return null;
        }
    }

  

    public class ColumnInfo
    {
        public string Header { get; set; }
        public string ValuePath { get; set; }
        public string TooltipPath { get; set; }
        public double Width { get; set; }
        public string TemplateKey { get; set; }

        private ColumnInfo(string header, double width)
        {
            Header = header;
            Width = width;
        }

        public static ColumnInfo FromTemplate(string header, string templateKey,
            string valuePath = null, double width = double.NaN)
        {
            return new ColumnInfo(header, width)
            {
                ValuePath = valuePath,
                TemplateKey = templateKey
            };
        }

        public static ColumnInfo FromBindings(string columnHeader, string valuePath, string tooltipPath,
            double width = double.NaN)
        {
            return new ColumnInfo(columnHeader, width)
            {
                ValuePath = valuePath,
                TooltipPath = tooltipPath
            };
        }

        public override string ToString()
        {
            return String.Format("Header:{0}, Key:{1}, ValuePath:{2}", Header, TemplateKey, ValuePath);
        }

        public override bool Equals(object obj)
        {
            return (obj is ColumnInfo && (obj as ColumnInfo).ValuePath == ValuePath) ||
                (obj is string && (obj as String).Equals(ValuePath));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
