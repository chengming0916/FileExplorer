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
    #region UI Events
    public class ViewChangedEvent : ViewModelEvent
    {
        public string ViewMode { get; set; }
        public string OldViewMode { get; set; }

        public ViewChangedEvent(object sender, string viewMode, string oldViewMode)
            : base(sender)
        {
            ViewMode = viewMode;
            OldViewMode = oldViewMode;
        }
    }

    public class ViewModelEvent
    {
        public object Sender { get; private set; }

        public ViewModelEvent(object sender)
        {
            Sender = sender;
        }
    }

    public class DirectoryChangedEvent : ViewModelEvent
    {
        public IEntryViewModel OriginalViewModel { get; private set; }
        public IEntryModel OriginalModel { get { return OriginalViewModel== null ? null : OriginalViewModel.EntryModel; } }
        public IEntryViewModel NewViewModel { get; private set; }
        public IEntryModel NewModel { get { return NewViewModel == null ? null : NewViewModel.EntryModel; } }


        public DirectoryChangedEvent(object sender, IEntryViewModel newVM, IEntryViewModel originalVM)
            : base(sender)
        {
            NewViewModel = newVM;
            OriginalViewModel = originalVM;
        }

        public DirectoryChangedEvent(object sender, IEntryModel newM, IEntryModel originalM)
            : this(sender, EntryViewModel.FromEntryModel(newM), EntryViewModel.FromEntryModel(originalM))
        {
            
        }
    }

    public class SelectionChangedEvent : ViewModelEvent
    {
        public IEnumerable<IEntryViewModel> SelectedViewModels { get; private set; }
        public IEnumerable<IEntryModel> SelectedModels { get { return from vm in SelectedViewModels select vm.EntryModel; } }

        public SelectionChangedEvent(object sender, IEnumerable<IEntryViewModel> evms)
            : base(sender)
        {
            SelectedViewModels = evms.ToList();
        }
    }

    public class ListCompletedEvent : ViewModelEvent
    {
        public IEnumerable<IEntryViewModel> ListedViewModels { get; private set; }
        public IEnumerable<IEntryModel> ListedModels { get { return from vm in ListedViewModels select vm.EntryModel; } }

        public ListCompletedEvent(object sender, IEnumerable<IEntryViewModel> evms)
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
    #endregion    

    public class TransferProgress 
    {
        public Int32? TotalEntriesIncrement { get; set; }
        public Int32? ProcessedEntriesIncrement { get; set; }
        public short? CurrentProgressPercent { get; set; }

        public static TransferProgress IncrementTotalEntries(int count = 1) 
        {
            return new TransferProgress() { TotalEntriesIncrement = count };
        }
        public static TransferProgress IncrementProcessedEntries(int count = 1)
        {
            return new TransferProgress() { ProcessedEntriesIncrement = count };
        }
    }

    public class NullTransferProgress : IProgress<TransferProgress>
    {
        public static NullTransferProgress Instance = new NullTransferProgress();
        public void Report(TransferProgress value)
        {            
        }
    }

    public class EntryChangedEvent
    {
        public EntryChangedEvent(string parseName, ChangeType changeType)
        {
            ChangeType = changeType;
            ParseName = parseName;
        }

        public EntryChangedEvent(string parseName, string orgParseName)
            : this(parseName, ChangeType.Moved)
        {
            OrgParseName = orgParseName;
        }

        public ChangeType ChangeType { get; private set; }
        public string ParseName { get; private set; }
        public string OrgParseName { get; private set; }
    }
  

    public class ColumnInfo
    {
        public string Header { get; set; }
        public string ValuePath { get; set; }
        public string TooltipPath { get; set; }
        public double Width { get; set; }
        public string TemplateKey { get; set; }
        public IComparer<IEntryModel> Comparer { get; set; }

        private ColumnInfo(string header, double width)
        {
            Header = header;
            Width = width;
        }

        public static ColumnInfo FromTemplate(string header, string templateKey,
            string valuePath = null, IComparer<IEntryModel> comparer = null, double width = double.NaN)
        {
            return new ColumnInfo(header, width)
            {
                Comparer = comparer,
                ValuePath = valuePath,
                TemplateKey = templateKey
            };
        }

        public static ColumnInfo FromBindings(string columnHeader, string valuePath, string tooltipPath,
            IComparer<IEntryModel> comparer = null, double width = double.NaN)
        {
            return new ColumnInfo(columnHeader, width)
            {
                Comparer = comparer,
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
