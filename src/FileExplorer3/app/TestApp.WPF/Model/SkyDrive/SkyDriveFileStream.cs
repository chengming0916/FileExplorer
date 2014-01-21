using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Utils;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveFileStream : MemoryStream
    {
        #region Constructor

        private SkyDriveFileStream(IEntryModel entryModel)
        {            
            _profile = entryModel.Profile as SkyDriveProfile;
            if (_profile == null)
                throw new ArgumentException();
            _entryModel = entryModel;
        }

        public static async Task<SkyDriveFileStream> OpenReadAsync(IEntryModel entryModel)
        {            
            string sourceUrl = (entryModel as SkyDriveItemModel).SourceUrl;
            byte[] bytes = await WebUtils.DownloadAsync(new Uri(sourceUrl));
            SkyDriveFileStream stream = new SkyDriveFileStream(entryModel);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public static async Task<SkyDriveFileStream> OpenReadWriteAsync(IEntryModel entryModel)
        {
            SkyDriveFileStream stream = await OpenReadAsync(entryModel);
            stream._udateSrcFunc = (s) => updateSourceAsync(s);
            return stream;
        }

        public static SkyDriveFileStream OpenWrite(IEntryModel entryModel)
        {
            SkyDriveFileStream stream = 
                new SkyDriveFileStream(entryModel) { _udateSrcFunc = (s) => updateSourceAsync(s) };

            return stream;
        }

        #endregion

        #region Methods

        private static async Task updateSourceAsync(SkyDriveFileStream stream)
        {
            await stream._profile.checkLoginAsync();

            CancellationTokenSource cts = new CancellationTokenSource();
            var skyModel = stream._entryModel as SkyDriveItemModel;            
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { });


            LiveConnectClient liveClient = new LiveConnectClient(stream._profile.Session);
            LiveOperationResult result;

            stream.Seek(0, SeekOrigin.Begin);
            var uid = (skyModel.Parent as SkyDriveItemModel).UniqueId;
            result = await liveClient.UploadAsync(uid,
                skyModel.Name, stream, OverwriteOption.Overwrite, cts.Token, progressHandler);

            skyModel.init(stream._profile, skyModel.FullPath, result.Result);
        }


        public override void Close()
        {
            if (!_closed)
            {
                _closed = true;
                if (_udateSrcFunc != null)
                    _udateSrcFunc(this);
            }
            //base.Close();
        }       

        #endregion

        #region Data

        private bool _closed = false;
        private SkyDriveProfile _profile;
        private IEntryModel _entryModel;
        private Func<SkyDriveFileStream, Task> _udateSrcFunc = null;

        #endregion

        #region Public Properties

        #endregion
    }
}
