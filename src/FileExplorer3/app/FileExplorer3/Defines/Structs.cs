using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using FileExplorer.ViewModels;

namespace FileExplorer.Defines
{
    [Obsolete]
    public class ViewChangedEvent
    {
        string ViewMode { get; set; }
        string OldViewMode { get; set; }

        public ViewChangedEvent(string viewMode, string oldViewMode)
        {
            ViewMode = viewMode;
            OldViewMode = oldViewMode;
        }
    }

    public class SelectionChangedEvent
    {
        public IEnumerable<IEntryViewModel> SelectedViewModels { get; private set; }
        public IEnumerable<IEntryModel> SelectedModels { get { return from vm in SelectedViewModels select vm.EntryModel; } }
        
        public SelectionChangedEvent(IEnumerable<IEntryViewModel> evms)
        {
            SelectedViewModels = evms.ToList();
        }
    }

    public static class ListViewColumnInfoExtension
    {
        public static ListViewColumnInfo Find(this ListViewColumnInfo[] cols, string valuePath)
        {
            foreach (var col in cols)
                if (col.ValuePath.Equals(valuePath) || col.Header.Equals(valuePath))
                    return col;
            return null;
        }
    }

    public class ListViewColumnInfo
    {
        public string Header { get; set; }
        public string ValuePath { get; set; }
        public string TooltipPath { get; set; }
        public double Width { get; set; }
        public string TemplateKey { get; set; }

        private ListViewColumnInfo(string header, double width)
        {
            Header = header;
            Width = width;
        }

        public static ListViewColumnInfo FromTemplate(string header, string templateKey,
            string valuePath = null, double width = double.NaN)
        {
            return new ListViewColumnInfo(header, width)
            {
                ValuePath = valuePath,
                TemplateKey = templateKey
            };
        }

        public static ListViewColumnInfo FromBindings(string columnHeader, string valuePath, string tooltipPath,
            double width = double.NaN)
        {
            return new ListViewColumnInfo(columnHeader, width)
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
            return (obj is ListViewColumnInfo && (obj as ListViewColumnInfo).ValuePath == ValuePath) ||
                (obj is string && (obj as String).Equals(ValuePath));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
