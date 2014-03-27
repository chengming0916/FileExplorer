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
        public IEntryModel OriginalModel { get { return OriginalViewModel == null ? null : OriginalViewModel.EntryModel; } }
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
        public string Source { get; set; }
        public IPathHelper SourcePathHelper { get; set; }
        public string Destination { get; set; }
        public IPathHelper DestinationPathHelper { get; set; }

        public static TransferProgress From(string src, IPathHelper srcPathHelper, string dest, IPathHelper destPathHelper)
        {
            return new TransferProgress()
            {
                Source = src,
                SourcePathHelper = srcPathHelper,
                Destination = dest,
                DestinationPathHelper = destPathHelper
            };
        }

        public static TransferProgress From(string src, IPathHelper pathHelper = null)
        {
            return From(src, pathHelper, null, null);
        }

        public static TransferProgress From(string src, string dest)
        {
            return From(src, PathHelper.Auto(src), dest, PathHelper.Auto(dest));
        }

        public static TransferProgress To(string dest, IPathHelper pathHelper = null)
        {
            return From(null, null, dest, pathHelper);
        }

        public static TransferProgress IncrementTotalEntries(int count = 1)
        {
            return new TransferProgress() { TotalEntriesIncrement = count };
        }
        public static TransferProgress IncrementProcessedEntries(int count = 1)
        {
            return new TransferProgress() { ProcessedEntriesIncrement = count };
        }
        public static TransferProgress UpdateCurrentProgress(short percent = 1)
        {
            return new TransferProgress() { CurrentProgressPercent = percent };
        }
    }

    public class NullTransferProgress : IProgress<TransferProgress>
    {
        public static NullTransferProgress Instance = new NullTransferProgress();
        public void Report(TransferProgress value)
        {
        }
    }

    public class ExplorerEvent
    {
        protected ExplorerEvent(object sender)
        {
            Sender = sender;
        }

        public object Sender { get; set; }
    }
    public class RootChangedEvent : ExplorerEvent
    {
        public static RootChangedEvent Created(object sender, params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(sender, ChangeType.Created, appliedRootDirectories);
        }

        public static RootChangedEvent Deleted(object sender, params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(sender, ChangeType.Deleted, appliedRootDirectories);
        }

        public static RootChangedEvent Changed(object sender, params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(sender, ChangeType.Changed, appliedRootDirectories);
        }

        public RootChangedEvent(object sender, ChangeType changeType, params IEntryModel[] appliedRootDirectories)
            : base(sender)
        {
            ChangeType = changeType;
            AppliedRootDirectories = appliedRootDirectories;
        }

        public ChangeType ChangeType { get; private set; }
        public IEntryModel[] AppliedRootDirectories { get; private set; }
    }

    public class EntryChangedEvent : ExplorerEvent
    {
        public EntryChangedEvent(object sender, ChangeType changeType, params string[] parseNames)
            : base(sender)
        {
            ChangeType = changeType;
            ParseNames = parseNames;
        }

        public EntryChangedEvent(object sender, Dictionary<string, string> renamedParseNames)
            : this(sender, ChangeType.Moved, renamedParseNames.Keys.ToArray())
        {
            _renamedParseNames = renamedParseNames ?? _renamedParseNames;
        }

        public EntryChangedEvent(object sender, string parseName, string orgParseName)
            : this(sender, new Dictionary<string, string>() { { parseName, orgParseName } })
        {
        }

        public string GetOrgParseName(string parseName)
        {
            return _renamedParseNames[parseName];
        }

        private Dictionary<string, string> _renamedParseNames = new Dictionary<string, string>();

        public ChangeType ChangeType { get; private set; }
        public string[] ParseNames { get; private set; }
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
