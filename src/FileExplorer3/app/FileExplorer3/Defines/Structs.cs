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

        public override bool Equals(object obj)
        {
            return obj is ListViewColumnInfo && (obj as ListViewColumnInfo).ValuePath == ValuePath;
        }
    }
}
