using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Utils;
using FileExplorer.WPF.Utils;
using Microsoft.Live;
using FileExplorer.WPF.Models;

namespace FileExplorer.Models.SevenZipSharp
{
    public static class SzsFileStream
    {
        public static async Task<WebFileStream> OpenReadAsync(IEntryModel entryModel, CancellationToken ct)
        {
            var profile = entryModel.Profile as SzsProfile;
            ISzsItemModel entryItemModel = entryModel as ISzsItemModel;
            IEntryModel rootModel = entryItemModel.Root.ReferencedFile;

            byte[] bytes = new byte[] { };
            using (Stream stream = await (rootModel.Profile as IDiskProfile).DiskIO.OpenStreamAsync(rootModel, Defines.FileAccess.Read, ct))
            {
                MemoryStream ms = new MemoryStream();
                if (profile.Wrapper.ExtractOne(stream, entryItemModel.RelativePath, null, ms))
                    bytes = ms.ToByteArray();
            }

            return new WebFileStream(entryModel, bytes, null);
        }


        public static async Task<WebFileStream> OpenReadWriteAsync(IEntryModel entryModel, CancellationToken ct)
        {
            var contents = await WebUtils.DownloadToBytesAsync((entryModel as SkyDriveItemModel).SourceUrl, () => new HttpClient(), ct);
            return new WebFileStream(entryModel, contents, (m, s) =>
            {
                AsyncUtils.RunSync(() => updateSourceAsync(s));
            });
        }

        public static WebFileStream OpenWrite(IEntryModel entryModel)
        {
            return new WebFileStream(entryModel, null, (m, s) =>
            {
                AsyncUtils.RunSync(() => updateSourceAsync(s));
            });
        }


        private static async Task updateSourceAsync(WebFileStream stream)
        {
            //throw new Exception();

            var szsProfile = stream.Profile as SzsProfile;
            var szsModel = stream.EntryModel as ISzsItemModel;
            var rootReferencedFile = szsModel.Root.ReferencedFile;
            string type = szsModel.Root.GetExtension();

            using (Stream archiveStream = await (rootReferencedFile.Profile as IDiskProfile).DiskIO.OpenStreamAsync(rootReferencedFile,
                Defines.FileAccess.ReadWrite, CancellationToken.None))
            {
                await szsProfile.Wrapper.CompressOne(type, archiveStream, szsModel.RelativePath, stream);
            }
        }

    }

}
