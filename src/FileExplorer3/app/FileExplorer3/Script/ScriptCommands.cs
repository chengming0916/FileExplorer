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
using System.ComponentModel;
using FileExplorer.IO;
using MetroLog;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {
        /// <summary>
        /// Serializable, Parse a path, requires Profile set to an IDiskProfile or IDiskProfile[].
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable if Bracketed.</param>
        /// <param name="destVariable"></param>
        /// <param name="foundCommand"></param>
        /// <param name="notFoundCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ParsePath(string pathVariable = "{Path}", string destVariable = "Entry",
            IScriptCommand foundCommand = null, IScriptCommand notFoundCommand = null)
        {
            return new ParsePath()
            {
                PathKey = pathVariable,
                DestinationKey = destVariable,
                NextCommand = (ScriptCommandBase)foundCommand,
                NotFoundCommand = (ScriptCommandBase)notFoundCommand
            };
        }

        /// <summary>
        /// Serializable, Parse path to "Entry" parameter, using IProfile[] from ParameterDic[ProfileKey].
        /// </summary>
        /// <param name="entryVariable"></param>
        /// <param name="destVariable"></param>
        /// <param name="maskVariable">Specify as mask directly (e.g. *), or reference another variable (e.g. {masks})</param>
        /// <param name="options"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand List(string entryVariable = "Entry", string destVariable = "Destination", 
            string maskVariable = "*",
            ListOptions options = ListOptions.File | ListOptions.Folder, 
            IScriptCommand nextCommand = null)
        {
            return new List()
            {
                EntryKey = entryVariable, DestinationKey = destVariable, MaskKey = maskVariable,
                ListOptions = options, NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a folder or file, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFolder"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreatePath(string pathVariable = "Path", bool isFolder = false, string destVariable = "Entry",
            NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return new DiskCreatePath()
            {
                PathKey = pathVariable,
                IsFolder = false,
                DestinationKey = destVariable,
                NameGenerationMode = nameGenerationMode,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a file, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="pathVariable"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreateFile(string pathVariable = "Path", string destVariable = "Entry",
            NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return DiskCreatePath(pathVariable, false, destVariable, nameGenerationMode, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a folder, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="pathVariable"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreateFolder(string pathVariable = "Path", string destVariable = "Entry",
           NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return DiskCreatePath(pathVariable, true, destVariable, nameGenerationMode, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFolder"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreatePath(string pathVariable = "Path", bool isFolder = false, string destVariable = "Entry",
            IScriptCommand nextCommand = null)
        {
            return ParsePath(pathVariable, destVariable, nextCommand, DiskCreatePath(pathVariable, false, destVariable,
                NameGenerationMode.NoRename, nextCommand));
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a file path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="pathVariable"></param>
        /// <param name="destVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreateFile(string pathVariable = "Path", string destVariable = "Entry",
           IScriptCommand nextCommand = null)
        {
            return DiskParseOrCreatePath(pathVariable, false, destVariable, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a folder path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="pathVariable"></param>
        /// <param name="destVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreateFolder(string pathVariable = "Path", string destVariable = "Entry",
           IScriptCommand nextCommand = null)
        {
            return DiskParseOrCreatePath(pathVariable, true, destVariable, nextCommand);
        }

        /// <summary>
        /// Serializable, Take an Entry (from entryVariable) and open it's stream (to streamVariable).
        /// Then close the stream after NextCommand is executed, and return ThenCommand.
        /// </summary>
        /// <param name="entryVariable"></param>
        /// <param name="streamVariable"></param>
        /// <param name="access"></param>
        /// <param name="streamCommand"></param>
        /// <param name="thenCommand"></param>
        /// <returns></returns>
        public static IScriptCommand OpenStream(string entryVariable = "Entry", string streamVariable = "Stream",
            FileAccess access = FileAccess.Read, IScriptCommand streamCommand = null, IScriptCommand thenCommand = null)
        {
            return new OpenStream()
            {
                EntryKey = entryVariable,
                StreamKey = streamVariable,
                Access = access,
                NextCommand = (ScriptCommandBase)streamCommand,
                ThenCommand = (ScriptCommandBase)thenCommand
            };
        }

        /// <summary>
        /// Serializable, Copy contents from ParameterDic[sourceVariable] (Stream or Byte[]) to ParameterDic[destinationVariable] (Stream).
        /// </summary>
        /// <param name="sourceVariable"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand CopyStream(string sourceVariable = "Source", string destinationVariable = "Destination",
            IScriptCommand nextCommand = null)
        {
            return new CopyStream()
            {
                SourceKey = sourceVariable,
                DestinationKey = destinationVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Copy contents from file1 to file2
        /// </summary>
        /// <param name="sourceFileVariable">Filepath of source</param>
        /// <param name="destinationFileVariable">Filepath of destination</param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand CopyFile(string sourceFileVariable = "SourceFile", string destinationFileVariable = "DestinationFile",
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.ParsePath(sourceFileVariable, "Source",
              ScriptCommands.DiskParseOrCreateFile(destinationFileVariable, "Destination",
              ScriptCommands.OpenStream("Source", "SourceStream", FileExplorer.Defines.FileAccess.Read,
              ScriptCommands.OpenStream("Destination", "DestinationStream", FileExplorer.Defines.FileAccess.Write,
              ScriptCommands.CopyStream("SourceStream", "DestinationStream", nextCommand))))
              , ResultCommand.Error(new FileNotFoundException(sourceFileVariable)));
        }


        /// <summary>
        /// Serializable, Download source Url (urlVariable) to "destinationVariable" property,
        /// 
        /// Variable in ParameterDic:
        /// Progress :  IProgress[TransferProgress] (Optional)
        /// HttpClientFunc : Func[HttpClient] (Optional)    
        /// </summary>
        /// <param name="urlVariable"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand Download(string urlVariable = "Url", string destinationVariable = "Destination",
            IScriptCommand nextCommand = null)
        {
            return new Download()
            {
                UrlKey = urlVariable,
                DestinationKey = destinationVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Download a web stream to a file.
        /// </summary>
        /// <param name="urlVariable">Url to access</param>
        /// <param name="destinationFileVariable">Destination file name.</param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DownloadFile(string urlVariable = "Url", string destinationFileVariable = "DestinationFile",
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Download(urlVariable, "DownloadStream",
               ScriptCommands.DiskParseOrCreateFile(destinationFileVariable, "Destination",
               ScriptCommands.OpenStream("Destination", "DestinationStream", FileExplorer.Defines.FileAccess.Write,
               ScriptCommands.CopyStream("DownloadStream", "DestinationStream", nextCommand))));
        }
    }


    /// <summary>
    /// Serializable, Parse path to "Entry" parameter, using IProfile[] from ParameterDic[ProfileKey].
    /// </summary>
    public class ParsePath : ScriptCommandBase
    {
        /// <summary>
        /// Profile to parse path (IProfile[] or IProfile), default = Profile
        /// </summary>
        public string ProfileKey { get; set; }

        /// <summary>
        /// Required, path to parse, default = {Path}
        /// </summary>
        public string PathKey { get; set; }

        /// <summary>
        /// Where to store after parse completed, default = Entry
        /// </summary>
        public string DestinationKey { get; set; }

        /// <summary>
        /// If the path is not found, command to execute.  Optional, return ResultCommand.Error if not specified.
        /// </summary>
        public ScriptCommandBase NotFoundCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ParsePath>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ParsePath() : base("ParsePath") { ProfileKey = "Profile"; PathKey = "{Path}"; DestinationKey = "Entry"; }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string path = pm.GetVariable<string>(PathKey, null);
            if (String.IsNullOrEmpty(path))
                return ResultCommand.Error(new ArgumentException("Path not specified."));

            if (!(pm.ContainsKey(ProfileKey)))
                return ResultCommand.Error(new ArgumentException("Profiles not specified."));
            IProfile[] profiles =
                pm[ProfileKey] is IProfile[] ? (IProfile[])pm[ProfileKey] :
                pm[ProfileKey] is IProfile ? new IProfile[] { (IProfile)pm[ProfileKey] } : null;

            if (profiles == null)
                return ResultCommand.Error(new ArgumentException("Profiles is not IProfile or IProfile[]."));

            pm[DestinationKey] = null;
            foreach (var p in profiles)
            {
                var retVal = await p.ParseAsync(path);
                if (retVal != null)
                {
                    logger.Info(String.Format("{0} = {1}", DestinationKey, retVal));
                    pm[DestinationKey] = retVal;
                    break;
                }
            }

            if (pm[DestinationKey] == null)
            {
                logger.Warn(String.Format("{0} = null", path));
                return NotFoundCommand ?? ResultCommand.Error(new FileNotFoundException(path + " is not found."));
            }

            return NextCommand;
        }

    }

    /// <summary>
    /// Serializable, List a directory contents.
    /// </summary>
    public class List : ScriptCommandBase
    {
        /// <summary>
        /// Entry to list, default = "Entry"
        /// </summary>
        public string EntryKey { get; set; }

        /// <summary>
        /// Where to store list result (IEntryModel[]), default = "Destination"
        /// </summary>
        public string DestinationKey { get; set; }

        /// <summary>
        /// File based mask, support * and ?, comma separated, default = "*"
        /// </summary>
        public string MaskKey { get; set; }

        /// <summary>
        /// Whether return folder result, default = File | Folder
        /// </summary>
        public ListOptions ListOptions { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<List>();

        public List()
            : base("List")
        {
            EntryKey = "Entry";
            DestinationKey = "Destination";
            MaskKey = "*";
            ListOptions = Defines.ListOptions.File | Defines.ListOptions.Folder;
        }

        private static Func<IEntryModel, bool> createFilter(ListOptions listOptions, string mask)
        {
            if (!listOptions.HasFlag(ListOptions.File))
                return em => em.IsDirectory && StringUtils.MatchFileMasks(em.Name, mask);
            else if (!listOptions.HasFlag(ListOptions.Folder))
                return em => !em.IsDirectory && StringUtils.MatchFileMasks(em.Name, mask);
            else return em => StringUtils.MatchFileMasks(em.Name, mask);
        }
        

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEntryModel entryModel = pm.ContainsKey(EntryKey) ? pm[EntryKey] as IEntryModel : null;
            if (entryModel == null || !entryModel.IsDirectory)
                return ResultCommand.Error(new ArgumentException(EntryKey + " is not found or not a directory IEntryModel"));
            string masks = pm.ReplaceVariableInsideBracketed(MaskKey);            

            IEntryModel[] listItems = (await entryModel.Profile.ListAsync(entryModel, pm.CancellationToken, 
                createFilter(ListOptions, masks))).ToArray();

            logger.Info(String.Format("{0} = IEntryModel[{1}]", DestinationKey, listItems.Length));
            pm[DestinationKey] = listItems;;

            return NextCommand;
        }

    }

    public class DiskCreatePath : ScriptCommandBase
    {
        /// <summary>
        /// Profile to create path IProfile, default = Profile,
        /// only IDiskProfile will be used.
        /// </summary>
        public string ProfileKey { get; set; }

        /// <summary>
        /// Required, path to create, default = Path
        /// </summary>
        public string PathKey { get; set; }

        /// <summary>
        /// Whether to create folder or file.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Where to store after parse completed, default = Entry.
        /// </summary>
        public string DestinationKey { get; set; }

        /// <summary>
        /// If filename already exists, how to generate a file.
        /// </summary>
        public NameGenerationMode NameGenerationMode { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DiskCreatePath>();

        public DiskCreatePath()
            : base("DiskCreatePath")
        { ProfileKey = "Profile"; DestinationKey = "Entry"; PathKey = "Path"; }


        public override bool CanExecute(ParameterDic pm)
        {
            return pm.ContainsKey(PathKey) &&
                pm[PathKey] is string &&
                !(pm[PathKey] as string).StartsWith("::{"); //Cannot execute if GuidPath            
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string path = pm.ContainsKey(PathKey) ? pm[PathKey] as string : null;
            if (path == null)
                return ResultCommand.Error(new ArgumentException("Path not specified."));

            IDiskProfile profile = pm.ContainsKey(ProfileKey) ? pm[ProfileKey] as IDiskProfile : null;
            if (profile == null)
                return ResultCommand.Error(new ArgumentException(ProfileKey + " is not assigned or not IDiskProfile."));

            string parentPath = profile.Path.GetDirectoryName(path);
            IFileNameGenerator fNameGenerator = FileNameGenerator.FromNameGenerationMode(NameGenerationMode,
                profile.Path.GetFileName(path));

            string fileName = fNameGenerator.Generate();
            while (fileName != null &&
                await profile.ParseAsync(profile.Path.Combine(parentPath, fileName)) != null)
                fileName = fNameGenerator.Generate();

            if (fileName == null)
                return ResultCommand.Error(new ArgumentException("Already exists."));

            string newEntryPath = profile.Path.Combine(parentPath, fileName);
            var createddModel = await profile.DiskIO.CreateAsync(newEntryPath, IsFolder, pm.CancellationToken);
            logger.Info(String.Format("{0} = {1} ({2})", DestinationKey, createddModel.FullPath, IsFolder ? "Folder" : "File"));
            pm[DestinationKey] = createddModel;

            //await ScriptRunner.RunScriptAsync(new Notifych)

            return NextCommand;
            //return new NotifyChangedCommand(_profile, newEntryPath, FileExplorer.Defines.ChangeType.Created,
            //    _thenFunc(createddModel));
        }
    }

    /// <summary>
    /// Serializable, Take an Entry (from EntryKey) and open it's stream (to StreamKey).
    /// Then close the stream after NextCommand is executed, and return ThenCommand.
    /// </summary>
    public class OpenStream : ScriptCommandBase
    {
        /// <summary>
        /// Entry (IEntryModel) to be used to open stream, default = Entry
        /// </summary>
        public string EntryKey { get; set; }

        /// <summary>
        /// Key for the stream opened, default = Stream
        /// </summary>
        public string StreamKey { get; set; }

        /// <summary>
        /// Access mode, default = Read.
        /// </summary>
        public FileAccess Access { get; set; }

        public ScriptCommandBase ThenCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<OpenStream>();

        public OpenStream()
            : base("OpenStream")
        {
            EntryKey = "Entry";
            StreamKey = "Stream";
            Access = FileAccess.Read;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEntryModel entryModel = pm.ContainsKey(EntryKey) ? pm[EntryKey] as IEntryModel : null;
            if (entryModel == null)
                return ResultCommand.Error(new ArgumentException(EntryKey + " is not found or not IEntryModel"));

            IDiskProfile profile = entryModel.Profile as IDiskProfile;
            if (profile == null)
                return ResultCommand.Error(new NotSupportedException(EntryKey + "'s Profile is not IDiskProfile"));

            using (var stream = await profile.DiskIO.OpenStreamAsync(entryModel, Access, pm.CancellationToken))
            {
                ParameterDic pmClone = pm.Clone();
                pmClone[StreamKey] = stream;
                logger.Info(String.Format("{0} = Stream of {1}", StreamKey, EntryKey));
                await ScriptRunner.RunScriptAsync(pmClone, NextCommand);
            }

            return ThenCommand;
        }
    }



    /// <summary>
    /// Serializable, Copy contents from ParameterDic[SourceKey] (Stream or Byte[]) to ParameterDic[thisProperty] (Stream).
    /// </summary>
    public class CopyStream : ScriptCommandBase
    {
        /// <summary>
        /// Stream or ByteArray is read from  ParameterDic[thisProperty], default = "Source"
        /// </summary>
        public string SourceKey { get; set; }

        /// <summary>
        /// Stream to write, default = "Destination"
        /// </summary>
        public string DestinationKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<CopyStream>();

        public CopyStream()
        {
            SourceKey = "Source";
            DestinationKey = "Destination";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            if (!pm.ContainsKey(SourceKey) || !pm.ContainsKey(DestinationKey))
                return ResultCommand.Error(new ArgumentException(String.Format("{0} or {1} not found in ParameterDic", SourceKey, DestinationKey)));

            Stream srcStream = pm[SourceKey] is Stream ? (Stream)pm[SourceKey] :
                pm[SourceKey] is byte[] ? new MemoryStream((byte[])pm[SourceKey]) : null;
            Stream destStream = pm[DestinationKey] is Stream ? (Stream)pm[DestinationKey] : null;

            if (srcStream == null)
                return ResultCommand.Error(new ArgumentException(SourceKey + " not a stream."));
            if (destStream == null)
                return ResultCommand.Error(new ArgumentException(DestinationKey + " not a stream."));

            logger.Info(String.Format("{0} -> {1}", SourceKey, DestinationKey));
            await srcStream.CopyToAsync(destStream);

            return NextCommand;
        }
    }


    /// <summary>
    /// Serializable, Download source Url to "destinationVariable" property,
    /// 
    /// Variable in ParameterDic:
    /// Progress :  IProgress[TransferProgress] (Optional)
    /// HttpClientFunc : Func[HttpClient] (Optional)    
    /// </summary>
    public class Download : ScriptCommandBase
    {
        /// <summary>
        /// Func[HttpClient], which is used to download file, and dispose when completed, optional, default = "HttpClient"
        /// </summary>
        public string HttpClientKey { get; set; }
        /// <summary>
        /// Web url to download, required, default = "Url"
        /// </summary>
        public string UrlKey { get; set; }
        /// <summary>
        /// After downloaded, store in ParameterDic[thisProperty] as ByteArray, default = "Stream"
        /// </summary>
        public string DestinationKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Download>();

        /// <summary>
        /// Serializable, Download source Url to "destinationVariable" property,
        /// </summary>
        public Download() :
            base("Download", "Progress")
        {
            UrlKey = "Url";
            DestinationKey = "Stream";
            HttpClientKey = "HttpClient";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var pdv = pm.ContainsKey("Progress") && pm["Progress"] is IProgress<TransferProgress>
                ? pm["Progress"] as IProgress<TransferProgress> :
                NullTransferProgress.Instance;
            string url = pm.ContainsKey(UrlKey) ? pm[UrlKey] as string : null;
            if (url == null)
                return ResultCommand.Error(new ArgumentException("Unspecified Url."));

            try
            {
                using (var httpClient =
                    pm.ContainsKey(HttpClientKey) && pm[HttpClientKey] is Func<HttpClient> ? ((Func<HttpClient>)pm[HttpClientKey])() :
                    new HttpClient())
                {
                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, pm.CancellationToken);
                    if (!response.IsSuccessStatusCode)
                        throw new WebException(String.Format("{0} when downloading {1}", response.StatusCode, url));

                    MemoryStream destStream = new MemoryStream();
                    logger.Info(String.Format("{0} = Stream of {1}", DestinationKey, url));
                    using (Stream srcStream = await response.Content.ReadAsStreamAsync())
                    {
                        pdv.Report(TransferProgress.From(url));
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
