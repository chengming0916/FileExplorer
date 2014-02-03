using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#else
//using Fesersoft.Hashing;
using System.Security.Cryptography;
#endif

namespace Cofe.Core.Utils
{
    /// <summary>
    /// Stream related Utils.
    /// </summary>
    public static class StreamUtils
    {
        //http://stackoverflow.com/questions/230128/best-way-to-copy-between-two-stream-instances-c
        /// <summary>
        /// Copy a stream to another stream.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="resetInputStream"></param>
        /// <param name="resetOutputStream"></param>
        /// <param name="closeOutputStream"></param>
        public static void CopyStream(Stream input, Stream output, bool resetInputStream = false, bool resetOutputStream = false, bool closeOutputStream = false)
        {
            if (resetInputStream)
                input.Seek(0, SeekOrigin.Begin);
            if (resetOutputStream)
                output.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }

            output.Flush();
#if !NETFX_CORE
            if (closeOutputStream)
                output.Close();
#endif
        }

        public static async Task CopyStreamAsync(Stream input, Stream output, bool resetInputStream = false, 
            bool resetOutputStream = false, bool closeOutputStream = false, Action<short> progress = null)
        {
            if (progress == null)
                progress = p => { };

            if (resetInputStream)
                input.Seek(0, SeekOrigin.Begin);
            if (resetOutputStream)
                output.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[32768];
            int read;
            while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                progress((short)Math.Truncate((read / input.Length * 100.0)));
                await output.WriteAsync(buffer, 0, read);
            }

            await output.FlushAsync();
#if !NETFX_CORE
            if (closeOutputStream)
                output.Close();
#endif
        }

        //http://stackoverflow.com/questions/11266141/c-sharp-convert-system-io-stream-to-byte
        public static byte[] ToByteArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length; )
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        ///// <summary>
        ///// Save a iconBitmap to JPEG stream.
        ///// </summary>
        ///// <param name="iconBitmap"></param>
        ///// <param name="stream"></param>
        //public static void SaveAsJPEGStream(this Bitmap bitmap, Stream stream)
        //{
        //    //http://stackoverflow.com/questions/41665/bmp-to-jpg-png-in-c
        //    var jpegCodecInfo = ImageCodecInfo.GetImageEncoders().First((e) => { return e.MimeType == "image/jpeg"; });
        //    EncoderParameters encoderParameters = new EncoderParameters(1);
        //    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
        //    lock (bitmap)
        //        bitmap.Save(stream, jpegCodecInfo, encoderParameters);
        //}

        ///// <summary>
        ///// Save a iconBitmap to PNG stream.
        ///// </summary>
        ///// <param name="iconBitmap"></param>
        ///// <param name="stream"></param>
        //public static void SaveAsPNGStream(this Bitmap bitmap, Stream stream)
        //{
        //    //http://stackoverflow.com/questions/1394297/generate-transparent-png-c
        //    lock (bitmap)
        //        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //    //var pngCodecInfo = ImageCodecInfo.GetImageEncoders().First((e,p) => { return e.MimeType == "image/png"; });
        //    //EncoderParameters encoderParameters = new EncoderParameters(1);
        //    //encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
        //    //iconBitmap.Save(stream, pngCodecInfo, encoderParameters);
        //}

        ///// <summary>
        ///// Read a stream and return it's CRC, does not reset or close the stream.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //public static string GetCRC(this Stream stream)
        //{
        //    Fesersoft.Hashing.crc32 crc32 = new Fesersoft.Hashing.crc32();
        //    uint crchash = (uint)crc32.CRC(stream);
        //    return (StringUtils.ConvertToHex(crchash));
        //}


        /// <summary>
        /// Read a stream and return it's MD5, does not reset or close the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetMD5(this Stream stream)
        {
#if NETFX_CORE
            var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");

            IBuffer buff;
            if (stream is MemoryStream)
            {
                buff = WindowsRuntimeBufferExtensions.GetWindowsRuntimeBuffer(stream as MemoryStream);
            }
            else //In case it returned a non-Memory stream.
            {
                MemoryStream ms = new MemoryStream();
                CopyStream(stream, ms);
                buff = WindowsRuntimeBufferExtensions.GetWindowsRuntimeBuffer(ms);
            }
            var hashed = alg.HashData(buff);
            return CryptographicBuffer.EncodeToHexString(hashed);
#else 
            MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
            byte[] hash = csp.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
#endif
        }

#if !NETFX_CORE
        public static Stream NewTempStream(out string fileName, string ext)
        {
            if (ext.StartsWith("."))
                ext = ext.TrimStart('.');
            do
            {
                fileName = PathFE.Combine(Path.GetTempPath(), StringUtils.RandomString(8) + "." + ext);
            }
            while (File.Exists(fileName));

            return new FileStream(fileName, FileMode.CreateNew);
        }
#endif
    }
}
