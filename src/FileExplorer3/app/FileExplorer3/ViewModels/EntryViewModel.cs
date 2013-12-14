using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINRT
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif
using Caliburn.Micro;
using FileExplorer.Models;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace FileExplorer.ViewModels
{

    public class EntryViewModel : ViewAware, IEntryViewModel
    {
        #region Cosntructor

        public static IEntryViewModel DummyNode = new EntryViewModel() { EntryModel = EntryModelBase.DummyModel };

        protected EntryViewModel()
        {

        }

        protected EntryViewModel(IEntryModel model)
        {
            EntryModel = model;
            _iconExtractSequences = model.Profile.GetIconExtractSequence(model);
            IsEditable = model.IsRenamable;
        }

        public static EntryViewModel FromEntryModel(IEntryModel model)
        {
            return new EntryViewModel(model);

        }

        public IEntryViewModel Clone()
        {
            return EntryViewModel.FromEntryModel(this.EntryModel);
        }

        #endregion



        #region Methods

        private async Task loadIcon()
        {
            Action<Task<ImageSource>> updateIcon = (tsk) =>
                {
                    if (tsk.IsCompleted && !tsk.IsFaulted && tsk.Result != null)
                        Icon = tsk.Result;
                };

            foreach (var ext in _iconExtractSequences)
                await ext.GetIconForModel(EntryModel).ContinueWith(updateIcon);
        }

        public override bool Equals(object obj)
        {
            return
                obj is EntryViewModel &&
                this.EntryModel.Profile.HierarchyComparer
                .CompareHierarchy(this.EntryModel, (obj as EntryViewModel).EntryModel)
                == Defines.HierarchicalResult.Current;
        }

        public override object GetView(object context = null)
        {
            return base.GetView(context);
        }

        public override string ToString()
        {
            return "evm-" + this.EntryModel.ToString();
        }

        #endregion

        #region Data

        bool _isSelected = false, _isEditing = false, _isEditable = false, _isIconLoaded = false;
        private ImageSource _icon = null;
        private IEnumerable<IEntryModelIconExtractor> _iconExtractSequences;

        #endregion

        #region Public Properties

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                _isEditing = value;
                NotifyOfPropertyChange(() => EntryModel);
                NotifyOfPropertyChange(() => IsEditing);
            }
        }
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                NotifyOfPropertyChange(() => IsEditable);

            }
        }

        public IEntryModel EntryModel { get; private set; }

        public ImageSource Icon
        {
            get
            {
                if (!_isIconLoaded)
                {
                    _isIconLoaded = true;
                    loadIcon();
                }
                return _icon;
            }
            set { _icon = value; NotifyOfPropertyChange(() => Icon); }
        }

        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; NotifyOfPropertyChange(() => IsSelected); } }

        #endregion
    }

}
