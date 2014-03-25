using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
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

    public class GetResourceIcon : IEntryModelIconExtractor
    {
        private ImageSource IconResource { get; set; }

        public GetResourceIcon(Uri uri)
        {
            BitmapImage iconResource = new BitmapImage();
            iconResource.BeginInit();
            iconResource.UriSource = uri;
            iconResource.EndInit();
            IconResource = iconResource;

        }

        public GetResourceIcon(string library, string path2Resource)
            : this(PathUtils.MakeResourcePath(library, path2Resource))
        {
    
        }

        public Task<ImageSource> GetIconForModelAsync(IEntryModel model, CancellationToken ct)
        {            
            return Task<ImageSource>.FromResult(IconResource);
        }
    }

    public class GetUriIcon : IEntryModelIconExtractor
    {
        private Func<IEntryModel, Uri> _uriFunc;
        private System.Threading.CancellationToken CancellationToken;
        private Func<HttpClient> _clientFunc;
        public GetUriIcon(Func<IEntryModel, Uri> uriFunc, Func<HttpClient> clientFunc = null)
        {
            _uriFunc = uriFunc;
            _clientFunc = clientFunc ?? (() => new HttpClient());
        }




        public async Task<ImageSource> GetIconForModelAsync(IEntryModel model, CancellationToken ct)
        {
            var response = await _clientFunc().GetAsync(_uriFunc(model).AbsoluteUri, ct);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var output = await response.Content.ReadAsByteArrayAsync();

                BitmapImage retIcon = new BitmapImage();

                retIcon.BeginInit();
                retIcon.StreamSource = new MemoryStream(output);

                retIcon.EndInit();
                retIcon.Freeze();
                return retIcon;
            }
            return await new GetDefaultIcon().GetIconForModelAsync(model, ct);
        }
    }
}
