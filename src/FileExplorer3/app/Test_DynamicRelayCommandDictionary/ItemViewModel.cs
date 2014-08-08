using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_DynamicRelayCommandDictionary
{
    public class ItemViewModel : NotifyPropertyChanged
    {

        private bool _isSelected;

        public ItemViewModel(int value)
        {
            Value = value;
            _isSelected = false;
        }

        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; NotifyOfPropertyChanged(() => IsSelected); } }
        public int Value { get; private set; }
        
    }
}
