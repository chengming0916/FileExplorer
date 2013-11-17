using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace TestTemplate.WPF
{

    //public interface IHierarchyComparer<T>
    //{
    //    HierarchicalResult CompareHierarchy(T a, T b);
    //}

    //public interface ITreeNodeviewModel
    //{
    //    bool IsExpanded { get; }
    //    bool IsSelected { get; }
    //    Task BroadcastAsync(ITreeNodeviewModel lookup, Func<ITreeNodeviewModel> foundFunc);
    //}

    //public interface ITreeViewModel
    //{
    //    IHierarchyComparer<ITreeNodeviewModel> comparer { get; }
    //    void NotifySelected(ITreeNodeviewModel node);
    //}

    //public class TreeViewModel : ITreeViewModel
    //{

    //    public IHierarchyComparer<ITreeNodeviewModel> comparer
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public void NotifySelected(ITreeNodeviewModel node)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        

        private static void generate(TreeNodeViewModel root, int level, string str = "")
        {
            if (level > 0)
                for (int i = 1; i < 6; i++)
                {
                    var vm = new TreeNodeViewModel()
                    {
                        Header = "Sub" + str + i.ToString(),
                        Path = (root.Path + "\\Sub" + str + i.ToString()).TrimStart('\\'),
                        _parent = root
                    };
                    generate(vm, level - 1, str + i.ToString());
                    root._subItems.Add(vm);
                }
        }
        public static TreeNodeViewModel GenerateFakeTreeViewModels()
        {
            var root = new TreeNodeViewModel() { };
            generate(root, 6);
            return root;
        }

        #region Constructor

        #endregion

        #region Methods

        #endregion

        #region Data

        private string _header = "Header";
        private string _path = "";
        private bool _isExpanded = false;
        private bool _isSelected = false;
        private TreeNodeViewModel _parent = null;
        private ObservableCollection<TreeNodeViewModel> _subItems = new ObservableCollection<TreeNodeViewModel>();

        #endregion

        #region Public Properties

        public string Header { get { return _header; } set { _header = value; PropertyChanged(this, new PropertyChangedEventArgs("Header")); } }
        public string Path { get { return _path; } set { _path = value; PropertyChanged(this, new PropertyChangedEventArgs("Path")); } }
        public bool IsExpanded { get { return _isExpanded; } set { _isExpanded = value; PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded")); } }
        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; PropertyChanged(this, new PropertyChangedEventArgs("IsSelected")); } }
        public ObservableCollection<TreeNodeViewModel> Subitems { get { return _subItems; } }

        public event PropertyChangedEventHandler PropertyChanged = (o, e) => { };


        #endregion


    }  
}
