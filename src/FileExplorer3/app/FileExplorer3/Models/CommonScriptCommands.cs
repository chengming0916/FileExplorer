using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public class NotifyChangedCommand : ScriptCommandBase
    {

        private string _fullParseName;
        private ChangeType _changeType;
        private IProfile _profile;
        public NotifyChangedCommand(IProfile profile, string fullParseName, ChangeType changeType)
            : base("NotifyChanges")
        {
            _profile = profile;
            _fullParseName = fullParseName;
            _changeType = changeType;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            _profile.NotifyEntryChanges(_fullParseName, _changeType);
            return ResultCommand.NoError;
        }

        public override bool Equals(object obj)
        {
            return obj is NotifyChangedCommand && (obj as NotifyChangedCommand)._profile.Equals(_profile) &&
                (obj as NotifyChangedCommand)._fullParseName.Equals(_fullParseName);
        }

        public override int GetHashCode()
        {
            return _fullParseName.GetHashCode() + _changeType.GetHashCode();
        }
    }



}
