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
            SelectionHelper = new TreeSelectionHelper(findChildFunc);
            _subItems.Add(new TreeNodeViewModel("", this, null) { Header = "Root" });
        }

        async Task<TreeNodeSelectionHelper> findChildFunc(object value)
        {
            string path = value as string;
            foreach (var item in this.RootItems)
                if (path.StartsWith(item.Path, StringComparison.CurrentCultureIgnoreCase))
                    return item.SelectionHelper;
            return null;
        }


        private ObservableCollection<TreeNodeViewModel> _subItems = new ObservableCollection<TreeNodeViewModel>();
        public TreeSelectionHelper SelectionHelper { get; set; }
        public ObservableCollection<TreeNodeViewModel> RootItems { get { return _subItems; } }
        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        public static TreeNodeViewModel DummyNode = new TreeNodeViewModel();

        public override string ToString()
        {
            if (this.Equals(DummyNode))
                return "TreeNode - Dummy";
            else return "TreeNode - " + this.Path;
        }

        protected TreeNodeViewModel() //For DummyNode
        { }

        public TreeNodeViewModel(object value, TreeViewModel root, TreeNodeViewModel parentNode)
        {
            if (root == null || value == null)
                throw new ArgumentException();
            this.Path = value as string;
            _root = root;
            _parent = parentNode;
            
            SelectionHelper = new TreeNodeSelectionHelper(value, root.SelectionHelper, parentNode == null ? null :
                parentNode.SelectionHelper,
                findChildFunc, compareFunc);

            Subitems.Add(DummyNode);
        }

        #region Constructor

        #endregion

        #region Methods

        async Task LoadAsync()
        {
            if (this._subItems.Count() == 1 && this._subItems[0]._root == null) //NotLoaded
            {                
                Subitems.Clear();
                for (int i = 1; i < 6; i++)
                {                    
                    string path = (Path + "\\Sub" + i.ToString()).TrimStart('\\');
                    var vm = new TreeNodeViewModel(path, _root, this)
                    {
                        Path = path,
                        Header = "Sub" + i.ToString()
                    };
                    this.Subitems.Add(vm);
                }
            }
        }

        async Task<TreeNodeSelectionHelper> findChildFunc(object value)
        {
            await LoadAsync();
            string path = value as string;
            foreach (var item in this.Subitems)
                if (path.StartsWith(item.Path, StringComparison.CurrentCultureIgnoreCase))
                    return item.SelectionHelper;
            return null;
        }

        HierarchicalResult compareFunc(object first, object second)
        {
            string path1 = first as string;
            string path2 = second as string;
            if (path1.Equals(path2, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Current;
            if (path1.StartsWith(path2, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Parent;
            if (path2.StartsWith(path1, StringComparison.CurrentCultureIgnoreCase))
                return HierarchicalResult.Child;
            return HierarchicalResult.Unrelated;
        }

        #endregion

        #region Data

        private string _header = "NotLoaded";
        private string _path = "";
        private bool _isExpanded = false;

        private TreeViewModel _root = null;
        private TreeNodeViewModel _parent = null;
        private ObservableCollection<TreeNodeViewModel> _subItems = new ObservableCollection<TreeNodeViewModel>();

        #endregion

        #region Public Properties
        public TreeNodeSelectionHelper SelectionHelper { get; set; }

        public string Header { get { return _header; } set { _header = value; PropertyChanged(this, new PropertyChangedEventArgs("Header")); } }
        public string Path { get { return _path; } set { _path = value; PropertyChanged(this, new PropertyChangedEventArgs("Path")); } }
        public bool IsExpanded { get { return _isExpanded; } 
            set { if (value) LoadAsync().Wait(); _isExpanded = value; PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded")); } }

        public ObservableCollection<TreeNodeViewModel> Subitems { get { return _subItems; } }

        public event PropertyChangedEventHandler PropertyChanged = (o, e) => { };


        #endregion


    }
}
