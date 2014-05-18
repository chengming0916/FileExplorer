using FileExplorer.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.UserControls;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FileExplorer.WPF.Defines
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

    public class CopyOfFilterChangedEventArgs : RoutedEventArgs
    {
        public CopyOfFilterChangedEventArgs(object source)
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

    public class CopyOfTransferProgress
    {
        public Int32? TotalEntriesIncrement { get; set; }
        public Int32? ProcessedEntriesIncrement { get; set; }
        public short? CurrentProgressPercent { get; set; }
        public string Source { get; set; }
        public IPathHelper SourcePathHelper { get; set; }
        public string Destination { get; set; }
        public IPathHelper DestinationPathHelper { get; set; }

        public static CopyOfTransferProgress From(string src, IPathHelper srcPathHelper, string dest, IPathHelper destPathHelper)
        {
            return new CopyOfTransferProgress()
            {
                Source = src,
                SourcePathHelper = srcPathHelper,
                Destination = dest,
                DestinationPathHelper = destPathHelper
            };
        }

        public static CopyOfTransferProgress From(string src, IPathHelper pathHelper = null)
        {
            return From(src, pathHelper, null, null);
        }

        public static CopyOfTransferProgress From(string src, string dest)
        {
            return From(src, PathHelper.Auto(src), dest, PathHelper.Auto(dest));
        }

        public static CopyOfTransferProgress To(string dest, IPathHelper pathHelper = null)
        {
            return From(null, null, dest, pathHelper);
        }

        public static CopyOfTransferProgress IncrementTotalEntries(int count = 1)
        {
            return new CopyOfTransferProgress() { TotalEntriesIncrement = count };
        }
        public static CopyOfTransferProgress IncrementProcessedEntries(int count = 1)
        {
            return new CopyOfTransferProgress() { ProcessedEntriesIncrement = count };
        }
        public static CopyOfTransferProgress UpdateCurrentProgress(short percent = 1)
        {
            return new CopyOfTransferProgress() { CurrentProgressPercent = percent };
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
        public object Sender { get; set; }
    }

    public class BroadcastEvent
    {
        public object EventToBroadcast { get; set; }

        public BroadcastEvent(object evnt)
        {
            EventToBroadcast = evnt;
        }
    }

    public class CopyOfBroadcastEvent
    {
        public object EventToBroadcast { get; set; }

        public CopyOfBroadcastEvent(object evnt)
        {
            EventToBroadcast = evnt;
        }
    }

    public class RootChangedEvent : ExplorerEvent
    {
        public static RootChangedEvent Created(params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(ChangeType.Created, appliedRootDirectories);
        }

        public static RootChangedEvent Deleted(params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(ChangeType.Deleted, appliedRootDirectories);
        }

        public static RootChangedEvent Changed(params IEntryModel[] appliedRootDirectories)
        {
            return new RootChangedEvent(ChangeType.Changed, appliedRootDirectories);
        }

        public RootChangedEvent(ChangeType changeType, params IEntryModel[] appliedRootDirectories)
            : base()
        {
            ChangeType = changeType;
            AppliedRootDirectories = appliedRootDirectories;
        }

        public ChangeType ChangeType { get; private set; }
        public IEntryModel[] AppliedRootDirectories { get; private set; }
    }

    public class EntryChangedEvent : ExplorerEvent
    {
        public EntryChangedEvent(ChangeType changeType, params string[] parseNames)
            : base()
        {
            ChangeType = changeType;
            ParseNames = parseNames;
        }

        public EntryChangedEvent(Dictionary<string, string> renamedParseNames)
            : this(ChangeType.Moved, renamedParseNames.Keys.ToArray())
        {
            _renamedParseNames = renamedParseNames ?? _renamedParseNames;
        }

        public EntryChangedEvent(string parseName, string orgParseName)
            : this(new Dictionary<string, string>() { { parseName, orgParseName } })
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

   

}
