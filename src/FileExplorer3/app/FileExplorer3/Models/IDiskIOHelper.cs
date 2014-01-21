using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IDiskIOHelper
    {
        IDiskPathMapper DiskPath { get; }        

        Task<Stream> OpenStreamAsync(IEntryModel entry, FileAccess access);
        Task DeleteAsync(IEntryModel entry);
        Task RenameAsync(IEntryModel entry, string newName);

    }

    public class DiskIOHelperBase : IDiskIOHelper
    {

        protected DiskIOHelperBase()
        {
            DiskPath = new NullDiskPatheMapper();
        }

        public IDiskPathMapper DiskPath { get; protected set; }        

        public virtual Task<Stream> OpenStreamAsync(IEntryModel entry, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public virtual Task DeleteAsync(IEntryModel entry)
        {
            throw new NotImplementedException();
        }

        public virtual Task RenameAsync(IEntryModel entry, string newName)
        {
            throw new NotImplementedException();
        }
    }
}
