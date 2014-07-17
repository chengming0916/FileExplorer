using FileExplorer.Models;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileExplorer.Defines
{
    

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

    public class FileNameFilter
    {
        public string Description { get; set; }
        public string Filter { get; set; }

        public FileNameFilter(string descdription, string filter)
        {
            Description = descdription;
            Filter = filter;
        }
    }



    public class ProgressEventArgs : EventArgs
    {
        public string Message { get; private set; }

        /// <summary>
        /// Return the maximum of the progress, or -1 if not specified.
        /// </summary>
        public int TotalProgress { get; private set; }

        /// <summary>
        /// Return the current progress, or -1 if not specified.
        /// </summary>
        public int CurrentProgress { get; private set; }

        /// <summary>
        /// Returns the current processing file.
        /// </summary>
        public string File { get; private set; }

        public bool Cancel { get; set; }

        public ProgressEventArgs(int completed, int total, string file)
        {
            this.TotalProgress = total;
            this.CurrentProgress = completed;
            this.File = file;
            this.Message = this.ToString();
        }

        public override string ToString()
        {
            return String.Format("({0}/{1}) {2}", CurrentProgress, TotalProgress, File);
        }
    }

    public class TransferProgress
    {
        public ProgressType Type { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public Int32? TotalEntriesIncrement { get; set; }
        public Int32? ProcessedEntriesIncrement { get; set; }
        public short? CurrentProgressPercent { get; set; }
        public string Source { get; set; }
        public IPathHelper SourcePathHelper { get; set; }
        public string Destination { get; set; }
        public IPathHelper DestinationPathHelper { get; set; }

        public static TransferProgress SetAction(string action)
        {
            return new TransferProgress()
            {
                Action = action
            };
        }

        public static TransferProgress Error(Exception ex)
        {
            return new TransferProgress()
            {
                Type = ProgressType.Error,
                Message = ex.Message
            };
        }

        public static TransferProgress SetMessage(ProgressType type, string message)
        {
            return new TransferProgress()
            {
                Type = type,
                Message = message
            };
        }


        public static TransferProgress Completed()
        {
            return new TransferProgress()
            {
                Type = ProgressType.Completed
            };
        }

        public static TransferProgress From(string src, IPathHelper srcPathHelper, string dest, IPathHelper destPathHelper)
        {
            return new TransferProgress()
            {
                Type = ProgressType.Running,
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
            return new TransferProgress() { Type = ProgressType.Running, TotalEntriesIncrement = count };
        }
        public static TransferProgress IncrementProcessedEntries(int count = 1)
        {
            return new TransferProgress() { Type = ProgressType.Running, ProcessedEntriesIncrement = count };
        }
        public static TransferProgress UpdateCurrentProgress(short percent = 1)
        {
            return new TransferProgress() { Type = ProgressType.Running, CurrentProgressPercent = percent };
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
