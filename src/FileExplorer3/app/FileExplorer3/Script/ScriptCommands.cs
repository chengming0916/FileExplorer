using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.Script
{
    //public static class ScriptCommands
    //{
    //    //public static IScriptCommand WriteBytes(IEntryModel entryModel, byte[] bytes,
    //    //   Func<IEntryModel, IScriptCommand> nextCommandFunc)
    //    //{
    //    //    return OpenFileStream(entryModel, FileExplorer.Defines.FileAccess.Write,
    //    //        (em, s) => new SimpleScriptCommand("WriteBytes", pd =>
    //    //        {
    //    //            s.Write(bytes, 0, bytes.Length);
    //    //            return ResultCommand.NoError;
    //    //        }), nextCommandFunc);

    //    //}
    //}


    /// <summary>
    /// Parse path 
    /// 
    /// Variable in ParameterDic:
    /// Profiles :  IProfile[] or IProfile, which is used to parse entry.    
    public class ParsePath : ScriptCommandBase
    {        
        public string Path { get; set; }

        /// <summary>
        /// Where to store after parse completed, Destination 
        /// </summary>
        public string DestinationKey { get; set; }



    }

    /// <summary>
    /// Write Byte to stream
    /// </summary>
    public class WriteByteArrayToDiskFile : ScriptCommandBase
    {
        

        public string DestinationFile { get; set; }
        /// <summary>
        /// Byte array is read from  ParameterDic[thisProperty], default = "Stream"
        /// </summary>
        public string SourceKey { get; set; }

        public WriteByteArrayToDiskFile()
        {
            SourceKey = "Stream";
        }
    }


    /// <summary>
    /// Serializable, Download source Url to "destinationVariable" property,
    /// 
    /// Variable in ParameterDic:
    /// Progress :  IProgress[TransferProgress] (Optional)
    /// HttpClientFunc : Func[HttpClient] (Optional)    
    /// </summary>
    public class DownloadAsByteArray : ScriptCommandBase
    {        
        /// <summary>
        /// Web url to download, required.
        /// </summary>
        public string SourceUrl { get; set; }
        /// <summary>
        /// After downloaded, store in ParameterDic[thisProperty] as ByteArray, default = "Stream"
        /// </summary>
        public string DestinationKey { get; set; }

        /// <summary>
        /// Serializable, Download source Url to "destinationVariable" property,
        /// </summary>
        public DownloadAsByteArray(IScriptCommand nextCommand) :
            base("DownloadAsByteArray", nextCommand, "Progress", "HttpClientFunc")
        {
            DestinationKey = "Stream";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var pdv = pm.ContainsKey("Progress") && pm["Progress"] is IProgress<TransferProgress>
                ? pm["Progress"] as IProgress<TransferProgress> :
                NullTransferProgress.Instance;

            try
            {
                using (var httpClient =
                    pm.ContainsKey("HttpClientFunc") && pm["HttpClientFunc"] is Func<HttpClient> ? ((Func<HttpClient>)pm["HttpClientFunc"])() :
                    new HttpClient())
                {
                    var response = await httpClient.GetAsync(SourceUrl, HttpCompletionOption.ResponseHeadersRead, pm.CancellationToken);
                    if (!response.IsSuccessStatusCode)
                        throw new WebException(String.Format("{0} when downloading {1}", response.StatusCode, SourceUrl));

                    MemoryStream destStream = new MemoryStream();

                    using (Stream srcStream = await response.Content.ReadAsStreamAsync())
                    {
                        pdv.Report(TransferProgress.From(SourceUrl));
                        byte[] buffer = new byte[1024];
                        ulong totalBytesRead = 0;
                        ulong totalBytes = 0;
                        try { totalBytes = (ulong)srcStream.Length; }
                        catch (NotSupportedException) { }

                        int byteRead = await srcStream.ReadAsync(buffer, 0, buffer.Length, pm.CancellationToken);
                        while (byteRead > 0)
                        {
                            await destStream.WriteAsync(buffer, 0, byteRead, pm.CancellationToken);
                            totalBytesRead = totalBytesRead + (uint)byteRead;
                            short percentCompleted = (short)((float)totalBytesRead / (float)totalBytes * 100.0f);
                            pdv.Report(TransferProgress.UpdateCurrentProgress(percentCompleted));

                            byteRead = await srcStream.ReadAsync(buffer, 0, buffer.Length, pm.CancellationToken);

                        }
                        await destStream.FlushAsync();
                    }

                    pm[DestinationKey] = destStream.ToByteArray();
                    return NextCommand;
                }
            }
            catch (Exception ex)
            {
                return ResultCommand.Error(ex);
            }
        }

    }    
}
