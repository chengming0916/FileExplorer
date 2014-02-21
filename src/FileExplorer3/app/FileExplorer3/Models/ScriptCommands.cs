using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels
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

namespace FileExplorer.Models
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


        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            string fileName = _fileNameGenerator.Generate(); 
            while (fileName != null && 
                await _profile.ParseAsync(_profile.Path.Combine(_parentPath, fileName)) != null)
                fileName = _fileNameGenerator.Generate();
            if (fileName == null)
                return ResultCommand.Error(new ArgumentException("Already exists."));

            var createddModel = await _profile.DiskIO.CreateAsync(
                _profile.Path.Combine(_parentPath, fileName), _isFolder, pm.CancellationToken);
            return _thenFunc(createddModel);
        }
    }
}
