using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileExplorer.Models
{
    public class DropBoxModelThumbnailExtractor : IEntryModelIconExtractor
    {
        public static DropBoxModelThumbnailExtractor Instance = new DropBoxModelThumbnailExtractor();

        public async Task<ImageSource>
            GetIconForModelAsync(IEntryModel model, System.Threading.CancellationToken ct)
        {
            var dboxModel = model as DropBoxItemModel;
            if (dboxModel != null && dboxModel.Metadata.Thumb_Exists)
            {
                byte[] bytes = (await (model.Profile as DropBoxProfile).Client.GetThumbnailTask(dboxModel.Metadata, 
                    DropNet.Models.ThumbnailSize.Large)).RawBytes;
                
                BitmapImage retIcon = new BitmapImage();

                retIcon.BeginInit();
                retIcon.StreamSource = new MemoryStream(bytes);

                retIcon.EndInit();
                retIcon.Freeze();
                return retIcon;
            }
            return null;
        }
    }
}
