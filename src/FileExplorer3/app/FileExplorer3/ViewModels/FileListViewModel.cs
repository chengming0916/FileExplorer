using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    [Export(typeof(FileListViewModel))]
    [Export(typeof(IScreen))]
    public class FileListViewModel : Screen, IEntryListViewModel
    {
         #region Cosntructor

        public FileListViewModel()
        {
            //var ec = ConventionManager.AddElementConvention<ListView>(
            //   ListView.ItemsSourceProperty, "ItemsSource", "SourceUpdated");
            //ec.ApplyBinding = (vmType, path, property, element, convention) =>
            //{
            //    ConventionManager.SetBinding(vmType, path, property, element,
            //        ec, ItemsControl.ItemsSourceProperty);
            //    return true;
            //};
            
        }
        
        #endregion

        #region Methods

        public IEnumerable<IResult> Load()
        {
            IProfile profile = new FileSystemInfoProfile();
            var parentModel = profile.ParseAsync(@"c:\temp").Result;
            return Load(new FileSystemInfoProfile(), parentModel, null);
        }

        public IEnumerable<IResult> Load(IProfile profile, IEntryModel em, Func<IEntryModel, bool> filter = null)
        {
            
           
            
            var parentEVm = EntryViewModel.FromEntryModel(profile, em);
            yield return Loader.Show("Loading");
            yield return new ChangeView("Icon");
            //yield return new AddLabelColumn("Label", "EntryModel.Label", null);
            yield return new LoadEntryList(parentEVm, filter);
            yield return new AppendEntryList(parentEVm, this);
            SelectedEntries.Add(Items.First());
            yield return Loader.Show();
            
        }
   
       
        #endregion

        #region Data

        private IObservableCollection<IEntryViewModel> _entryVms = new BindableCollection<IEntryViewModel>();
        private IObservableCollection<IEntryViewModel> _selectedVms = new BindableCollection<IEntryViewModel>();
        private int _itemWidth = 60, _itemHeight = 60, _cacheCount = 5;        
        private Orientation _orientation = Orientation.Vertical;        

        #endregion

        #region Public Properties

        public Orientation Orientation { get { return _orientation; } set { _orientation = value; NotifyOfPropertyChange(() => Orientation); } }
        public int CacheCount { get { return _cacheCount; } set { _cacheCount = value; NotifyOfPropertyChange(() => CacheCount); } }
        public int ItemWidth { get { return _itemWidth; } set { _itemWidth = value; NotifyOfPropertyChange(() => ItemWidth); } }
        public int ItemHeight { get { return _itemHeight; } set { _itemHeight = value; NotifyOfPropertyChange(() => ItemHeight); } }

        public IObservableCollection<IEntryViewModel> Items { get { return _entryVms; } }
        public IObservableCollection<IEntryViewModel> SelectedEntries { get { return _selectedVms; } }
        
        #endregion

       
    }
}
