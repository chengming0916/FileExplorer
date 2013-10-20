using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            yield return new ChangeViewMode("Default");
            yield return new LoadEntryList(parentEVm, filter);
            yield return new AppendEntryList(parentEVm, this);            
            yield return Loader.Show();
            
        }

        public override object GetView(object context = null)
        {
            return new Views.FileListView();            
        }
       
        #endregion

        #region Data

        private IObservableCollection<IEntryViewModel> _entryVms = new BindableCollection<IEntryViewModel>();
        private IObservableCollection<IEntryViewModel> _selectedVms = new BindableCollection<IEntryViewModel>();

        #endregion

        #region Public Properties

        public IObservableCollection<IEntryViewModel> Items { get { return _entryVms; } }
        public IObservableCollection<IEntryViewModel> SelectedEntries { get { return _selectedVms; } }
        
        #endregion

       
    }
}
