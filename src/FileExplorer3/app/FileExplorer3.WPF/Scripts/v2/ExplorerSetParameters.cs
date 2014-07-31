using FileExplorer.Models;
using FileExplorer.WPF.ViewModels;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{

    public static partial class UIScriptCommands
    {
        /// <summary>
        /// Serializable, change parameters of an IExplorerViewModel.
        /// </summary>
        /// <param name="explorerVariable"></param>
        /// <param name="parameterType"></param>
        /// <param name="valueVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ExplorerSetParameters(string explorerVariable = "{Explorer}", 
           ExplorerParameterType parameterType = ExplorerParameterType.EnableDrag,
           object valueVariable = null, IScriptCommand nextCommand = null)
        {
            return new ExplorerSetParameters()
            {
                ExplorerKey = explorerVariable,
                ParameterType = parameterType,
                ValueKey = valueVariable, 
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand ExplorerSetParameters(
           ExplorerParameterType parameterType = ExplorerParameterType.EnableDrag,
           object valueVariable = null, IScriptCommand nextCommand = null)
        {
            return ExplorerSetParameters("{Explorer}", parameterType, valueVariable, nextCommand);
        }
    }

    public enum ExplorerParameterType {  EnableDrag, EnableDrop, EnableMultiSelect, RootModels }

    public class ExplorerSetParameters : ScriptCommandBase
    {
        /// <summary>
        /// Point to Explorer (IExplorerViewModel) to be used.  Default = "{Explorer}".
        /// </summary>
        public string ExplorerKey { get; set; }

        /// <summary>
        /// The Parameter type to work with.
        /// </summary>
        public ExplorerParameterType ParameterType { get; set; }

        /// <summary>
        /// The value to set to, can be a key e.g. ({AnotherVariable}) or actual value (true).
        /// </summary>
        public object ValueKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ExplorerSetParameters>();

        public ExplorerSetParameters()
            : base("ExplorerSetParameters")
        {
            ExplorerKey = "{Explorer}";
            ParameterType = ExplorerParameterType.EnableDrag;
            ValueKey = "true";
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var evm = pm.GetValue<IExplorerViewModel>(ExplorerKey);

            if (evm == null)
                return ResultCommand.Error(new KeyNotFoundException(ExplorerKey));

            object value = ValueKey is string ? pm.GetValue<object>(ValueKey as string) : ValueKey;

            switch (ParameterType)
            {
                case ExplorerParameterType.EnableDrag :
                    evm.FileList.EnableDrag = evm.DirectoryTree.EnableDrag = true.Equals(value);
                    break;
                case ExplorerParameterType.EnableDrop :
                    evm.FileList.EnableDrop = evm.DirectoryTree.EnableDrop = true.Equals(value);                        
                    break;
                case ExplorerParameterType.EnableMultiSelect :
                    evm.FileList.EnableMultiSelect = true.Equals(value);                        
                    break;
                case ExplorerParameterType.RootModels :
                    if (ValueKey == null)
                        return ResultCommand.Error(new ArgumentNullException("ValueKey"));

                    IEntryModel[] rootModels = ValueKey is string ? pm.GetValue<IEntryModel[]>(ValueKey as string) :
                        ValueKey as IEntryModel[];
                    if (rootModels == null)                        
                        return ResultCommand.Error(new KeyNotFoundException(ValueKey.ToString()));
                    evm.RootModels = rootModels;

                    break;

                default: return ResultCommand.Error(new NotSupportedException(ParameterType.ToString()));
            }

            logger.Info(String.Format("Set {0} to {1}", ParameterType, ValueKey));

            return NextCommand;
        }
    }
}
