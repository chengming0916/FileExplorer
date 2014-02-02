using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileExplorer.Utils;

namespace FileExplorer.Models
{

    public class GetDefaultIcon : IEntryModelIconExtractor
    {
        private static ImageSource FileIcon { get; set; }
        private static ImageSource FolderIcon { get; set; }
        public static GetDefaultIcon Instance = new GetDefaultIcon();

        static GetDefaultIcon()
        {
            BitmapImage fileIcon = new BitmapImage();
            fileIcon.BeginInit();
            fileIcon.UriSource = new Uri("pack://application:,,,/FileExplorer3;component/Themes/Resources/file.ico");
            fileIcon.EndInit();
            FileIcon = fileIcon;
            BitmapImage folderIcon = new BitmapImage();
            folderIcon.BeginInit();
            folderIcon.UriSource = new Uri("pack://application:,,,/FileExplorer3;component/Themes/Resources/folder.ico");
            folderIcon.EndInit();
            FolderIcon = folderIcon;
        }

        public Task<ImageSource> GetIconForModelAsync(IEntryModel model, CancellationToken ct)
        {
            if (model.IsDirectory)
                return Task<ImageSource>.FromResult(FolderIcon);
            else return Task<ImageSource>.FromResult(FileIcon);
        }
    }

    public class GetUriIcon : IEntryModelIconExtractor
    {
        private Func<IEntryModel, Uri> _uriFunc;
        private System.Threading.CancellationToken CancellationToken;
        public GetUriIcon(Func<IEntryModel, Uri> uriFunc)
        {
            _uriFunc = uriFunc;
        }




        public async Task<ImageSource> GetIconForModelAsync(IEntryModel model, CancellationToken ct)
        {
            var output = await WebUtils.DownloadAsync(_uriFunc(model), ct);            

            BitmapImage retIcon = new BitmapImage();
            if (!ct.IsCancellationRequested)
            {
                retIcon.BeginInit();
                retIcon.StreamSource = new MemoryStream(output);
                retIcon.EndInit();
                retIcon.Freeze();
            }
            return retIcon;
        }
    }
}
