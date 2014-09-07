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

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {
        public static IScriptCommand AttachResizeItemAdorner(string ResizeItemAdornerVariable = "{ResizeItemAdorner}", 
            IScriptCommand nextCommand = null)
        {
            return new ResizeItemAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Attach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand DettachResizeItemAdorner(string ResizeItemAdornerVariable = "{ResizeItemAdorner}", 
            IScriptCommand nextCommand = null)
        {
            return new ResizeItemAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Detach,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand UpdateResizeItemAdorner(string ResizeItemAdornerVariable = "{ResizeItemAdorner}", 
            IScriptCommand nextCommand = null)
        {
            return new ResizeItemAdornerCommand()
            {
                AdornerMode = UIEventHub.AdornerMode.Update,                
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }
    }


    public class ResizeItemAdornerCommand : UIScriptCommandBase<ItemsControl, RoutedEventArgs>
    {
        public AdornerMode AdornerMode { get; set; }

        /// <summary>
        /// If attach, the selection adorner (of type ResizeItemAdorner) will be set to the key, 
        /// If update, 
        /// If detach, the ResizeItemAdorner will be point to null.
        /// Default = "{ResizeItemAdorner}".
        /// </summary>
        public string ResizeItemAdornerKey { get; set; }

        ///// <summary>
        ///// Start position relative to sender, adjusted with scrollbar position.
        ///// </summary>
        //public string StartPositionAdjustedKey { get; set; }

        ///// <summary>
        ///// Current position relative to sender, regardless the scrollbar.
        ///// </summary>
        //public string CurrentPositionKey { get; set; }


        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ResizeItemAdornerCommand>();


        public ResizeItemAdornerCommand()
            : base("ResizeItemAdornerCommand")
        {
            ResizeItemAdornerKey = "{ResizeItemAdorner}";            
            //StartPositionAdjustedKey = "{StartPositionAdjusted}";
            //CurrentPositionKey = "{CurrentPosition}";
        }

        protected override Script.IScriptCommand executeInner(ParameterDic pm, ItemsControl ic,
            RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            var scp = ControlUtils.GetScrollContentPresenter(ic);
            if (scp != null)
            {
                AdornerLayer adornerLayer = ControlUtils.GetAdornerLayer(ic);

                switch (AdornerMode)
                {
                    case UIEventHub.AdornerMode.Attach:
                        if (adornerLayer == null)
                            return ResultCommand.Error(new Exception("Adorner layer not found."));
                        if (AttachedProperties.GetResizeItemAdorner(scp) == null)
                        {

                            //Create and register adorner.
                            ResizeItemAdorner adorner = new ResizeItemAdorner(scp);
                            pm.SetValue(ResizeItemAdornerKey, adorner);
                            AttachedProperties.SetResizeItemAdorner(scp, adorner);
                            AttachedProperties.SetLastScrollContentPresenter(ic, scp); //For used when detach.

                            adornerLayer.Add(adorner);
                        }
                        break;

                    case UIEventHub.AdornerMode.Detach:

                        var lastScp = AttachedProperties.GetLastScrollContentPresenter(ic); //For used when detach.
                        var lastAdorner = AttachedProperties.GetResizeItemAdorner(scp);
                        if (lastAdorner != null)
                            adornerLayer.Remove(lastAdorner);

                        AttachedProperties.SetLastScrollContentPresenter(ic, null);
                        AttachedProperties.SetResizeItemAdorner(scp, null);
                        pm.SetValue<Object>(ResizeItemAdornerKey, null);
                        break;

                    case UIEventHub.AdornerMode.Update:
                        var updateAdorner = pm.GetValue<ResizeItemAdorner>(ResizeItemAdornerKey) ??
                            AttachedProperties.GetResizeItemAdorner(scp);

                        if (updateAdorner == null)
                            return ResultCommand.Error(new Exception("Adorner not found."));

                        updateAdorner.SetTargetItem((
                            ic.ItemsSource as IEnumerable).OfType<ISelectable>()
                            .FirstOrDefault(s => s is IResizable && s.IsSelected) as IResizable);
                        break;

                }



            }
            return NextCommand;
        }

    }
}
