﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickZip.UserControls.MVVM;
using QuickZip.UserControls.MVVM.ViewModel;
using QuickZip.UserControls.MVVM.Model;
using System.Windows.Media;
using System.Windows.Data;

namespace QuickZip.UserControls.MVVM
{
    /// <summary>
    /// Media View.
    /// </summary>
    /// <typeparam name="FI"></typeparam>
    /// <typeparam name="DI"></typeparam>
    /// <typeparam name="FSI"></typeparam>
    public class MediaViewerViewModel<FI, DI, FSI> : ViewerBaseViewModel<FI, DI, FSI>
        where FI : FSI
        where DI : FSI
    {
        #region Constructor
        public MediaViewerViewModel(Profile<FI, DI, FSI> profile,
           FileModel<FI, DI, FSI> embedFileModel)
            : base(profile, embedFileModel)
        {
            IsDirectoryTreeEnabled = true;           
            CurrentViewerMode = ViewerMode.Media;
            MediaFile = _profile.GetDiskPath(embedFileModel.EmbeddedFile);
            //SimpleStatusbarViewModel.Status = embedFileModel.Name;
        }

        #endregion

        #region Methods

        public override void Refresh()
        {
            //throw new NotImplementedException();
        }

        public override void Expand()
        {
            //throw new NotImplementedException();
        }

        public override void OnUnload()
        {

        }

        protected override string getLabel()
        {
            if (EmbeddedEntryViewModel != null)
                return EmbeddedEntryViewModel.EmbeddedModel.Label;
            else return "";
        }

        protected override string getToolTip()
        {
            if (EmbeddedEntryViewModel != null)
                return EmbeddedEntryViewModel.EmbeddedModel.ParseName;
            else return "";
        }

        protected override ImageSource getSmallIcon()
        {
            return EmbeddedEntryViewModel.SmallIcon.Item1.Value;
        }


        #endregion

        #region Data

        private CollectionViewSource _subMedias;

        #endregion

        #region Public Proeprties

        public CollectionViewSource SubMedias { get { return _subMedias; } }
        //public SimpleStatusbarViewModel SimpleStatusbarViewModel { get { return StatusbarViewModel as SimpleStatusbarViewModel; } }

        #endregion
    }
}
