using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using MetroLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand AttachResizeItemAdorner(
            string adornerLayerVariable = "{AdornerLayer}",
            string resizeItemAdornerVariable = "{ResizeItemAdorner}",
            IScriptCommand nextCommand = null)
        {
            return new ResizeItemAdornerCommand()
            {
                AdornerLayerKey = adornerLayerVariable,
                ResizeItemAdornerKey = resizeItemAdornerVariable,
                AdornerMode = UIEventHub.AdornerMode.Attach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand DettachResizeItemAdorner(
            string adornerLayerVariable = "{AdornerLayer}",
            string resizeItemAdornerVariable = "{ResizeItemAdorner}",
            IScriptCommand nextCommand = null)
        {
            return new ResizeItemAdornerCommand()
            {
                AdornerLayerKey = adornerLayerVariable,
                ResizeItemAdornerKey = resizeItemAdornerVariable,
                AdornerMode = UIEventHub.AdornerMode.Detach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        //public static IScriptCommand UpdateResizeItemAdorner(string resizeItemAdornerVariable = "{ResizeItemAdorner}", 
        //    string targetItemVariable = null,
        //    IScriptCommand nextCommand = null)
        //{
        //    return new ResizeItemAdornerCommand()
        //    {
        //        AdornerMode = UIEventHub.AdornerMode.Update,
        //        TargetItemKey = targetItemVariable,
        //        NextCommand = (ScriptCommandBase)nextCommand
        //    };
        //}
    }


    public class ResizeItemAdornerCommand : UIScriptCommandBase<ItemsControl, RoutedEventArgs>
    {
        public AdornerMode AdornerMode { get; set; }

        /// <summary>
        /// Point to the adorner layer to attach/detach adorner, Default = {CanvasResize.AdornerLayer}
        /// </summary>
        public string AdornerLayerKey { get; set; }

        /// <summary>
        /// If attach, the selection adorner (of type ResizeItemAdorner) will be set to the key, 
        /// If update, 
        /// If detach, the ResizeItemAdorner will be point to null.
        /// Default = "{CanvasResize.ResizeItemAdorner}".
        /// </summary>
        public string ResizeItemAdornerKey { get; set; }

        /// <summary>
        /// If set, the selected item (IResizable) will be used for adorner's datacontext, 
        /// otherwise lookup from ItemsControl, Default = null.
        /// </summary>
        public string TargetItemKey { get; set; }


        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ResizeItemAdornerCommand>();


        public ResizeItemAdornerCommand()
            : base("ResizeItemAdornerCommand")
        {
            AdornerLayerKey = "{CanvasResize.AdornerLayer}";
            ResizeItemAdornerKey = "{CanvasResize.ResizeItemAdorner}";
            TargetItemKey = null;
            //StartPositionAdjustedKey = "{StartPositionAdjusted}";
            //CurrentPositionKey = "{CurrentPosition}";
        }

        protected override Script.IScriptCommand executeInner(ParameterDic pm, ItemsControl ic,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            if (scp != null)
            {
                AdornerLayer adornerLayer = pm.GetValue<AdornerLayer>(AdornerLayerKey);
                if (adornerLayer != null)

                switch (AdornerMode)
                {
                    case UIEventHub.AdornerMode.Attach:
                       
                         IResizable targetItem = TargetItemKey == null ?
                            (ic.ItemsSource as IEnumerable).OfType<ISelectable>()
                                .FirstOrDefault(s => s is IResizable && s.IsSelected) as IResizable :
                            pm.GetValue<IResizable>(TargetItemKey);

                        UIElement selectedItem = ic.ItemContainerGenerator.ContainerFromItem(targetItem) as UIElement;
                        ResizeItemAdorner resizeAdorner = new ResizeItemAdorner(adornerLayer) { SelectedItem = targetItem };
                        pm.SetValue(ResizeItemAdornerKey, resizeAdorner, false);
                        adornerLayer.Add(resizeAdorner);

                        break;

                    case UIEventHub.AdornerMode.Detach:
                        ResizeItemAdorner resizeAdorner1 = pm.GetValue<ResizeItemAdorner>(ResizeItemAdornerKey);
                        if (resizeAdorner1 != null)
                            adornerLayer.Remove(resizeAdorner1);
                        pm.SetValue<object>(ResizeItemAdornerKey, null);                                               
                        break;

                    case UIEventHub.AdornerMode.Update:
                        
                       
                        

                        //var updateAdorner = pm.GetValue<ResizeItemAdorner>(ResizeItemAdornerKey) ??
                        //    AttachedProperties.GetResizeItemAdorner(scp);

                        
                        //adornerLayer = UITools.FindVisualChild<AdornerLayer>(selectedItem);
                        //ResizeItemAdorner updateAdorner = new ResizeItemAdorner(adornerLayer);
                        //adornerLayer.Add(updateAdorner);

                        ////if (updateAdorner == null)
                        ////    return ResultCommand.Error(new Exception("Adorner not found."));


                        //updateAdorner.SelectedItem = targetItem;
                        break;

                }



            }
            return NextCommand;
        }

    }
}
