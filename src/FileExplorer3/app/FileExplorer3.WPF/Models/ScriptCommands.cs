using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Utils;
using FileExplorer.Models;
using FileExplorer.IO;
using System.Threading;

namespace FileExplorer.WPF.ViewModels
{
    public static partial class ScriptCommands
    {
        public static IScriptCommand ParsePath(IProfile[] profiles, string path, Func<IEntryModel, IScriptCommand> ifFoundFunc,
            IScriptCommand ifNotFound)
        {
            return new ParsePathCommand(profiles, path, ifFoundFunc, ifNotFound);
        }

        public static IScriptCommand CreatePath(IProfile profile, string path, bool isFolder, bool renameIfExists,
            Func<IEntryModel, IScriptCommand> thenFunc)
        {
            if (profile is IDiskProfile)
            {
                string parentPath = profile.Path.GetDirectoryName(path);
                string name = profile.Path.GetFileName(path);

                return new DiskCreateCommand(profile as IDiskProfile, 
                    parentPath,
                    renameIfExists ? FileNameGenerator.Rename(name) : FileNameGenerator.NoRename(name), 
                    isFolder, thenFunc);
            }
            return ResultCommand.Error(new NotSupportedException("Profile is not IDiskProfile."));
        }
        
        public static Func<IEntryModel, bool> FileOnlyFilter = em => !em.IsDirectory;
        public static Func<IEntryModel, bool> DirectoryOnlyFilter = em => em.IsDirectory;

        /// <summary>
        /// List contents (to Result:List of IEntryModel) of the specified Directory.
        /// Parameter - Directory, Recrusive, Refresh, Mask
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand List(Func<IEntryModel, bool> filter = null, 
            bool recrusive = false, Func<IEntryModel[], IScriptCommand> nextCommandFunc = null)
        {
            return new ListDirectoryCommand(filter, recrusive, nextCommandFunc);
        }

        public static IScriptCommand List(IEntryModel directory, Func<IEntryModel, bool> filter = null,
            bool recrusive = false, Func<IEntryModel[], IScriptCommand> nextCommandFunc = null)
        {
            return new ListDirectoryCommand(directory, filter, recrusive, nextCommandFunc);
        }
       

        public static IScriptCommand CreatePath(IEntryModel parentModel, string name, bool isFolder, 
            bool renameIfExists,
            Func<IEntryModel, IScriptCommand> thenFunc)
        {
            IProfile profile = parentModel.Profile;
            return CreatePath(profile, profile.Path.Combine(parentModel.FullPath, name), isFolder, renameIfExists, thenFunc);
        }

        public static IScriptCommand ParseOrCreatePath(IDiskProfile profile, string path,
            bool isFolder, Func<IEntryModel, IScriptCommand> thenFunc)
        {
            return ScriptCommands.ParsePath(new[] { profile }, path, thenFunc,
                ScriptCommands.CreatePath(profile, path, isFolder, false, thenFunc));
        }
    }
}

namespace FileExplorer.WPF.Models
{
    public class ParsePathCommand : ScriptCommandBase
    {
        private IProfile[] _profiles;
        private string _path;
        private Func<IEntryModel, IScriptCommand> _ifFoundFunc;
        private IScriptCommand _ifNotFound;

        public ParsePathCommand(IProfile[] profiles, string path, Func<IEntryModel, IScriptCommand> ifFoundFunc,
                IScriptCommand ifNotFound)
            : base("ParsePath")
        {
            _profiles = profiles;
            _path = path;
            _ifFoundFunc = ifFoundFunc;
            _ifNotFound = ifNotFound;
        }


        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            foreach (var p in _profiles)
            {
                var result = await p.ParseAsync(_path);
                if (result != null)
                    return _ifFoundFunc(result);
            }
            return _ifNotFound;
        }
    }

    public class ListDirectoryCommand : ScriptCommandBase
    {
        Func<IEntryModel, bool> _filter = em => true;
        bool _recrusive = false;
        IEntryModel _directory = null;
        private Func<IEntryModel[], IScriptCommand> _nextCommandFunc;

        public ListDirectoryCommand(IEntryModel directory, Func<IEntryModel, bool> filter = null,
            bool recrusive = false, Func<IEntryModel[], IScriptCommand> nextCommandFunc = null)
            : base("List", "Directory", "Mask", "Refresh")
        {
            _filter = filter ?? (em => true);
            _recrusive = recrusive;
            _nextCommandFunc = nextCommandFunc ?? (ems => ResultCommand.NoError) ;
        }

        public ListDirectoryCommand(Func<IEntryModel, bool> filter = null, bool recrusive = false, 
            Func<IEntryModel[], IScriptCommand> nextCommandFunc = null)
           : this(null, filter, recrusive, nextCommandFunc)
        {
        }

        async Task<IList<IEntryModel>> listAsync(IEntryModel dir, CancellationToken ct, 
            Func<IEntryModel,bool> filter, bool refresh)
        {
            List<IEntryModel> retList = new List<IEntryModel>();

            foreach (var em in await dir.Profile.ListAsync(dir, ct, filter, refresh))
            {
                if (filter(em))
                    retList.Add(em);
                ct.ThrowIfCancellationRequested();
                if (_recrusive && em.IsDirectory)
                    retList.AddRange(await listAsync(em, ct, filter, refresh));
            }
            return retList;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEntryModel directory = _directory ?? pm["Directory"] as IEntryModel;
            if (directory == null)
                return ResultCommand.Error(new ArgumentException("Directory"));

            Func<IEntryModel, bool> filter;
            if (pm.ContainsKey("Mask"))
            {
                string mask = pm["Mask"] as string;
                filter = (em => _filter(em) && PathFE.MatchFileMask(em.Name, mask));
            }
            else filter = _filter;

            bool refresh  = pm.ContainsKey("Refresh") && (bool)pm["Refresh"];

            IEntryModel[] result = (await listAsync(directory, pm.CancellationToken, filter, refresh)).ToArray();


            return _nextCommandFunc(result);
        }        
    }

    public class DiskCreateCommand : ScriptCommandBase
    {
        private IDiskProfile _profile;
        private Func<IEntryModel, IScriptCommand> _thenFunc;
        private bool _isFolder;
        private string _parentPath;
        private IFileNameGenerator _fileNameGenerator;

        public DiskCreateCommand(IDiskProfile profile, string parentPath, IFileNameGenerator fileNameGenerator, bool isFolder, 
            Func<IEntryModel, IScriptCommand> thenFunc)
            : base("ParsePath")
        {
            _profile = profile;
            _parentPath = parentPath;
            _fileNameGenerator = fileNameGenerator;
            _thenFunc = thenFunc;
            _isFolder = isFolder;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return !_parentPath.StartsWith("::{"); //Cannot execute if GuidPath
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string fileName = _fileNameGenerator.Generate(); 
            while (fileName != null && 
                await _profile.ParseAsync(_profile.Path.Combine(_parentPath, fileName)) != null)
                fileName = _fileNameGenerator.Generate();
            if (fileName == null)
                return ResultCommand.Error(new ArgumentException("Already exists."));

            string newEntryPath = _profile.Path.Combine(_parentPath, fileName);
            var createddModel = await _profile.DiskIO.CreateAsync(newEntryPath, _isFolder, pm.CancellationToken);

            return new NotifyChangedCommand(_profile, newEntryPath, FileExplorer.Defines.ChangeType.Created,
                _thenFunc(createddModel));
        }
    }
}
