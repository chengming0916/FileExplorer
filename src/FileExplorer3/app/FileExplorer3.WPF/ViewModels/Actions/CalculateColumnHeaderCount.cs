using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.WPF.Models;

namespace FileExplorer.WPF.ViewModels
{
    public class CalculateColumnHeaderCount : IResult
    {
        #region Cosntructor

        public CalculateColumnHeaderCount(ColumnFilter[] filters)
        {
            _filters = filters;
        }

        #endregion

        #region Methods

        public void Execute(ActionExecutionContext context)
        {
            foreach (var f in _filters)
                f.MatchedCount = 0;

            var entryModels = context["EntryList"] as IEnumerable<IEntryModel>;
            if (entryModels != null)
                foreach (var em in entryModels)
                    foreach (var f in _filters)
                        if (f.Matches(em))
                            f.MatchedCount++;
        }

        #endregion

        #region Data

        private ColumnFilter[] _filters;

        #endregion

        #region Public Properties

        public event EventHandler<ResultCompletionEventArgs> Completed;

        #endregion


    }
}
