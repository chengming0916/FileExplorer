using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public static partial class ScriptCommands
    {
        public static IScriptCommand ParsePath(IProfile[] profiles, string path, Func<IEntryModel, IScriptCommand> ifFoundFunc,
            IScriptCommand ifNotFound)
        {
            return new ParsePathCommand(profiles, path, ifFoundFunc, ifNotFound);
        }

        public static IScriptCommand CreatePath(IDiskProfile profile, string path, bool isFolder, Func<IEntryModel, IScriptCommand> thenFunc)
        {
            return new DiskCreateCommand(profile, path, isFolder, thenFunc);
        }

        public static IScriptCommand ParseOrCreatePath(IDiskProfile profile, string path,
            bool isFolder, Func<IEntryModel, IScriptCommand> thenFunc)
        {
            return ScriptCommands.ParsePath(new[] { profile }, path, thenFunc,
                ScriptCommands.CreatePath(profile, path, isFolder, thenFunc));
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
        private string _path;
        private Func<IEntryModel, IScriptCommand> _thenFunc;
        private bool _isFolder;

        public DiskCreateCommand(IDiskProfile profile, string path, bool isFolder, Func<IEntryModel, IScriptCommand> thenFunc)
            : base("ParsePath")
        {
            _profile = profile;
            _path = path;
            _thenFunc = thenFunc;
            _isFolder = isFolder;
        }


        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var createddModel = await _profile.DiskIO.CreateAsync(_path, _isFolder, pm.CancellationToken);
            return _thenFunc(createddModel);
        }
    }
}
