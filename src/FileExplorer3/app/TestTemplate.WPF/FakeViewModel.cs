using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTemplate.WPF
{
    public class FakeViewModel : INotifyPropertyChanged
    {
        public FakeViewModel(string header, params string[] subHeaders)
        {
            Header = header;
            SubDirectories = new ObservableCollection<FakeViewModel>();
            foreach (var sh in subHeaders)
                SubDirectories.Add(new FakeViewModel(sh));
        }

        public string Header { get; set; }
        public ObservableCollection<FakeViewModel> SubDirectories { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
