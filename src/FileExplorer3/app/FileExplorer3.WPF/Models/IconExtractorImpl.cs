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
using FileExplorer.WPF.Utils;
using FileExplorer.Utils;

namespace FileExplorer.WPF.Models
{

    public class GetDefaultIcon : IEntryModelIconExtractor
    {
        private static byte[] FileIcon { get; set; }
        private static byte[] FolderIcon { get; set; }
        public static GetDefaultIcon Instance = new GetDefaultIcon();

        static GetDefaultIcon()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(GetDefaultIcon));
            string libraryName = assembly.GetName().Name;
            FileIcon = assembly.GetManifestResourceStream(
                PathUtils.MakeResourcePath(libraryName, "/Themes/Resources/file.ico")).ToByteArray();
            FolderIcon = assembly.GetManifestResourceStream(
                PathUtils.MakeResourcePath(libraryName, "/Themes/Resources/folder.ico")).ToByteArray();


        }

        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            if (model.IsDirectory)
                return Task<ImageSource>.FromResult(FolderIcon);
            else return Task<ImageSource>.FromResult(FileIcon);
        }
    }

    

    public class GetResourceIcon : IEntryModelIconExtractor
    {
        private byte[] IconResource { get; set; }

        public GetResourceIcon(object sender, string path2Resource)
        {
            var assembly = System.Reflection.Assembly.GetAssembly(sender.GetType());
            string libraryName = assembly.GetName().Name;
            string resourcePath = PathUtils.MakeResourcePath(libraryName, path2Resource);

            IconResource = assembly.GetManifestResourceStream(resourcePath).ToByteArray();
        }

        public Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
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


        public async Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct)
        {
            var response = await _clientFunc().GetAsync(_uriFunc(model).AbsoluteUri, ct);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var output = await response.Content.ReadAsByteArrayAsync();

                BitmapImage retIcon = new BitmapImage();
                return new MemoryStream(output).ToArray();
            }
            return await new GetDefaultIcon().GetIconBytesForModelAsync(model, ct);
        }
    }
}
