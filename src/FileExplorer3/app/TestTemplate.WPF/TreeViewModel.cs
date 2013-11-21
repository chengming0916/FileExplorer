using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;

namespace TestTemplate.WPF
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        public TreeViewModel()
        {
            Entries = new TreeEntryHelper<TreeNodeViewModel>();
            SelectionHelper = new TreeSelectionHelper<TreeNodeViewModel, string>(Entries, compareFunc, (vm) => vm.SelectionHelper);

            Entries.SetEntries(new TreeNodeViewModel("", "Root", this, null));            

        }

        HierarchicalResult compareFunc(string path1, string path2)
        {
            if (path1.Equals(path2, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Current;
            if (path1.StartsWith(path2, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Parent;
            if (path2.StartsWith(path1, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Child;
            return HierarchicalResult.Unrelated;
        }    
        
        public TreeSelectionHelper<TreeNodeViewModel, string> SelectionHelper { get; set; }
        public TreeEntryHelper<TreeNodeViewModel> Entries { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        //public static TreeNodeViewModel DummyNode = new TreeNodeViewModel();

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Header))
                return "TreeNode - Dummy";
            else return "TreeNode - " + this.Path;
        }

        protected TreeNodeViewModel() //For DummyNode
        { }

        public TreeNodeViewModel(string value, string header, TreeViewModel root, TreeNodeViewModel parentNode)
        {
            if (root == null || value == null)
                throw new ArgumentException();
            this.Path = value as string;
            _root = root;
            _parent = parentNode;
            _header = header;


            Entries = new TreeEntryHelper<TreeNodeViewModel>(() => Task.Run(() =>
            {
                return (IEnumerable<TreeNodeViewModel>)new List<TreeNodeViewModel>(
                    from i in Enumerable.Range(1, 9)
                    select new TreeNodeViewModel(
                        (Path + "\\Sub" + i.ToString()).TrimStart('\\'),
                        "Sub" + i.ToString(),
                        _root, this)
                    );
            }));

            SelectionHelper = new TreeNodeSelectionHelper<TreeNodeViewModel, string>(
                value,  root.SelectionHelper, parentNode == null ? null : parentNode.SelectionHelper, Entries);           
        }

        #region Constructor

        #endregion

        #region Methods

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

   

        #endregion

        #region Data

        private string _header = "NotLoaded";
        private string _path = "";

        private TreeViewModel _root = null;
        private TreeNodeViewModel _parent = null;
        private TreeNodeSelectionHelper<TreeNodeViewModel, string> _selectionHelper;
        private TreeEntryHelper<TreeNodeViewModel> _entryHelper;

        #endregion

        #region Public Properties
        public TreeNodeSelectionHelper<TreeNodeViewModel, string> SelectionHelper
        {
            get { return _selectionHelper; }
            set { _selectionHelper = value; NotifyPropertyChanged("SelectionHelper"); }
        }
        public TreeEntryHelper<TreeNodeViewModel> Entries
        {
            get { return _entryHelper; }
            set { _entryHelper = value; NotifyPropertyChanged("Entries"); }
        }

        public string Header { get { return _header; } set { _header = value; NotifyPropertyChanged("Header"); } }
        public string Path { get { return _path; } set { _path = value; NotifyPropertyChanged("Path"); } }

        public event PropertyChangedEventHandler PropertyChanged = (o, e) => { };


        #endregion


    }
}
