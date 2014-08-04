using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.Models;
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
            return new ExplorerParam()
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

    public enum ExplorerParameterType
    {
        //Explorer
        RootModels,

        //FilePicker
        FilePickerMode, FilterStr, 

        //FileList
        EnableDrag, EnableDrop, EnableMultiSelect, 
        ColumnList, ColumnFilters
    }

    public class ExplorerParam : ScriptCommandBase
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

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ExplorerParam>();

        public ExplorerParam()
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
                case ExplorerParameterType.EnableDrag:
                    evm.FileList.EnableDrag = evm.DirectoryTree.EnableDrag = true.Equals(value);
                    break;
                case ExplorerParameterType.EnableDrop:
                    evm.FileList.EnableDrop = evm.DirectoryTree.EnableDrop = true.Equals(value);
                    break;
                case ExplorerParameterType.EnableMultiSelect:
                    evm.FileList.EnableMultiSelect = true.Equals(value);
                    break;
                case ExplorerParameterType.RootModels:
                    if (ValueKey == null)
                        return ResultCommand.Error(new ArgumentNullException("ValueKey"));

                    IEntryModel[] rootModels = ValueKey is string ? 
                        await pm.GetValueAsEntryModelArrayAsync(ValueKey as string, null) :
                        ValueKey as IEntryModel[];
                    if (rootModels == null)
                        return ResultCommand.Error(new KeyNotFoundException(ValueKey.ToString()));
                    evm.RootModels = rootModels;

                    break;
                case ExplorerParameterType.FilePickerMode:
                    var mode = pm.GetValue(ValueKey as string);
                    FilePickerMode pickerMode;
                    if (mode is FilePickerMode)
                        pickerMode = (FilePickerMode)mode;
                    else if (mode is string)
                        Enum.TryParse<FilePickerMode>(mode as string, out pickerMode);
                    else break;
                    if (evm is FilePickerViewModel)
                        (evm as FilePickerViewModel).PickerMode = pickerMode;
                    break;
                case ExplorerParameterType.FilterStr:
                    string filterStr = pm.GetValue<string>(ValueKey as string);
                    if (filterStr != null)
                        if (evm is FilePickerViewModel)
                            (evm as FilePickerViewModel).FilterStr = filterStr;
                    break;
                case ExplorerParameterType.ColumnList:
                    ColumnInfo[] columnInfo = pm.GetValue<ColumnInfo[]>(ValueKey as string);
                    if (columnInfo != null)
                        evm.FileList.Columns.ColumnList = columnInfo;
                    break;
                case ExplorerParameterType.ColumnFilters:
                    ColumnFilter[] columnfilters = pm.GetValue<ColumnFilter[]>(ValueKey as string);
                    if (columnfilters != null)
                        evm.FileList.Columns.ColumnFilters = columnfilters;
                    break;

                default: return ResultCommand.Error(new NotSupportedException(ParameterType.ToString()));
            }

            logger.Info(String.Format("Set {0} to {1} ({2})", ParameterType, ValueKey, value));

            return NextCommand;
        }
    }
}
