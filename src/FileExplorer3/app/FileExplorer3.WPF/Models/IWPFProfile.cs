using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IWPFProfile : IProfile
    {
        IEventAggregator Events { get; }
    }

    public interface IDiskProfile : IProfile
    {
        IDiskIOHelper DiskIO { get; }
    }
}
