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
        public static IScriptCommand StartDragDrop(string dragSourceVariable = "{ISupportDrag}",
          IScriptCommand nextCommand = null, IScriptCommand cancelCommand = null)
        {
            return new DragDropCommand()
            {
                State = DragDropState.StartShell,
                DragSourceKey = dragSourceVariable,
                NextCommand = (ScriptCommandBase)nextCommand,
                CancelCommand = (ScriptCommandBase)cancelCommand
            };
        }
    }

    public enum DragDropState { StartShell, ContinueShell, EndShell }
    public enum DragMethod { None, Normal, Menu }

    public class DragDropCommand : UIScriptCommandBase<UIElement, RoutedEventArgs>
    {
        public DragDropState State { get; set; }

        /// <summary>
        /// Point to DataContext (ISupportDrag) to initialize the drag, default = "{ISupportDrag}".
        /// </summary>
        public string DragSourceKey { get; set; }

        /// <summary>
        /// Point to DataContext (ISupportDrop) to initialize the drag, default = "{ISupportDrop}".
        /// </summary>
        public string DropTargetKey { get; set; }

        public ScriptCommandBase CancelCommand { get; set; }


        public static string DragDropModeKey { get { return DragDropLiteCommand.DragDropModeKey; } }

        /// <summary>
        ///  Specify the DragMethod (DragMethod, Menu/Normal), Default=Normal.
        /// Store DragMethod in ParameterDic for further use, Default={DragDrop.DragDropMode}.
        /// </summary>
        public static string DragMethodKey { get; set; }

        //public static string DataObjectKey { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DragDropCommand>();


        static DragDropCommand()
        {
            DragMethodKey = "{DragDrop.DragMethod}";
        }


        public DragDropCommand()
            : base("DragDropCommand")
        {
            DragSourceKey = "{ISupportDrag}";
            DropTargetKey = "{ISupportDrop}";
        }



        protected override IScriptCommand executeInner(ParameterDic pm, UIElement sender, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
        {
            switch (State)
            {
                case DragDropState.StartShell : 
                    return executeStartDragDrop(pm, sender);

                case DragDropState.ContinueShell : 
 
                case DragDropState.EndShell :
                    
                    switch (pm.GetValue(DragMethodKey, DragMethod.Normal))
                    {
                        //case DragMethod.Menu :
                        default:
                            return NextCommand;
                        //default: return HubScriptCommands.DetachDragDropAdorner(NextCommand);
                    }
                    
                default: throw new NotSupportedException(State.ToString());
            }            
        }

        #region StartDragDrop

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            //The control, like treeview or listview
            FrameworkElement control = sender as FrameworkElement;

            //ESC pressed
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                control.AllowDrop = true;
            }
            else
                if (e.KeyStates == DragDropKeyStates.None)
                {
                    _dataObj.SetData(ShellClipboardFormats.CFSTR_INDRAGLOOP, 0);
                    e.Action = DragAction.Drop;
                }
                else
                    e.Action = DragAction.Continue;

            _dataObj.SetData(typeof(DragDropKeyStates), e.KeyStates);

            e.Handled = true;
        }

        private void OnPreviewDrop(object sender, QueryContinueDragEventArgs e)
        {

        }

        private IScriptCommand executeStartDragDrop(ParameterDic pm, UIElement sender)
        {
            //Debug.WriteLine(String.Format("DoDragDrop"));

            ISupportShellDrag _isd = pm.GetValue<ISupportShellDrag>(DragSourceKey);
            var draggables = _isd.GetDraggables().ToList();
            _dataObj = _isd.GetDataObject(draggables);

            if (_dataObj == null)
                return ResultCommand.NoError; //Nothing to drag.

            var effect = _isd.QueryDrag(draggables);

            System.Windows.DragDrop.AddQueryContinueDragHandler(sender, new QueryContinueDragEventHandler(OnQueryContinueDrag));

            _dataObj.SetData(typeof(ISupportDrag), _isd);

            //Determine and set the desired drag method. (Normal, Menu)            
            _dataObj.SetData(typeof(DragMethod), pm.GetValue(DragMethodKey, DragMethod.Normal));
            //Notify Shell DragDrop mode is activated.
            pm.SetValue(DragDropModeKey, "Shell", false);

            foreach (var d in draggables) d.IsDragging = true;

            DragDropEffects resultEffect;
            try
            {
                //Start the DragDrop.
                resultEffect = System.Windows.DragDrop.DoDragDrop(sender, _dataObj, effect);                
            }
            finally
            {
                //Reset DragDropMode.
                pm.SetValue<string>(DragDropModeKey, null, false);
                //Notify draggables not IsDragging.
                foreach (var d in draggables) d.IsDragging = false;
                System.Windows.DragDrop.RemoveQueryContinueDragHandler(sender, new QueryContinueDragEventHandler(OnQueryContinueDrag));
                //Debug.WriteLine(String.Format("NotifyDropCompleted {0}", resultEffect));
            }
            var dataObj = _dataObj;
            _dataObj = null;

            return resultEffect != DragDropEffects.None ? NextCommand : CancelCommand;
        }

        #endregion


        #region Data

        private IDataObject _dataObj = null;

        #endregion

    }
}
