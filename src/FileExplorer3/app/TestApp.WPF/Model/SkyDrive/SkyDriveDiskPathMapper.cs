using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;

namespace FileExplorer.Models
{
    public class SkyDriveDiskPathMapper : IDiskPathMapper
    {
        #region Constructor

        public SkyDriveDiskPathMapper(SkyDriveProfile profile, string tempPath = null)
        {
            _profile = profile;
            _tempPath = tempPath != null ? tempPath : Path.Combine(Path.GetTempPath(), "FileExplorer");
            Directory.CreateDirectory(_tempPath);
        }

        #endregion

        #region Methods

      
        /// <summary>
        /// Download
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateCacheAsync(IEntryModel model)
        {
            var skyModel = model as SkyDriveItemModel;
            var mapInfo = this[model];
            if (mapInfo != null)
            {
                if (model.IsDirectory) //Directory
                {
                    Directory.CreateDirectory(mapInfo.IOPath);
                    foreach (var subEm in await _profile.ListAsync(model))
                        await UpdateCacheAsync(subEm);
                }
                else //File
                {                    
                    byte[] bytes = await WebUtils.DownloadAsync(mapInfo.SourceUrl);
                    Directory.CreateDirectory(Path.GetDirectoryName(mapInfo.IOPath));
                    using (var s = File.OpenWrite(mapInfo.IOPath))
                        s.Write(bytes, 0, bytes.Length);
                    _isCachedDictionary[model.FullPath] = true;
                }
            }            
        }

        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task UpdateSourceAsync(IEntryModel model)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        private string _tempPath;
        private ConcurrentDictionary<string, bool> _isCachedDictionary = new ConcurrentDictionary<string, bool>();
        private SkyDriveProfile _profile;


        #endregion

        #region Public Properties

        public DiskMapInfo this[IEntryModel model]
        {
            get 
            {
                string path = Path.Combine(_tempPath, model.FullPath.Replace('/', '\\').TrimStart('\\'));
                bool isCached = _isCachedDictionary.ContainsKey(model.FullPath) && _isCachedDictionary[model.FullPath];
                string sourceUrl = (model as SkyDriveItemModel).SourceUrl;
                if (sourceUrl == null)
                    return null;
                return new DiskMapInfo(path, isCached) { SourceUrl = new Uri(sourceUrl) };
            }
        }


        #endregion


      
    }
}
