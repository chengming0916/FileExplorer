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
        public double Width { get; set; }
        public int FontSize { get; set; }
        public static Random rand = new Random();

        public ValueViewModel(int value)
        {
            Value = value;
            Height = (rand.NextDouble() * 15) + 19;
            Width = (rand.NextDouble() * 100);
            FontSize = (rand.Next(30)) + 3;
        }

    }
}
