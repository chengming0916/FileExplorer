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
        /// Serializable, Assign a variable to ParameterDic when running.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value">If equals to null, remove the variable. (or use ScriptCommands.Reset)</param>
        /// <param name="skipIfExists"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand Assign(string variable = "{Variable}", object value = null,
            bool skipIfExists = false, IScriptCommand nextCommand = null)
        {
            return new Assign()
            {
                VariableKey = variable,
                Value = value,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, remove a variable from ParameterDic.
        /// </summary>
        /// <param name="nextCommand"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public static IScriptCommand Reset(IScriptCommand nextCommand = null, params string[] variables)
        {
            return ScriptCommands.RunCommands(Script.RunCommands.RunMode.Parallel, nextCommand,
                variables.Select(v => ScriptCommands.Assign(v, null)).ToArray());
        }

        /// <summary>
        /// Serializable, Parse a path, requires Profile set to an IDiskProfile or IDiskProfile[].
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="destVariable"></param>
        /// <param name="foundCommand"></param>
        /// <param name="notFoundCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ParsePath(string pathVariable = "{Path}", string destVariable = "{Entry}",
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
        public static IScriptCommand List(string entryVariable = "{Entry}", string destVariable = "{Destination}",
            string maskVariable = "*",
            ListOptions options = ListOptions.File | ListOptions.Folder,
            IScriptCommand nextCommand = null)
        {
            return new List()
            {
                EntryKey = entryVariable,
                DestinationKey = destVariable,
                MaskKey = maskVariable,
                ListOptions = options,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a folder or file, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="isFolder"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreatePath(string pathVariable = "{Path}", bool isFolder = false, string destVariable = "{Entry}",
            NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return new DiskCreatePath()
            {
                PathKey = pathVariable,
                IsFolder = isFolder,
                DestinationKey = destVariable,
                NameGenerationMode = nameGenerationMode,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a file, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreateFile(string pathVariable = "{Path}", string destVariable = "{Entry}",
            NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return DiskCreatePath(pathVariable, false, destVariable, nameGenerationMode, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, create a folder, requires Profile set to an IDiskProfile.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCreateFolder(string pathVariable = "{Path}", string destVariable = "{Entry}",
           NameGenerationMode nameGenerationMode = NameGenerationMode.Rename, IScriptCommand nextCommand = null)
        {
            return DiskCreatePath(pathVariable, true, destVariable, nameGenerationMode, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="isFolder"></param>
        /// <param name="destVariable"></param>
        /// <param name="nameGenerationMode"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreatePath(string pathVariable = "{Path}", bool isFolder = false, string destVariable = "{Entry}",
            IScriptCommand nextCommand = null)
        {
            return ParsePath(pathVariable, destVariable, nextCommand, DiskCreatePath(pathVariable, isFolder, destVariable,
                NameGenerationMode.NoRename, nextCommand));
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a file path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="destVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreateFile(string pathVariable = "{Path}", string destVariable = "{Entry}",
           IScriptCommand nextCommand = null)
        {
            return DiskParseOrCreatePath(pathVariable, false, destVariable, nextCommand);
        }

        /// <summary>
        /// Serializable, For DiskProfile only, parse a folder path, if not exists create it, store to destVariable (default Entry), then call next command.
        /// </summary>
        /// <param name="pathVariable">Actual path or reference variable (if Bracketed), e.g. C:\Temp or {Path}.</param>
        /// <param name="destVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskParseOrCreateFolder(string pathVariable = "{Path}", string destVariable = "{Entry}",
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
        public static IScriptCommand OpenStream(string entryVariable = "{Entry}", string streamVariable = "{Stream}",
            FileAccess access = FileAccess.Read, IScriptCommand streamCommand = null, IScriptCommand thenCommand = null)
        {
            return new DiskOpenStream()
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
        public static IScriptCommand CopyStream(string sourceVariable = "{Source}", string destinationVariable = "{Destination}",
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
        /// Serializable, Copy contents from file1 to file2
        /// </summary>
        /// <param name="sourceFileVariable">Filepath of source</param>
        /// <param name="destinationFileVariable">Filepath of destination</param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCopyFile(string sourceFileVariable = "{SourceFile}", string destinationFileVariable = "{DestinationFile}",
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.ParsePath(sourceFileVariable, "{DiskCopyFile.Source}",
              ScriptCommands.DiskParseOrCreateFile(destinationFileVariable, "{DiskCopyFile.Destination}",
                ScriptCommands.OpenStream("{DiskCopyFile.Source}", "{SourceStream}", FileExplorer.Defines.FileAccess.Read,
                    ScriptCommands.OpenStream("{DiskCopyFile.Destination}", "{DestinationStream}", FileExplorer.Defines.FileAccess.Write,
                        ScriptCommands.CopyStream("{SourceStream}", "{DestinationStream}",
                            ScriptCommands.Reset(nextCommand, "{DiskCopyFile.Source}", "{DiskCopyFile.Destination}"))))),
              ResultCommand.Error(new FileNotFoundException(sourceFileVariable)));
        }

        /// <summary>
        /// Not serializable, Copy contents from srcFile to destFile.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destFile"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskCopyFile(IEntryModel srcFile, IEntryModel destFile, IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("{DiskCopyFile-Source}", srcFile, false,
                   ScriptCommands.Assign("{DiskCopyFile-Dest}", destFile, false,
                    ScriptCommands.OpenStream("{DiskCopyFile.Source}", "{SourceStream}", FileExplorer.Defines.FileAccess.Read,
                   ScriptCommands.OpenStream("{DiskCopyFile.Destination}", "{DestinationStream}", FileExplorer.Defines.FileAccess.Write,
                       ScriptCommands.CopyStream("{CopyStream-Source}", "{CopyStream-Dest}",
                           ScriptCommands.Reset(nextCommand, "{DiskCopyFile-Source}", "{DiskCopyFile-Dest}"))))));
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
        public static IScriptCommand Download(string urlVariable = "{Url}", string destinationVariable = "{Destination}",
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
        public static IScriptCommand DownloadFile(string urlVariable = "{Url}", string destinationFileVariable = "{DestinationFile}",
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Download(urlVariable, "{DownloadStream}",
               ScriptCommands.DiskParseOrCreateFile(destinationFileVariable, "{Destination}",
               ScriptCommands.OpenStream("{Destination}", "{DestinationStream}", FileExplorer.Defines.FileAccess.Write,
               ScriptCommands.CopyStream("{DownloadStream}", "{DestinationStream}",
               ScriptCommands.Reset(nextCommand, "{DownloadStream}", "{Destination}")))));
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
        /// Required, path to parse, default = {Path} or C:\\Temp
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
        public ParsePath() : base("ParsePath") { ProfileKey = "{Profile}"; PathKey = "{Path}"; DestinationKey = "{Entry}"; }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string path = pm.ReplaceVariableInsideBracketed(PathKey);
            if (String.IsNullOrEmpty(path))
                return ResultCommand.Error(new ArgumentException("Path not specified."));

            IProfile profile = pm.GetValue<IProfile>(ProfileKey, null);
            IProfile[] profiles = profile != null ? new IProfile[] { profile } :
                pm.GetValue<IProfile[]>(ProfileKey, null);
            if (profiles == null)
                return ResultCommand.Error(new ArgumentException("Profiles not specified."));

            pm.SetValue<IEntryModel>(DestinationKey, null);
            foreach (var p in profiles)
            {
                var retVal = await p.ParseAsync(path);
                if (retVal != null)
                {
                    logger.Info(String.Format("{0} = {1}", DestinationKey, retVal));
                    pm.SetValue<IEntryModel>(DestinationKey, retVal);                    
                    break;
                }
            }

            if (pm.GetValue(DestinationKey) == null)
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
            EntryKey = "{Entry}";
            DestinationKey = "{Destination}";
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
            IEntryModel entryModel = pm.GetValue<IEntryModel>(EntryKey);
            if (entryModel == null || !entryModel.IsDirectory)
                return ResultCommand.Error(new ArgumentException(EntryKey + " is not found or not a directory IEntryModel"));
            string masks = pm.ReplaceVariableInsideBracketed(MaskKey);

            IEntryModel[] listItems = (await entryModel.Profile.ListAsync(entryModel, pm.CancellationToken,
                createFilter(ListOptions, masks))).ToArray();

            logger.Info(String.Format("{0} = IEntryModel[{1}]", DestinationKey, listItems.Length));
            pm.SetValue(DestinationKey, listItems);            

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
        /// Required, path to create, default = {Path} or C:\Temp
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
        { ProfileKey = "{Profile}"; DestinationKey = "{Entry}"; PathKey = "{Path}"; }


        public override bool CanExecute(ParameterDic pm)
        {
            string path = pm.ReplaceVariableInsideBracketed(PathKey);
            return !String.IsNullOrEmpty(path) && !path.StartsWith("::{"); //Cannot execute if GuidPath            
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string path = pm.ReplaceVariableInsideBracketed(PathKey);
            if (path == null)
                return ResultCommand.Error(new ArgumentException("Path not specified."));

            IDiskProfile profile = pm.GetValue<IDiskProfile>(ProfileKey);
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
            pm.SetValue(DestinationKey, createddModel);

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
    public class DiskOpenStream : ScriptCommandBase
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

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DiskOpenStream>();

        public DiskOpenStream()
            : base("DiskOpenStream")
        {
            EntryKey = "{Entry}";
            StreamKey = "{Stream}";
            Access = FileAccess.Read;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEntryModel entryModel = pm.GetValue<IEntryModel>(EntryKey); 
            if (entryModel == null)
                return ResultCommand.Error(new ArgumentException(EntryKey + " is not found or not IEntryModel"));

            IDiskProfile profile = entryModel.Profile as IDiskProfile;
            if (profile == null)
                return ResultCommand.Error(new NotSupportedException(EntryKey + "'s Profile is not IDiskProfile"));

            using (var stream = await profile.DiskIO.OpenStreamAsync(entryModel, Access, pm.CancellationToken))
            {
                ParameterDic pmClone = pm.Clone();
                pmClone.SetValue(StreamKey, stream);
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
            SourceKey = "{Source}";
            DestinationKey = "{Destination}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {            
            byte[] srcStreamAsByte = pm.GetValue<byte[]>(SourceKey);
            Stream srcStream = srcStreamAsByte != null ? new MemoryStream(srcStreamAsByte) :
                pm.GetValue<Stream>(SourceKey);
            Stream destStream = pm.GetValue<Stream>(DestinationKey);

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
            UrlKey = "{Url}";
            DestinationKey = "{Stream}";
            HttpClientKey = "{HttpClient}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var pdv = pm.GetValue<IProgress<TransferProgress>>("{Progress}", NullTransferProgress.Instance);
            string url = pm.GetValue<string>(UrlKey);
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

                    pm.SetValue(DestinationKey, destStream.ToByteArray());
                    return NextCommand;
                }
            }
            catch (Exception ex)
            {
                return ResultCommand.Error(ex);
            }
        }

    }


    /// <summary>
    /// Serializable, Assign a variable to ParameterDic when running.
    /// </summary>
    public class Assign : ScriptCommandBase
    {
        /// <summary>
        /// Variable name to set to, default = "Variable".
        /// </summary>
        public string VariableKey { get; set; }

        /// <summary>
        /// The actual value, default = null = remove.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Whether skip (or override) if key already in dictionary, default = true.
        /// </summary>
        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Assign>();

        public Assign()
        {
            VariableKey = "{Variable}";
            Value = null;
            SkipIfExists = true;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (pm.SetValue<Object>(VariableKey, Value, SkipIfExists))
                logger.Info(String.Format("{0} = {1}", VariableKey, Value));
            else logger.Warn(String.Format("Skipped {0}, already exists.", VariableKey));

            return NextCommand;
        }
    }

}
