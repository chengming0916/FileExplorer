using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Models;
using FileExplorer.UIEventHub;

namespace FileExplorer.WPF.ViewModels
{
    public class FileListItemViewModel : EntryViewModel, IFileListItemViewModel, ISupportDropHelper
    {
        #region FileListItemDropHelper

        #region MyRegion

        internal class FileListItemDropHelper : DropHelper<IEntryModel>
        {
            
            public FileListItemDropHelper(FileListItemViewModel flvm)
                : base( () => flvm.EntryModel.Label,
                (ems, eff) => flvm.EntryModel.Profile.DragDrop().QueryDrop(ems, flvm.EntryModel, eff),
                da => flvm.EntryModel.Profile.DragDrop().GetEntryModels(da),
                (ems, da, eff) => flvm.EntryModel.Profile.DragDrop().OnDropCompleted(ems, da, flvm.EntryModel, eff), em => EntryViewModel.FromEntryModel(em))
            {                 
            }
        }

        #endregion

        public FileListItemViewModel(IEntryModel model, IReportSelected<IEntryViewModel> reportSelected)
            : base(model)
        {
            _reportSelected = reportSelected;
            DropHelper =
                model.Profile.DragDrop().QueryCanDrop(model) ? (ISupportDrop)new FileListItemDropHelper(this) : NoDropHelper.Instance;
            
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
