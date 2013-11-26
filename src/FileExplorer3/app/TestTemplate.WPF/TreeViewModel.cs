﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;
using FileExplorer;

namespace TestTemplate.WPF
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        public TreeViewModel()
        {
            //Submodel is TreeNodeViewModel,
            Entries = new EntriesHelper<TreeNodeViewModel>();
            //Value is based on string
            Selection = new TreeRootSelector<TreeNodeViewModel, string>(Entries, compareFunc);

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

        public ITreeSelector<TreeNodeViewModel, string> Selection { get; set; }
        public IEntriesHelper<TreeNodeViewModel> Entries { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    
    }


    public class TreeNodeViewModel : INotifyPropertyChanged, ISupportTreeSelector<TreeNodeViewModel, string>
    {
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
            _path = value;
            _root = root;
            _parent = parentNode;
            _header = header;


            Entries = new EntriesHelper<TreeNodeViewModel>(() => Task.Run(() =>
            {
                return (IEnumerable<TreeNodeViewModel>)new List<TreeNodeViewModel>(
                    from i in Enumerable.Range(1, 9)
                    select new TreeNodeViewModel(
                        (Path + "\\Sub" + i.ToString()).TrimStart('\\'),
                        "Sub" + i.ToString(),
                        _root, this)
                    );
            }));

            Selection = new TreeSelector<TreeNodeViewModel, string>(value, this, 
                parentNode == null ? root.Selection : parentNode.Selection, Entries);
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
        private bool _isOverflowed = false;

        #endregion

        #region Public Properties

        public ITreeSelector<TreeNodeViewModel, string> Selection { get; set; }
        public IEntriesHelper<TreeNodeViewModel> Entries { get; set; }

        public bool IsOverflowedOrRoot { get { return _isOverflowed || _parent == null; } set { } }
        public bool IsOverflowed
        {
            get { return _isOverflowed; }
            set
            {
                _isOverflowed = value;
                NotifyPropertyChanged("IsOverflowed"); NotifyPropertyChanged("IsOverflowedOrRoot");
            }
        }
        public string Header { get { return _header; } set { _header = value; NotifyPropertyChanged("Header"); } }
        public string Path { get { return _path; } set { _path = value; NotifyPropertyChanged("Path"); } }

        public event PropertyChangedEventHandler PropertyChanged = (o, e) => { };


        #endregion


    }
}
