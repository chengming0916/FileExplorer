using Caliburn.Micro;
using FileExplorer.IO;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{

    public abstract class DiskProfileBase : ProfileBase, IDiskProfile
    {

        public DiskProfileBase(IEventAggregator events)
            : base(events)
        {
            MetadataProvider = new MetadataProviderBase(new BasicMetadataProvider(), new FileBasedMetadataProvider());
        }

        public IDiskIOHelper DiskIO { get; protected set; }
    }
}
