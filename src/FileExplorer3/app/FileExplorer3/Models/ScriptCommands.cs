using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;

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
}
