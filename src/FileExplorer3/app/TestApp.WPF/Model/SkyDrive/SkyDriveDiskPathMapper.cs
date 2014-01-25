﻿using System;
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
            _tempPath = tempPath != null ? tempPath : Path.Combine(Path.GetTempPath(), "FileExplorer");
            Directory.CreateDirectory(_tempPath);
        }

        #endregion

        #region Methods


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