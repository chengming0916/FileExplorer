using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.BaseControls;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class EntryViewModelHierarchyHelper : IHierarchyHelper
    {

        #region Constructor

        public EntryViewModelHierarchyHelper(
            IEntryModel[] rootModels,
            Func<IEntryModel, IEntryViewModel> viewModelFunc, 
            Func<IEntryModel, bool> filterFunc)
        {
            _rootModels = rootModels;
            _viewModelFunc = viewModelFunc ?? (m => EntryViewModel.FromEntryModel(m));
            _filterFunc = filterFunc;
        }

        #endregion

        #region Methods

        private IEntryViewModel castAsViewModel(object item)
        {
            if (item is IEntryViewModel)
                return item as IEntryViewModel;
            else 
                throw new ArgumentException();            
        }

        public IEnumerable<object> GetHierarchy(object item, bool includeCurrent)
        {
            IEntryViewModel itemVM = castAsViewModel(item);

            if (includeCurrent)
                yield return itemVM;

            var current = itemVM.EntryModel;
            while (current != null)
            {
                yield return _viewModelFunc(current);
                current = current.Parent;
            }
        }

        public string GetPath(object item)
        {
            IEntryViewModel itemVM = castAsViewModel(item);
            return itemVM.EntryModel.FullPath;
        }

        public object GetItem(object rootItem, string path)
        {
            foreach (var r in _rootModels)
            {
                var retVal = r.Profile.ParseAsync(path).Result;
                if (retVal != null)
                    return retVal;
            }
            return null;
        }

        public IEnumerable List(object item)
        {
            IEntryViewModel itemVM = castAsViewModel(item);
            var listResult = itemVM.EntryModel.Profile.ListAsync(itemVM.EntryModel, _filterFunc).Result;
            return listResult.Select(m => _viewModelFunc(m));
        }

        public string ExtractPath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            else return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }

        public string ExtractName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }
        
        #endregion

        #region Data

        Func<IEntryModel, IEntryViewModel> _viewModelFunc;
        private IDirectoryTreeViewModel _directoryTreeViewModel;
        private IEntryModel[] _rootModels;
        private Func<IEntryModel, bool> _filterFunc;
        
        #endregion

        #region Public Properties

        public char Separator { get { return '\\'; } }
        public StringComparison StringComparisonOption { get { return StringComparison.CurrentCultureIgnoreCase; } }
        
        #endregion


     

        

        

      
    }
}
