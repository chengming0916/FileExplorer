using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public Task<ImageSource> GetIconForModelAsync(IEntryModel model)
        {
            if (model.IsDirectory)
                return Task<ImageSource>.FromResult(FolderIcon);
            else return Task<ImageSource>.FromResult(FileIcon);
        }
    }

    //public class GetFromProfile : IEntryModelIconExtractor
    //{
    //    public static GetFromProfile Instance = new GetFromProfile();

    //    public async Task<ImageSource> GetIconForModel(IEntryModel model)
    //    {
    //        var icon = await model.Profile.GetIconAsync(model, 32);
    //        icon.Freeze();
    //        return icon;
    //    }
    //}
}
