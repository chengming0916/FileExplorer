using FileExplorer.Defines;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileExplorer.UIEventHub
{
    public static partial class HubScriptCommands
    {                        
        /// <summary>
        /// Assumed {FindSelectionMode} is assigned (use DetermineFindSelectionMode())
        /// find selected items and assign to {SelectedList} and {SelectedIdList}
        /// </summary>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand FindSelectedItems(IScriptCommand nextCommand = null)
        {            
            return 
                HubScriptCommands.DetermineFindSelectionMode("{FindSelectionMode}", 
                ScriptCommands.Switch("{FindSelectionMode}",
                      new Dictionary<FindSelectionMode, IScriptCommand>()
                      {
                          { FindSelectionMode.IChildInfo, 
                              HubScriptCommands.FindSelectedItemsUsingIChildInfo()  },
                          { FindSelectionMode.GridView, 
                              HubScriptCommands.FindSelectedItemsUsingGridView()  }
                      },
                      HubScriptCommands.FindSelectedItemsUsingHitTest(),
                      nextCommand));
        }
    }

    //public enum PostFindSelectedItemAction { None, Select, Highlight }

    ///// <summary>
    ///// Call the appropriated FindSelectedItemsXYZ to poll a list of selected list (or selected + unselectedList)
    ///// Then Select or highlight items based on if the ItemsControl IsSelecting.
    ///// </summary>
    //public class FindSelectedItems : UIScriptCommandBase<ItemsControl, RoutedEventArgs>
    //{
    //    /// <summary>
    //    /// Action after selected items found, Default = None
    //    /// </summary>
    //    public PostFindSelectedItemAction ThenAction { get; set; }

    //    /// <summary>
    //    /// Point to output of find selection mode (FindSelectionMode).
    //    /// </summary>
    //    public string FindSelectionModeKey { get; set; }

    //    public FindSelectedItems()
    //        : base("FindSelectedItems")
    //    {
    //        ThenAction = PostFindSelectedItemAction.None;
    //    }

    //    protected override IScriptCommand executeInner(ParameterDic pm, ItemsControl ic, RoutedEventArgs evnt, IUIInput input, IList<IUIInputProcessor> inpProcs)
    //    {
    //        var scp = ControlUtils.GetScrollContentPresenter(ic);
    //        bool isSelecting = AttachedProperties.GetIsSelecting(ic);

    //        IScriptCommand findCommand = null;
    //        IScriptCommand selectCommand = null;
    //        FindSelectionMode fsMode = FindSelectionMode.HitTest;

    //        IChildInfo icInfo = UITools.FindVisualChild<Panel>(scp) as IChildInfo;
    //        if (icInfo != null)
    //            fsMode = FindSelectionMode.IChildInfo;
    //        else
    //            if (ic is ListView && (ic as ListView).View is GridView)
    //                fsMode = FindSelectionMode.GridView;

    //        //IChildInfo icInfo = UITools.FindVisualChild<Panel>(scp) as IChildInfo;
    //        //if (icInfo != null)
    //        //{
    //        //    findCommand = new FindSelectedItemsUsingIChildInfo(ic, icInfo);
    //        //    selectCommand = isSelecting ? HubScriptCommands.SelectItems : HubScriptCommands.HighlightItems;
    //        //}
    //        //else
    //        //    if (ic is ListView && (ic as ListView).View is GridView)
    //        //    {
    //        //        var gview = (ic as ListView).View as GridView;
    //        //        findCommand = new FindSelectedItemsUsingGridView(ic, gview, scp);
    //        //        selectCommand = isSelecting ? HubScriptCommands.SelectItems : HubScriptCommands.HighlightItems;
    //        //    }
    //        //    else
    //        //    {
    //        //        findCommand = new FindSelectedItemsUsingHitTest(ic);
    //        //        selectCommand = isSelecting ? HubScriptCommands.SelectItemsByUpdate : HubScriptCommands.HighlightItemsByUpdate;
    //        //    }

    //        return ScriptCommands.RunCommandsInSequence(NextCommand, findCommand, selectCommand);
    //    }
    //}


}
