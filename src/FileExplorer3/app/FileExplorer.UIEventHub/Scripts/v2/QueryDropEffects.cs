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
        public static IScriptCommand QueryDropEffects(string dropTargetVariable = "{ISupportDrop}", string draggablesVariable = "{Draggables}", 
            string dataObjVariable = "{DataObj}", string destinationVariable = "{Effects}", bool skipIfExists = false, IScriptCommand nextCommand = null)
        {
            return new QueryDropEffectsCommand()
            {
                DropTargetKey = dropTargetVariable,
                DraggablesKey = draggablesVariable,
                DataObjectKey = dataObjVariable,
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
    public class QueryDropEffectsCommand : UIScriptCommandBase<UIElement, RoutedEventArgs>
    {        
        /// <summary>
        /// Point to DataContext (ISupportDrop) to initialize the drag, default = "{ISupportDrop}".
        /// </summary>
        public string DropTargetKey { get; set; }

        public string DraggablesKey { get; set; }

        public string DataObjectKey { get; set; }

        /// <summary>
        /// Point to returned value (QueryDropResult) from DropTarget.QueryDrop() method, Default={Effects}.
        /// </summary>
        public string DestinationKey { get; set; }

        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<QueryDrag>();



        public QueryDropEffectsCommand()
            : base("QueryDropResultCommand")
        {
            DropTargetKey = "{ISupportDrop}";
            DraggablesKey = "{Draggables}";
            DataObjectKey = "{DataObj}";
            DestinationKey = "{Effects}";
            SkipIfExists = false;
        }



        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            DragEventArgs devnt = evnt as DragEventArgs;
            ISupportDrop dropTarget = pm.GetValue<ISupportDrop>(DropTargetKey);
            IDraggable[] draggables = pm.GetValue<IDraggable[]>(DraggablesKey);
            if (devnt != null && dropTarget != null && draggables != null)
            {
                var queryDropEffect = dropTarget.QueryDrop(draggables, devnt.AllowedEffects);
                devnt.Effects = queryDropEffect.SupportedEffects;
                pm.SetValue(DestinationKey, queryDropEffect, SkipIfExists);
            }
            return NextCommand;
        }

    }
}
