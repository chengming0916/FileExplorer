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
        private static ImageSource FileIcon { get; set; }
        private static ImageSource FolderIcon { get; set; }

        static EntryViewModel()
        {
            BitmapImage fileIcon = new BitmapImage();
            fileIcon.BeginInit();
            fileIcon.UriSource = new Uri("pack://application:,,,/FileExplorer3;component/Themes/Resources/file.ico");
            fileIcon.EndInit();
            FileIcon = fileIcon;
            BitmapImage folderIcon = new BitmapImage();
            folderIcon.BeginInit();
            folderIcon.UriSource = new Uri("pack://application:,,,/FileExplorer3;component/Themes/Resources/folder.ico");
            folderIcon.EndInit();
            FolderIcon = folderIcon;
        }

        private EntryViewModel()
        {
            Func<ImageSource> _getIcon = () => {
                var icon = EntryModel.Profile.GetIconAsync(EntryModel, 32).Result;
                if (icon != null)
                    icon.Freeze();
                return icon;

            };
            //if (Profile == null)
            //    _getIcon = () => null;
            _icon = new Lazy<ImageSource>(_getIcon);
        }

        public static EntryViewModel FromEntryModel(IEntryModel model)
        {
            return new EntryViewModel()
            {                
                EntryModel = model,
                IsEditable = model.IsRenamable
            };
        }

        public IEntryViewModel Clone()
        {
            return EntryViewModel.FromEntryModel(this.EntryModel);
        }

        #endregion



        #region Methods

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

        bool _isSelected = false, _isEditing = false, _isEditable = false;

        private Lazy<ImageSource> _icon;

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

        public Lazy<ImageSource> Icon { get { return _icon; } }

        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; NotifyOfPropertyChange(() => IsSelected); } }

        #endregion
    }

}
