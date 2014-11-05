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

        public DiskProfileBase(IEventAggregator events, params IConverterProfile[] converters)
            : base(events, converters)
        {
            MetadataProvider = new MetadataProviderBase(new BasicMetadataProvider(), new FileBasedMetadataProvider());
            DragDrop = new NullDragDropHandler();
        }

        public IDiskIOHelper DiskIO { get; protected set; }
        public IDragDropHandler DragDrop { get; protected set; }
    }
}
