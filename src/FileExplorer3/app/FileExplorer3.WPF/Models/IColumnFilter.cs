using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IColumnFilter
    {
        string Header { get; }
        long MatchedCount { get; }
        bool IsChecked { get; set; }
        string ValuePath { get;}
        Func<IEntryModel, bool> Matches { get; }
    }
}
