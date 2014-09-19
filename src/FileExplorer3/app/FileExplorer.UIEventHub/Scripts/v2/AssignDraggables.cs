using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        /// <summary>
        /// Obtain draggables (IDraggable[]) and assign to a variable, or assign null if not found.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="iSupportDropVariable"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand AssignDraggables(AssignDraggagablesMode mode = AssignDraggagablesMode.FromISupportShellDrop, 
            string iSupportDropVariable = "{ISupportDrop}",
            string destinationVariable = "{Draggables}", bool skipIfExists = false, IScriptCommand nextCommand = null)
        {
            return new AssignDraggables()
            {
                Mode = mode,
                ISupportDropKey = iSupportDropVariable,
                DestinationKey = destinationVariable,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }

    public enum AssignDraggagablesMode {  FromISupportShellDrop }

    /// <summary>
    /// Obtain draggables array (IDraggable[]) and assign to a variable, or assign null if not found.
    /// </summary>
    public class AssignDraggables : UIScriptCommandBase
    {
        public AssignDraggagablesMode Mode { get; set; }

        /// <summary>
        /// Point to a ViewModel that support ISupportShellDrop or ISupportDrop, Default = {ISupportDrop}.
        /// </summary>
        public string ISupportDropKey { get; set; }

        /// <summary>
        /// Point to where draggable (IDraggable[]) is stored, Default = {Draggables}.
        /// </summary>
        public string DestinationKey { get; set; }

        public bool SkipIfExists { get; set; }

        public AssignDraggables()
            : base("AssignDraggables")
        {
            Mode = AssignDraggagablesMode.FromISupportShellDrop;
            ISupportDropKey = "{ISupportDrop}";
            DestinationKey = "{Draggables}";
            SkipIfExists = false;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (SkipIfExists && pm.HasValue(DestinationKey))
                return NextCommand;

            object value = null;
            switch (Mode)
            {
                case AssignDraggagablesMode.FromISupportShellDrop :
                     ISupportShellDrop issd = pm.GetValue<ISupportShellDrop>(ISupportDropKey);
                     IUIDragInput dragInp = pm.GetValue<IUIDragInput>(base.InputKey);

                    if (issd is ISupportShellDrop && dragInp != null)
                    {                        
                        IDataObject dataObj = dragInp.Data;
                        if (dataObj != null)
                        {
                            value = (issd.QueryDropDraggables(dataObj) ?? new IDraggable[] {}).ToArray();                            
                        }
                    }
                    break;
            }

            pm.SetValue(DestinationKey, value, SkipIfExists);
            return NextCommand;                                    
        }
    }
}
