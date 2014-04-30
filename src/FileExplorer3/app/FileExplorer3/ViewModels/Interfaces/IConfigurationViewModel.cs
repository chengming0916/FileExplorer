using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IConfigurationViewModel : INotifyPropertyChanged
    {
        IConfiguration Configuration { get; }
    }
}
