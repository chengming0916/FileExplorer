using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.UIEventHub.Defines;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorer.UIEventHub
{

    public static partial class HubScriptCommands
    {
        public static IScriptCommand QueryDrop(string dropTargetVariable = "{ISupportDrop}", string draggablesVariable = "{Draggables}", 
            string dataObjVariable = "{DataObj}", string dropEffectVariable = "{Effect}", 
            string destinationVariable = "{ResultEffect}",
            bool skipIfExists = false, 
            IScriptCommand nextCommand = null)
        {
            return new QueryDropCommand()
            {
                DropTargetKey = dropTargetVariable,
                DraggablesKey = draggablesVariable,
                DataObjectKey = dataObjVariable,
                DropEffectKey = dropEffectVariable,
                DestinationKey = destinationVariable,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }

    /// <summary>
    /// Use DropTarget's QueryDrop to obtain DragDropEffects (QueryDropResult), which contains
    /// PreferredEffect and SupportedEffects
    /// </summary>
    public class QueryDropCommand : UIScriptCommandBase<UIElement, RoutedEventArgs>
    {        
        /// <summary>
        /// Point to DataContext (ISupportDrop) to initialize the drag, default = "{ISupportDrop}".
        /// </summary>
        public string DropTargetKey { get; set; }

        public string DraggablesKey { get; set; }

        public string DataObjectKey { get; set; }

        /// <summary>
        /// Point to desired DragDropEffect to pass to ISupportDrop.Drop() method, if not set the effect 
        /// will be obtained from the EventArgs (DragEventArgs), Default={DropEffect}.
        /// </summary>
        public string DropEffectKey { get; set; }

        /// <summary>
        /// Point to returned value (DragDropEffects) from DropTarget.Drop() method, Default={DropEffect}.
        /// </summary>
        public string DestinationKey { get; set; }

        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<QueryDrag>();
        


        public QueryDropCommand()
            : base("QueryDropCommand")
        {
            DropTargetKey = "{ISupportDrop}";
            DraggablesKey = "{Draggables}";
            DataObjectKey = "{DataObj}";
            DropEffectKey = "{DropEffect}";
            SkipIfExists = false;
        }



        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            DragEventArgs devnt = evnt as DragEventArgs;
            ISupportDrop dropTarget = pm.GetValue<ISupportDrop>(DropTargetKey);
            IDraggable[] draggables = pm.GetValue<IDraggable[]>(DraggablesKey);
            if (devnt != null && dropTarget != null && draggables != null)
            {
                DragDropEffects effect =
                    pm.HasValue<DragDropEffects>(DropEffectKey) ?
                    pm.GetValue<DragDropEffects>(DropEffectKey) : devnt.AllowedEffects;
                var dataObj = pm.GetValue<IDataObject>(DataObjectKey);

                if (dropTarget is ISupportShellDrop)               
                    devnt.Effects = (dropTarget as ISupportShellDrop).Drop(draggables, dataObj, effect);                
                else devnt.Effects = dropTarget.Drop(draggables, devnt.AllowedEffects);

                devnt.Effects = effect;
                pm.SetValue(DestinationKey, effect, SkipIfExists);
            }
    
            return NextCommand;
        }

    }
}
