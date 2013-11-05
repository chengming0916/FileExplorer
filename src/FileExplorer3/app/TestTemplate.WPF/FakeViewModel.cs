using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTemplate.WPF
{
    public class FakeViewModel : INotifyPropertyChanged
    {

        private static void generate(FakeViewModel root, int level)
        {
            if (level > 0)
                for (int i = 1; i < 5; i++)
                {
                    var vm = new FakeViewModel()
                    {
                        Header = root.Header + i.ToString(),
                        Value = (root.Value + "\\Sub" + root.Header + i.ToString()).TrimStart('\\'),
                        Latency = root.Latency
                    };
                    generate(vm, level - 1);
                    root._subDirectories.Add(vm);
                }
        }

        public static FakeViewModel GenerateFakeViewModels()
        {
            var root = new FakeViewModel() { Latency = TimeSpan.FromSeconds(5) };
            generate(root, 5);
            return root;
        }

        public FakeViewModel(params FakeViewModel[] rootModels)
        {
            Header = "Root";
            Value = "";
            Latency = TimeSpan.FromSeconds(0);
            _subDirectories = new ObservableCollection<FakeViewModel>();
            foreach (var rm in rootModels)
                _subDirectories.Add(rm);
        }

        public FakeViewModel(string header, params string[] subHeaders)
        {
            Header = header;
            Value = header;
            SubDirectories = new ObservableCollection<FakeViewModel>();
            foreach (var sh in subHeaders)
                _subDirectories.Add(new FakeViewModel(sh) { Value = header + "\\" + sh });
        }

        private ObservableCollection<FakeViewModel> _subDirectories;
        public string Header { get; set; }
        public string Value { get; set; }
        public TimeSpan Latency { get; set; }
        public ObservableCollection<FakeViewModel> SubDirectories
        {
            get
            {
                Thread.Sleep(Latency);
                return _subDirectories;
            }
            set { _subDirectories = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
