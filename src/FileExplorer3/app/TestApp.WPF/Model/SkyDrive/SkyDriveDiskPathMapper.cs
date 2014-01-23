using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Utils;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveDiskPathMapper : IDiskPathMapper
    {
        #region Constructor        

        public SkyDriveDiskPathMapper(string tempPath = null)
        {
            //_profile = profile;
            _tempPath = tempPath != null ? tempPath : Path.Combine(Path.GetTempPath(), "FileExplorer");
            Directory.CreateDirectory(_tempPath);
        }

        #endregion

        #region Methods


        /// <summary>
        /// Does not work, use DiskIOHelper
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateCacheAsync(IEntryModel model)
        {
            await _profile.checkLoginAsync();

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
        public async Task UpdateSourceAsync(IEntryModel model)
        {
            await _profile.checkLoginAsync();

            CancellationTokenSource cts = new CancellationTokenSource();
            var skyModel = model as SkyDriveItemModel;
            var mapInfo = this[model];
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { });

            if (mapInfo != null)
            {
                if (model.IsDirectory)
                {
                    throw new NotImplementedException();
                }
                else
                {                    
                    LiveConnectClient liveClient = new LiveConnectClient(_profile.Session);
                    LiveOperationResult result;
                    using (var s = File.OpenRead(mapInfo.IOPath))                    
                    {
                        var uid = (skyModel.Parent as SkyDriveItemModel).UniqueId;
                        result = await liveClient.UploadAsync(uid,
                            skyModel.Name, s, OverwriteOption.Overwrite, cts.Token, progressHandler);
                    }

                    (model as SkyDriveItemModel).init(_profile, model.FullPath, result.Result);                    
                }
            }
            
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
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                bool isCached = _isCachedDictionary.ContainsKey(model.FullPath) && _isCachedDictionary[model.FullPath];
                string sourceUrl = (model as SkyDriveItemModel).SourceUrl;
                if (sourceUrl == null)
                {
                    if (model.IsDirectory)
                        return new DiskMapInfo(path, isCached, true);
                    else return new DiskMapInfo(path, isCached, true);
                }
                return new DiskMapInfo(path, isCached, true) { SourceUrl = new Uri(sourceUrl) };
            }
        }


        #endregion



    }
}
