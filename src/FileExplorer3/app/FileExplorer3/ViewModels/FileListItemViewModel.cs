﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileListItemViewModel : EntryViewModel, IFileListItemViewModel, ISupportDropHelper
    {
        #region FileListItemDropHelper

        #region MyRegion

        internal class FileListItemDropHelper : DropHelper<IEntryModel>
        {
            private static IEnumerable<IEntryModel> dataObjectFunc(IDataObject da,
                FileListItemViewModel flvm)
            {
                return flvm.EntryModel.Profile.GetEntryModels(da);
            }

            public FileListItemDropHelper(FileListItemViewModel flvm)
                : base(
                (ems, eff) => flvm.EntryModel.Profile.QueryDrop(ems, flvm.EntryModel, eff),
                da => dataObjectFunc(da, flvm),
                (ems, da, eff) => flvm.EntryModel.Profile.OnDropCompleted(ems, da, flvm.EntryModel, eff), em => EntryViewModel.FromEntryModel(em))
            {                 
            }
        }

        #endregion

        public FileListItemViewModel(IEntryModel model, IReportSelected<IEntryViewModel> reportSelected)
            : base(model)
        {
            _reportSelected = reportSelected;
            DropHelper = 
                model.Profile.QueryCanDrop(model) ? (ISupportDrop)new FileListItemDropHelper(this) : NoDropHelper.Instance;
            
        }

        #endregion

        #region Methods

        public override void NotifyOfPropertyChange(string propertyName = "")
        {
            base.NotifyOfPropertyChange(propertyName);

            switch (propertyName)
            {
                case "IsSelected" :
                    if (this.IsSelected)
                        _reportSelected.ReportChildSelected(this);
                    else _reportSelected.ReportChildUnSelected(this);
                    break;
            }
        }

        #endregion

        #region Data

        IReportSelected<IEntryViewModel> _reportSelected;

        #endregion

        #region Public Properties

        public ISupportDrop DropHelper { get; private set; }

        #endregion
    }
}
