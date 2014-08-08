using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test_DynamicRelayCommandDictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RootViewModel _rvm;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _rvm = new RootViewModel();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rvm.Commands.AddRandom = ScriptCommands.AssignValueConverter(ValueConverterType.ExecuteMethod, "{Converter}", 
                                    ScriptCommands.Reassign("{RootVM}", "{Converter}", null, false, 
                                    ScriptCommands.PrintDebug("Add")), "AddRandomNumber");

            _rvm.Commands.Add = ScriptCommands.AssignValueConverter(ValueConverterType.ExecuteMethod, "{Converter}",
                                    ScriptCommands.Reassign("{RootVM}", "{Converter}", null, false,
                                    ScriptCommands.PrintDebug("Add")), "AddNumber", "{Parameter}");


        }
    }
}
