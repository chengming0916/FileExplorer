using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTemplate.WPF
{
    public class FakeTreeViewModel : INotifyPropertyChanged
    {
        private string _header = "Header";
        private string _path = "";
        private bool _isExpanded = false;
        private bool _isSelected = false;
        private FakeTreeViewModel _parent = null;
        private ObservableCollection<FakeTreeViewModel> _subItems = new ObservableCollection<FakeTreeViewModel>();

        private static void generate(FakeTreeViewModel root, int level, string str = "")
        {
            if (level > 0)
                for (int i = 1; i < 5; i++)
                {
                    var vm = new FakeTreeViewModel()
                    {
                        Header = "Sub" + str + i.ToString(),
                        Path = (root.Path + "\\Sub" + str + i.ToString()).TrimStart('\\'),                        
                        _parent = root
                    };
                    generate(vm, level - 1, str + i.ToString());
                    root._subItems.Add(vm);
                }
        }
        public static FakeTreeViewModel GenerateFakeTreeViewModels()
        {
            var root = new FakeTreeViewModel() {  };
            generate(root, 5);
            return root;
        }


        public string Header { get { return _header; } set { _header = value; PropertyChanged(this, new PropertyChangedEventArgs("Header")); } }
        public string Path { get { return _path; } set { _path = value; PropertyChanged(this, new PropertyChangedEventArgs("Path")); } }
        public bool IsExpanded { get { return _isExpanded; } set { _isExpanded = value; PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded")); } }
        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; PropertyChanged(this, new PropertyChangedEventArgs("IsSelected")); } }
        public ObservableCollection<FakeTreeViewModel> Subitems { get { return _subItems; } }

        public event PropertyChangedEventHandler PropertyChanged = (o, e) => { };
    }
}
