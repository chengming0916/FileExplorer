using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {        
        public static IScriptCommand AttachDragDropAdorner(
            IScriptCommand nextCommand = null)
        {
            return AssignAdornerLayer("PART_DragDropAdorner", AssignDragDropAdorner.AdornerLayerKey, false,
                  new AssignDragDropAdorner()
                  {
                        NextCommand = (ScriptCommandBase)
                        HubScriptCommands.AttachAdorner(AssignDragDropAdorner.AdornerLayerKey, AssignDragDropAdorner.AdornerKey)
                  });
                   
        }

        public static IScriptCommand DetachDragDropAdorner(
            IScriptCommand nextCommand = null)
        {
            return ScriptCommands.IfAssigned(AssignDragDropAdorner.AdornerKey,
                    AssignAdornerLayer("PART_DragDropAdorner", AssignDragDropAdorner.AdornerLayerKey, false,
                    HubScriptCommands.DetachAdorner(AssignDragDropAdorner.AdornerLayerKey, AssignDragDropAdorner.AdornerKey)));
        }
    }

    public class AssignDragDropAdorner : UIScriptCommandBase<UIElement, RoutedEventArgs>
    {
        /// <summary>
        /// Point to where to store the DragDropAdorner, default={DragDrop.DragDropAdorner}.
        /// </summary>
        public static string AdornerKey { get; set; }

          /// <summary>
        /// Point to the adorner layer to attach/detach adorner, Default = {DragDrop.AdornerLayer}
        /// </summary>
        public static string AdornerLayerKey { get; set; }

        static AssignDragDropAdorner()
        {
            AdornerKey = "{DragDrop.DragDropAdorner}";
            AdornerLayerKey = "{DragDrop.AdornerLayer}";
        }

        public AssignDragDropAdorner()
            : base("AssignDragDropAdorner")
        {
       
        }


        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            var adornerLayer = pm.GetValue<AdornerLayer>(AdornerLayerKey);
            DragAdorner adorner = new DragAdorner(adornerLayer);
            pm.SetValue(AdornerKey, adorner);
            return NextCommand;
        }

      
    }
}
