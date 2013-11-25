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
        bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector);
    }

    public static class ITreeSelectionProcessorExtension
    {
        public static bool Process<VM, T>(this ITreeSelectionProcessor<VM, T>[] processors,
            HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            foreach (var p in processors)
                if (!p.Process(hr, parentSelector, selector))
                    return false;
            return true;
        }
    }

    public class TreeSelectionProcessor<VM, T> : ITreeSelectionProcessor<VM, T>
    {

        public TreeSelectionProcessor(HierarchicalResult appliedResult, 
            Func<HierarchicalResult, ITreeSelector<VM, T>, ITreeSelector<VM, T>, bool> processFunc)
        {
            _processFunc = processFunc;
            _appliedResult = appliedResult;
        }

        private Func<HierarchicalResult, ITreeSelector<VM, T>, ITreeSelector<VM, T>, bool> _processFunc;
        private HierarchicalResult _appliedResult;

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_appliedResult.HasFlag(hr))
                return _processFunc(hr, parentSelector, selector);
            return true;
        }
    }

    public class SetSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetSelected<VM, T> Instance = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (hr == HierarchicalResult.Current)
                selector.IsSelected = true;
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

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_hr.HasFlag(hr))                
                    selector.IsSelected = false;
            return true;
        }
    }

    public class SetExpanded<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetExpanded<VM, T> Instance = new SetExpanded<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (hr == HierarchicalResult.Child)
                selector.EntryHelper.IsExpanded = true;
            return true;
        }
    }

    public class SetChildSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildSelected<VM, T> Instance = new SetChildSelected<VM, T>();

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (hr == HierarchicalResult.Child)
               parentSelector.SetSelectedChild(selector.Value);                
            return true;
        }
    }

    public class SetChildNotSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildNotSelected<VM, T> WhenChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Child);
        public static SetChildNotSelected<VM, T> WhenNotChild = new SetChildNotSelected<VM, T>(HierarchicalResult.Current |
            HierarchicalResult.Parent | HierarchicalResult.Unrelated);


        public SetChildNotSelected(HierarchicalResult hr)
        {
            _hr = hr;
        }

        private HierarchicalResult _hr;

        public bool Process(HierarchicalResult hr, ITreeSelector<VM,T> parentSelector, ITreeSelector<VM,T> selector)
        {
            if (_hr.HasFlag(hr))
                
                    selector.SetSelectedChild(default(T));
            return true;
        }
    }

}
