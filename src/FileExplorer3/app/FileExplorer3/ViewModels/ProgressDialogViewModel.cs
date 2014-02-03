using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public class ProgressDialogViewModel : Screen, IProgress<TransferProgress>
    {
        #region Constructor

        public ProgressDialogViewModel(ParameterDic pd, CancellationTokenSource cts = null)
        {
            _pd = pd;
            _cts = cts ?? new CancellationTokenSource();
        }

        #endregion

        #region Methods

        public void Report(TransferProgress value)
        {
            if (value.TotalEntriesIncrement.HasValue)
                TotalEntries += value.TotalEntriesIncrement.Value;
            if (value.ProcessedEntriesIncrement.HasValue)
                ProcessedEntries += value.ProcessedEntriesIncrement.Value;
            if (value.CurrentProgressPercent.HasValue)
                CurrentEntryProgress = value.CurrentProgressPercent.Value;         
        }

        private short getOverallProgress()
        {
            return (short)(Math.Truncate((_processedEntries * 100.0) / (_totalEntries * 100.0) * 100) + CurrentEntryProgress);
        }

        #endregion

        #region Data

        private Int32 _totalEntries = 0, _processedEntries = 0;
        private short _currentEntryProgress = 0;
        private CancellationTokenSource _cts;
        private ParameterDic _pd;

        #endregion

        #region Public Properties

        public Int32 TotalEntries { get { return _totalEntries; } set { _totalEntries = value; NotifyOfPropertyChange(() => TotalEntries); NotifyOfPropertyChange(() => OverallProgress); } }
        public Int32 ProcessedEntries { get { return _processedEntries; } set { _processedEntries = value; NotifyOfPropertyChange(() => ProcessedEntries); NotifyOfPropertyChange(() => OverallProgress); } }

        public short CurrentEntryProgress { get { return _currentEntryProgress; } set { _currentEntryProgress = value; NotifyOfPropertyChange(() => CurrentEntryProgress); NotifyOfPropertyChange(() => OverallProgress); } }
        public short OverallProgress { get { return getOverallProgress(); } }

        public CancellationToken CancellationToken { get { return _cts.Token; } }
        public CancellationTokenSource CancellationTokenSource { get { return _cts; } }
        
        
        #endregion
       
    }
}
