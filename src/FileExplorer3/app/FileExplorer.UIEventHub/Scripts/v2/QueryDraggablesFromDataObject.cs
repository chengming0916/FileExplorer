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
        public static IScriptCommand QueryDraggablesFromDataObject(
            string iSupportDropVariable = "{ISupportDrop}", string dataObjectVariable = "{DataObj}", 
            string destinationVariable = "{Draggables}", bool skipIfExists = false, IScriptCommand nextCommand = null)
        {
            return new QueryDraggablesFromDataObject()
            {
                ISupportDropKey = iSupportDropVariable,
                DestinationKey = destinationVariable,
                DataObjectKey = dataObjectVariable,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }
 
    /// <summary>
    /// Obtain draggables array (IDraggable[]) and assign to a variable, or assign null if not found.
    /// </summary>
    public class QueryDraggablesFromDataObject : ScriptCommandBase
    {
        /// <summary>
        /// Point to a ViewModel that support ISupportShellDrop or ISupportDrop, Default = {ISupportDrop}.
        /// </summary>
        public string ISupportDropKey { get; set; }

        /// <summary>
        /// Point to DataObject, or obtain from  Default = {DataObj}.
        /// </summary>
        public string DataObjectKey { get; set; }

        /// <summary>
        /// Point to where draggable (IDraggable[]) is stored, Default = {Draggables}.
        /// </summary>
        public string DestinationKey { get; set; }

        public bool SkipIfExists { get; set; }

        public QueryDraggablesFromDataObject()
            : base("AssignDraggables")
        {
            ISupportDropKey = "{ISupportDrop}";
            DataObjectKey = "{DataObj}";
            DestinationKey = "{Draggables}";
            SkipIfExists = false;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (SkipIfExists && pm.HasValue(DestinationKey))
                return NextCommand;

            IDraggable[] value = new IDraggable[] {};
         
            ISupportShellDrop issd = pm.GetValue<ISupportShellDrop>(ISupportDropKey);
            IDataObject dataObj = pm.GetValue<IDataObject>(DataObjectKey);
            if (issd != null && dataObj != null)
                value = (issd.QueryDropDraggables(dataObj) ?? new IDraggable[] {}).ToArray();

            pm.SetValue(DestinationKey, value, SkipIfExists);
            return NextCommand;                                    
        }
    }
}
