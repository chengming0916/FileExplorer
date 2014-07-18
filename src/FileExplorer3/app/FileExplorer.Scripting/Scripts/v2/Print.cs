using FileExplorer.Script;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {
        /// <summary>
        /// Serializable, print content of a variable to debug.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand PrintDebug(string variable, IScriptCommand nextCommand = null)
        {
            return new Print()
            {
                DestinationType = Print.PrintDestinationType.Debug,
                VariableKey = variable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, print content of a variable to logger.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand PrintLogger(string variable, IScriptCommand nextCommand = null)
        {
            return new Print()
            {
                DestinationType = Print.PrintDestinationType.Logger,
                VariableKey = variable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }

    /// <summary>
    /// Serializable, print content of a variable to debug.
    /// </summary>
    public class Print : ScriptCommandBase
    {
        [Flags]
        public enum PrintDestinationType { Logger = 1 << 0, Debug = 1 << 1 }

        /// <summary>
        /// Variable to print.
        /// </summary>
        public string VariableKey { get; set; }

        /// <summary>
        /// Where to print to.
        /// </summary>
        public PrintDestinationType DestinationType { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Print>();

        public Print()
            : base("Print")
        {
            DestinationType = PrintDestinationType.Logger;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            string variable = pm.ReplaceVariableInsideBracketed(VariableKey);

            if (DestinationType.HasFlag(PrintDestinationType.Debug))
                Debug.WriteLine(variable);
            if (DestinationType.HasFlag(PrintDestinationType.Logger))
                logger.Info(variable);

            return NextCommand;
        }

    }

}
