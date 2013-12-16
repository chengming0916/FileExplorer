using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Tools;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FileExplorer.Models;
using QuickZip.Converters;
using QuickZip.UserControls.Logic.Tools.IconExtractor;

namespace FileExplorer.Models
{
    public class GetFromSystemImageList : IEntryModelIconExtractor
    {
        private IconExtractor _iconExtractor = new ExIconExtractor();
        public static GetFromSystemImageList Instance = new GetFromSystemImageList();
        public Task<ImageSource> GetIconForModel(IEntryModel model)
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
                                return Task.FromResult<ImageSource>(
                                    Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap));
                            else return Task.FromResult<ImageSource>(null);
                        });
                }

            return Task.FromResult<ImageSource>(null);
        }
    }

    public class GetImageFromImageExtractor : IEntryModelIconExtractor
    {
        public static GetImageFromImageExtractor Instance = new GetImageFromImageExtractor();

        public Task<ImageSource> GetIconForModel(IEntryModel model)
        {
            return Task<ImageSource>.Run(() =>
                {
                    if (model != null && !String.IsNullOrEmpty(model.FullPath))
                    {
                        var bitmap = ImageExtractor.ExtractImage(model.FullPath, new Size(120, 90), true);
                        if (bitmap != null)
                            return (ImageSource)Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap);
                    }
                    return null;
                });

        }
    }

    public class GetFromIconExtractIcon : IEntryModelIconExtractor
    {
        public static GetFromIconExtractIcon Instance = new GetFromIconExtractIcon();

        public Task<ImageSource> GetIconForModel(IEntryModel model)
        {
            if (model == null || model.IsDirectory)
                return Task.FromResult<ImageSource>(null);

            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(model.FullPath))
            using (var bitmap = icon.ToBitmap())
                return Task.FromResult<ImageSource>(
                    Cofe.Core.Utils.BitmapSourceUtils.CreateBitmapSourceFromBitmap(bitmap));
        }
    }
}
