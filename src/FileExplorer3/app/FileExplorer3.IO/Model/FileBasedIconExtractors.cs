using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Tools;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using FileExplorer.WPF.Models;
using QuickZip.Converters;
using QuickZip.UserControls.Logic.Tools.IconExtractor;
using ExifLib;
using System.Windows.Media.Imaging;
using Cofe.Core.Utils;
using FileExplorer.WPF.Defines;
using FileExplorer.IO.Defines;
using FileExplorer.WPF.Utils;

namespace FileExplorer.Models
{
    public class GetFromSystemImageList : IEntryModelIconExtractor
    {
        private IconExtractor _iconExtractor = new ExIconExtractor();
        public static GetFromSystemImageList Instance = new GetFromSystemImageList();
        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            if (model != null && !String.IsNullOrEmpty(model.FullPath))
                using (FileSystemInfoEx fsi = FileSystemInfoEx.FromString(model.FullPath))
                {
                    Bitmap bitmap = null;
                    if (fsi != null)
                        return fsi.RequestPIDL(pidl =>
                        {
                            bitmap = _iconExtractor.GetBitmap(QuickZip.Converters.IconSize.extraLarge,
                                pidl.Ptr, model.IsDirectory, false);
                           
                            if (bitmap != null)
                                return Task.FromResult<byte[]>(
                                    bitmap.ToByteArray());
                            else return Task.FromResult<byte[]>(null);
                        });
                }

            return Task.FromResult<byte[]>(null);
        }
    }

    public class GetFromSystemImageListUsingExtension : IEntryModelIconExtractor
    {
        private IconExtractor _iconExtractor = new IconExtractor();
        private Func<IEntryModel, string> _fileNameFunc;
        public static GetFromSystemImageListUsingExtension Instance = new GetFromSystemImageListUsingExtension();

        public GetFromSystemImageListUsingExtension(Func<IEntryModel, string> fileNameFunc = null)
        {
            _fileNameFunc = fileNameFunc == null ? e => e.Label : fileNameFunc;
        }

        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            return Task<ImageSource>.Run(() =>
                {
                    if (model != null)
                    {
                        string fname = _fileNameFunc(model);
                        using (Bitmap bitmap =
                            _iconExtractor.GetBitmap(IconSize.large, fname, model.IsDirectory, false))
                            if (bitmap != null)
                                return bitmap.ToByteArray();
                        
                    }
                    return null;
                });


        }
    }

    public class GetImageFromImageExtractor : IEntryModelIconExtractor
    {
        public static GetImageFromImageExtractor Instance = new GetImageFromImageExtractor();

        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            return Task<byte[]>.Run(() =>
                {
                    if (model != null && !String.IsNullOrEmpty(model.FullPath))
                    {
                        using (Bitmap bitmap =
                            ImageExtractor.ExtractImage(model.FullPath, new Size(120, 90), true))
                        {
                            if (bitmap != null)
                                return bitmap.ToByteArray();
                        }
                    }
                    return null;
                });

        }
    }

    public class GetFromIconExtractIcon : IEntryModelIconExtractor
    {
        public static GetFromIconExtractIcon Instance = new GetFromIconExtractIcon();

        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            if (model == null || model.IsDirectory)
                return Task.FromResult<byte[]>(null);

            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(model.FullPath))
            using (var bitmap = icon.ToBitmap())
                return Task.FromResult<byte[]>(
                    bitmap.ToByteArray());
        }
    }


    public class GetExifThumbnail : IEntryModelIconExtractor
    {
        public async Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            var diskModel = model as DiskEntryModelBase;
            if (diskModel != null && diskModel.IsFileWithExtension(FileExtensions.ExifExtensions))
            {
                using (var stream = 
                    await diskModel.DiskProfile.DiskIO.OpenStreamAsync(diskModel, 
                        FileExplorer.Defines.FileAccess.Read, ct))
                {
                    using (ExifReader reader = new ExifReader(stream))
                    {
                        var thumbnailBytes = reader.GetJpegThumbnailBytes();
                        if (thumbnailBytes != null && thumbnailBytes.Length > 0)
                            return thumbnailBytes;
                    }
                }

            }

            return null;

        }
    }
}
