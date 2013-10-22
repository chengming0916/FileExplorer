using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class ListViewColumnInfo
    {
        public string Header { get; set; }
        public string ValuePath { get; set; }
        public string TooltipPath { get; set; }
        public double Width { get; set; }
        public string TemplateKey { get; set; }

        private ListViewColumnInfo(string columnHeader, double columnWidth)
        {
            Header = columnHeader;
            Width = columnWidth;
        }

        public static ListViewColumnInfo FromTemplate(string columnHeader, string templateKey, double columnWidth = double.NaN)
        {
            return new ListViewColumnInfo(columnHeader, columnWidth)
            {
                TemplateKey = templateKey
            };
        }

        public static ListViewColumnInfo FromBindings(string columnHeader, string columnValuePath, string columnTooltipPath,
            double columnWidth = double.NaN)
        {
            return new ListViewColumnInfo(columnHeader, columnWidth)
            {
                ValuePath = columnValuePath,
                TooltipPath = columnTooltipPath
            };
        }
    }
}
