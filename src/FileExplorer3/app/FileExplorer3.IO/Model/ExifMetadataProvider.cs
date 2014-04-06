using Cofe.Core.Utils;
using ExifLib;
using FileExplorer.Defines;
using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models
{

    public class ExifMetadataProvider : MetadataProviderBase
    {
        public static ExifTags[] RecognizedExifTags = new[]
            {
                ExifTags.ResolutionUnit, ExifTags.ExposureProgram, ExifTags.ISOSpeedRatings, ExifTags.Flash, 
                ExifTags.Orientation, ExifTags.PixelXDimension, ExifTags.PixelYDimension, 
                
                ExifTags.DateTime, ExifTags.DateTimeDigitized, ExifTags.DateTimeOriginal, 

                ExifTags.ApertureValue, 

                ExifTags.FNumber, ExifTags.FocalLength, ExifTags.XResolution, ExifTags.YResolution,

                ExifTags.ExposureTime
            };

        public override async Task<IEnumerable<IMetadata>> GetMetadataAsync(IEnumerable<IEntryModel> selectedModels, int modelCount, IEntryModel parentModel)
        {
            List<IMetadata> retList = new List<IMetadata>();

            Action<ExifReader, ExifTags> addExifVal = (reader, tag) =>
                {
                    object val = null;
                    switch (tag)
                    {
                        case ExifTags.FNumber:
                        case ExifTags.FocalLength:
                        case ExifTags.XResolution : 
                        case ExifTags.YResolution :
                            int[] rational;
                            if (reader.GetTagValue(tag, out rational))
                                val = rational[0];
                            break;
                        default:
                            reader.GetTagValue<object>(tag, out val);
                            break;
                    }

                    if (val != null)
                    {
                        DisplayType displayType = DisplayType.Auto;
                        switch (val.GetType().Name)
                        {
                            case "DateTime":
                                displayType = DisplayType.DateTime;
                                break;
                            case "Double" :
                            case "Float" :
                                val = Math.Round(Convert.ToDouble(val), 2).ToString();
                                displayType = DisplayType.Text;
                                break;
                            default :
                                displayType = DisplayType.Text;
                                val = val.ToString();
                                break;
                        }
                        retList.Add(new Metadata(displayType, MetadataStrings.strImage, tag.ToString(),
                                 val) { IsVisibleInStatusbar = false });
                    }
                };

            if (selectedModels.Count() == 1)
            {
                try
                {
                    var diskModel = selectedModels.First() as DiskEntryModelBase;
                    if (GetExifThumbnail.IsExifSupported(diskModel))
                    {
                        using (var stream = await diskModel.DiskProfile.DiskIO.OpenStreamAsync(diskModel, FileAccess.Read, CancellationToken.None))
                        using (ExifReader reader = new ExifReader(stream))
                        {
                            var thumbnailBytes = reader.GetJpegThumbnailBytes();
                            if (thumbnailBytes != null && thumbnailBytes.Length > 0)
                                retList.Add(new Metadata(DisplayType.Image, MetadataStrings.strImage, MetadataStrings.strThumbnail,
                                    ConverterUtils.ToBitmapImage(thumbnailBytes)) { IsVisibleInStatusbar = false });

                            foreach (var tag in RecognizedExifTags)
                                addExifVal(reader, tag);
                        }
                    }
                }
                catch { }
            }



            return retList;
        }

    }
}
