using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_IOCPanelTest
{
    public class ValueViewModel : NotifyPropertyChanged
    {
        public int Value { get; set; }
        public double Height { get; set; }

        public ValueViewModel(int value)
        {
            Value = value;
            Height = (new Random().NextDouble() * 15) + 19;
        }

    }
}
