using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Initialize a view model
    /// </summary>
    public interface IViewModelInitializer<VM>
    {
        Task InitalizeAsync(VM viewModel);
    }
}
