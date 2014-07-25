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
        /// Whether skip (or override) if key already in dictionary, default = false.
        /// </summary>
        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Assign>();

        public Assign()
            : base("Assign")
        {
            VariableKey = "{Variable}";
            Value = null;
            SkipIfExists = false;
        }

        protected Assign(string commandKey)
            : base(commandKey)
        {
            VariableKey = "{Variable}";
            Value = null;
            SkipIfExists = false;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (Value != null)
                if (pm.SetValue<Object>(VariableKey, Value, SkipIfExists))
                    logger.Debug(String.Format("{0} = {1}", VariableKey, Value));
            // else logger.Debug(String.Format("Skipped {0}, already exists.", VariableKey));

            return NextCommand;
        }
    }

}
