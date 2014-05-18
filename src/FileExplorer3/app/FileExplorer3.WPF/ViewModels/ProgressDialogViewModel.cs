using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core;
using FileExplorer.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Defines;

namespace FileExplorer.WPF.ViewModels
{
    public class NullProgresViewModel : IProgress<TransferProgress>
    {
        public static NullProgresViewModel Instance = new NullProgresViewModel();
        public void Report(TransferProgress value)
        {
        }
    }

    public class ProgressDialogViewModel : Screen, IProgress<TransferProgress>
    {
        #region Constructor

        public ProgressDialogViewModel(ParameterDic pd, CancellationTokenSource cts = null)
        {
            _pd = pd;
            if (_pd.ContainsKey("ProgressHeader"))
                Header = _pd["ProgressHeader"] as string;
            _cts = cts ?? new CancellationTokenSource();
            _timeRemain = new EstimateTimeRemainViewModel();
        }

        #endregion

        #region Methods

        public void Report(TransferProgress value)
        {
            if (value.Source != null) Source = value.Source;
            if (value.Destination != null) Destination = value.Destination;
            if (value.SourcePathHelper != null) SourcePathHelper = value.SourcePathHelper;
            if (value.DestinationPathHelper != null) DestinationPathHelper = value.DestinationPathHelper;

            if (value.TotalEntriesIncrement.HasValue)
            {
                TotalEntries += value.TotalEntriesIncrement.Value;
                TimeRemain.TotalItems = TotalEntries;
            }
            if (value.ProcessedEntriesIncrement.HasValue)
            {
                ProcessedEntries += value.ProcessedEntriesIncrement.Value;
                TimeRemain.ProcessedItems = ProcessedEntries;
            }
            if (value.CurrentProgressPercent.HasValue)
                CurrentEntryProgress = value.CurrentProgressPercent.Value;
        }

        private short getOverallProgress()
        {
            return (short)(Math.Truncate((_processedEntries * 100.0) / (_totalEntries * 100.0) * 100) + CurrentEntryProgress);
        }

        public string getMessage()
        {
            string src = Source == null ? null : SourcePathHelper == null ? Source : SourcePathHelper.GetFileName(Source);
            string dest = Destination == null ? null : DestinationPathHelper == null ? Destination : DestinationPathHelper.GetDirectoryName(Destination);
            return String.Format("From [b]{0}[/b] To [b]{1}[/b]", src, dest);
        }

        #endregion

        #region Data

        private Int32 _totalEntries = 0, _processedEntries = 0;
        private short _currentEntryProgress = 0;
        private CancellationTokenSource _cts;
        private ParameterDic _pd;
        private string _header;
        private string _source;
        private string _destination;
        private string _message;
        private IPathHelper _sourcePathHelper;
        private IPathHelper _destinationPathHelper;
        private IEstimateTimeRemainViewModel _timeRemain;

        #endregion

        #region Public Properties

        public IEstimateTimeRemainViewModel TimeRemain { get { return _timeRemain; } set { _timeRemain = value; NotifyOfPropertyChange(() => TimeRemain); } }
        public string Header { get { return _header; } set { _header = value; NotifyOfPropertyChange(() => Header); } }
        public string Message { get { return getMessage(); } }

        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                NotifyOfPropertyChange(() => Source);
                NotifyOfPropertyChange(() => Message);
            }
        }
        public IPathHelper SourcePathHelper
        {
            get { return _sourcePathHelper; }
            set
            {
                _sourcePathHelper = value; NotifyOfPropertyChange(() => SourcePathHelper);
                NotifyOfPropertyChange(() => Message);
            }
        }

        public string Destination
        {
            get { return _destination; }
            set
            {
                _destination = value;
                NotifyOfPropertyChange(() => Destination);
                NotifyOfPropertyChange(() => Message);
            }
        }

        public IPathHelper DestinationPathHelper
        {
            get { return _destinationPathHelper; }
            set
            {
                _destinationPathHelper = value; NotifyOfPropertyChange(() => DestinationPathHelper);
                NotifyOfPropertyChange(() => Message);
            }
        }

        public Int32 TotalEntries
        {
            get { return _totalEntries; }
            set
            {
                _totalEntries = value;
                NotifyOfPropertyChange(() => TotalEntries);
                NotifyOfPropertyChange(() => Progress);
                NotifyOfPropertyChange(() => UnprocessedEntries);
            }
        }
        public Int32 ProcessedEntries
        {
            get { return _processedEntries; }
            set
            {
                _processedEntries = value;
                NotifyOfPropertyChange(() => ProcessedEntries);
                NotifyOfPropertyChange(() => Progress);
                NotifyOfPropertyChange(() => UnprocessedEntries);
            }
        }
        public Int32 UnprocessedEntries { get { return Math.Abs(_totalEntries - _processedEntries); } }

        public short CurrentEntryProgress { get { return _currentEntryProgress; } set { _currentEntryProgress = value; NotifyOfPropertyChange(() => CurrentEntryProgress); NotifyOfPropertyChange(() => Progress); } }
        public short Progress { get { return getOverallProgress(); } }

        public CancellationToken CancellationToken { get { return _cts.Token; } }
        public CancellationTokenSource CancellationTokenSource { get { return _cts; } }


        #endregion

    }
}
