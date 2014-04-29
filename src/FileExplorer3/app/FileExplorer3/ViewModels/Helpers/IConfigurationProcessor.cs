using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IConfigurationHelper
    {
        EntriesHelper<IConfigurationViewModel> Configurations { get; }
        Task LoadAsync(Stream stream);
        Task SaveAsync(Stream stream);
        IConfigurationViewModel Insert(IConfiguration configuration);
        void Remove(string name);
    }
}
