using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeSelectionProcessor<VM, T>
    {
        bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper, 
            ITreeNodeSelectionHelper<VM, T> selectionHelper);
    }

    public static class ITreeSelectionProcessorExtension
    {
        public static bool Process<VM, T>(this ITreeSelectionProcessor<VM, T>[] processors,
            HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper, ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            foreach (var p in processors)
                if (!p.Process(hr, parentSelectionHelper, selectionHelper))
                    return false;
            return true;
        }
    }

    public class SetSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetSelected<VM, T> Instance = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            if (hr == HierarchicalResult.Current)
                selectionHelper.IsSelected = true;
            return true;
        }
    }

    public class SetNotSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetNotSelected<VM, T> WhenCurrent = new SetNotSelected<VM, T>(HierarchicalResult.Current);
        public static SetNotSelected<VM, T> WhenNotCurrent = new SetNotSelected<VM, T>(
            HierarchicalResult.Child | HierarchicalResult.Parent | HierarchicalResult.Unrelated);

        public SetNotSelected(HierarchicalResult hr)
        {
            _hr = hr;
        }

        private HierarchicalResult _hr;

        public bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            if (_hr.HasFlag(hr))
                selectionHelper.IsSelected = false;
            return true;
        }
    }

    public class SetExpanded<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetExpanded<VM, T> Instance = new SetExpanded<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            if (hr == HierarchicalResult.Child)
                selectionHelper.Entries.IsExpanded = true;
            return true;
        }
    }

    public class SetChildSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildSelected<VM, T> Instance = new SetChildSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            if (hr == HierarchicalResult.Child || hr == HierarchicalResult.Current)
                parentSelectionHelper.SetSelectedChild(selectionHelper.Value);                
            return true;
        }
    }

    public class SetChildNotSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildNotSelected<VM, T> WhenChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Current | HierarchicalResult.Child);
        public static SetChildNotSelected<VM, T> WhenNotChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Parent | HierarchicalResult.Unrelated);


        public SetChildNotSelected(HierarchicalResult hr)
        {
            _hr = hr;
        }

        private HierarchicalResult _hr;

        public bool Process(HierarchicalResult hr, ITreeNodeSelectionHelper<VM, T> parentSelectionHelper,
            ITreeNodeSelectionHelper<VM, T> selectionHelper)
        {
            
            if (_hr.HasFlag(hr))
                parentSelectionHelper.SetSelectedChild(default(T));
            return true;
        }
    }
   
}
