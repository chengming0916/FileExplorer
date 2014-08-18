using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    public class ExplorerShowMdi : ScriptCommandBase
    {
        
        public string MdiWindowKey { get; set; }

        public string ExplorerKey { get; set; }


        public ExplorerShowMdi()
            : base("ExplorerShowMdi")
        {
            MdiWindowKey = "{MdiWindow}";
            ExplorerKey = "{Explorer}";
        }

        public override IScriptCommand Execute(FileExplorer.ParameterDic pm)
        {
            return base.Execute(pm);
        }
    }
}
