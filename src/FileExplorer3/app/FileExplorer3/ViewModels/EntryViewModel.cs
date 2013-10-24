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

namespace FileExplorer.ViewModels
{

    public class EntryViewModel : ViewAware, IEntryViewModel
    {
        #region Cosntructor

        private EntryViewModel()
        {
            Func<ImageSource> _getIcon = () => Profile.GetIconAsync(EntryModel, 32).Result;
            _icon = new Lazy<ImageSource>(_getIcon);
        }

        public static EntryViewModel FromEntryModel(IProfile profile, IEntryModel model)
        {
            return new EntryViewModel()
            {
                Profile = profile,
                EntryModel = model,
                IsEditable = model.IsRenamable
            };
        }
     
        #endregion

       

        #region Methods

        public override object GetView(object context = null)
        {
            return base.GetView(context);
        }

        #endregion

        #region Data

        bool _isSelected = false,_isEditing = false, _isEditable = false; 

        private Lazy<ImageSource> _icon;

        #endregion

        #region Public Properties

        public IProfile Profile { get; private set; }
        public bool IsEditing { get { return _isEditing; } set { _isEditing = value; NotifyOfPropertyChange(() => IsEditing); } }
        public bool IsEditable { get { return _isEditable; } 
            set { _isEditable = value; 
                NotifyOfPropertyChange(() => IsEditable);
                NotifyOfPropertyChange(() => EntryModel);
            } }

        public IEntryModel EntryModel { get; private set; }

        public Lazy<ImageSource> Icon { get { return _icon; } }

        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; NotifyOfPropertyChange(() => IsSelected); } }

        #endregion
    }
    
}
