using FileExplorer.Script;
using MetroLog;
using System;
using System.Collections;
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
        /// Serializable, given an array, iterate it with NextCommand, then run ThenCommand when all iteration is finished.
        /// </summary>
        /// <param name="ItemsVariable"></param>
        /// <param name="currentItemVariable"></param>
        /// <param name="doCommand"></param>
        /// <param name="thenCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ForEach(string ItemsVariable = "{Items}", string currentItemVariable = "{CurrentItem}",
            IScriptCommand doCommand = null, IScriptCommand thenCommand = null)
        {
            return new ForEach()
            {
                ItemsKey = ItemsVariable,
                CurrentItemKey = currentItemVariable,
                NextCommand = (ScriptCommandBase)doCommand,
                ThenCommand = (ScriptCommandBase)thenCommand
            };
        }
    }

    /// <summary>
    /// Serializable, given an array, iterate it with NextCommand, then run ThenCommand when all iteration is finished.
    /// </summary>
    public class ForEach : ScriptCommandBase
    {
        /// <summary>
        /// Array of item to be iterated, support IEnumerable or Array, Default=Items
        /// </summary>
        public string ItemsKey { get; set; }
        /// <summary>
        /// When iterating item (e.g. i in foreach (var i in array)), the current item will be stored in this key.
        /// Default = CurrentItem
        /// </summary>
        public string CurrentItemKey { get; set; }

        /// <summary>
        /// Iteration command is run in NextCommand, when all iteration complete ThenCommand is run.
        /// </summary>
        public ScriptCommandBase ThenCommand { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ForEach>();

        public ForEach()
            : base("ForEach")
        {
            CurrentItemKey = "{CurrentItem}";
            ItemsKey = "{Items}";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEnumerable e = pm.GetValue<IEnumerable>(ItemsKey);
            if (e == null)
                return ResultCommand.Error(new ArgumentException(ItemsKey));

            uint counter = 0;
            foreach (var item in e)
            {
                counter++;
                pm.SetValue(CurrentItemKey, item);
                await ScriptRunner.RunScriptAsync(pm.Clone(), NextCommand);
                if (pm.Error != null)
                {
                    pm.SetValue<Object>(CurrentItemKey, null);
                    return ResultCommand.Error(pm.Error);
                }
            }
            logger.Info("Looped {0} items", counter);
            pm.SetValue<Object>(CurrentItemKey, null);

            return ThenCommand;
        }
    }
}
