﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public class FileSystemInfoProfile : IProfile
    {
        #region Cosntructor

        #endregion

        #region Methods

        public IComparer<IEntryModel> GetComparer(string property)
        {
            return new ValueComparer<IEntryModel>(p => p.FullPath);
        }

        public Task<IEntryModel> ParseAsync(string path)
        {
            IEntryModel retVal = null;
            if (Directory.Exists(path))
                retVal = new FileSystemInfoModel(new DirectoryInfo(path));
            else
                if (File.Exists(path))
                    retVal = new FileSystemInfoModel(new FileInfo(path));
            return Task.FromResult<IEntryModel>(retVal);
        }

        public Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null)
        {
            List<IEntryModel> retVal = new List<IEntryModel>();
            if (entry.IsDirectory)
            {
                DirectoryInfo di = new DirectoryInfo(entry.FullPath);
                retVal.AddRange(from fsi in di.GetFileSystemInfos() select new FileSystemInfoModel(fsi));
            }
            return Task.FromResult<IEnumerable<IEntryModel>>(retVal);
        }
        
        public Task<ImageSource> GetIconAsync(IEntryModel entry, int size)
        {
            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(entry.FullPath))
            using (var bitmap = icon.ToBitmap())
                return Task.FromResult<ImageSource>(
                    Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap));
        }

        public IResult<IEnumerable<IEntryModel>> Transfer(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }

        public IResult<bool> Rename(IEntryModel source, string newName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion





        public Task<IEnumerable<IEntryModel>> TransferAsync(TransferMode mode, IEntryModel[] source, IEntryModel dest)
        {
            throw new NotImplementedException();
        }

        Task<bool> IProfile.Rename(IEntryModel source, string newName)
        {
            throw new NotImplementedException();
        }



      
    }
}
