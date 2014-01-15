using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class DiskMapInfo
    {
        public bool IsCached { get; private set; }
        public string IOPath { get; private set; }
        public Uri SourceUrl { get; set; }

        public DiskMapInfo(string ioPath, bool isCached)
        {
            IOPath = ioPath;
            IsCached = isCached;
        }
    }

    /// <summary>
    /// Map EntryModel to Local disk path
    /// </summary>
    public interface IDiskPathMapper
    {
        DiskMapInfo this[IEntryModel model] { get; }
        
        /// <summary>
        /// Download the entry from source.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdateCacheAsync(IEntryModel model);


        /// <summary>
        /// Upload the entry back to source.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdateSourceAsync(IEntryModel model);
    }

   
}
