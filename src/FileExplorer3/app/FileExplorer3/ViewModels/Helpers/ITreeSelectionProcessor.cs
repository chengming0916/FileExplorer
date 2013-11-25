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
        bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel);
    }

    public static class ITreeSelectionProcessorExtension
    {
        public static bool Process<VM, T>(this ITreeSelectionProcessor<VM, T>[] processors,
            HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            foreach (var p in processors)
                if (!p.Process(hr, parentViewModel, viewModel))
                    return false;
            return true;
        }
    }

    public class TreeSelectionProcessor<VM, T> : ITreeSelectionProcessor<VM, T>
    {

        public TreeSelectionProcessor(HierarchicalResult appliedResult, Func<HierarchicalResult, VM, VM, bool> processFunc)
        {
            _processFunc = processFunc;
            _appliedResult = appliedResult;
        }

        private Func<HierarchicalResult, VM, VM, bool> _processFunc;
        private HierarchicalResult _appliedResult;

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (_appliedResult.HasFlag(hr))
                return _processFunc(hr, parentViewModel, viewModel);
            return true;
        }
    }

    public class SetSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetSelected<VM, T> Instance = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (hr == HierarchicalResult.Current)
                if (viewModel is ISupportTreeSelector<VM, T>)
                    (viewModel as ISupportTreeSelector<VM, T>).Selection.IsSelected = true;
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

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (_hr.HasFlag(hr))
                if (viewModel is ISupportTreeSelector<VM, T>)
                    (viewModel as ISupportTreeSelector<VM, T>).Selection.IsSelected = false;
            return true;
        }
    }

    public class SetExpanded<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetExpanded<VM, T> Instance = new SetExpanded<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (hr == HierarchicalResult.Child)
                if (viewModel is ISupportTreeSelector<VM, T>)
                    (viewModel as ISupportTreeSelector<VM, T>).Entries.IsExpanded = true;
            return true;
        }
    }

    public class SetChildSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildSelected<VM, T> Instance = new SetChildSelected<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (hr == HierarchicalResult.Child)
                if (parentViewModel is ISupportTreeSelector<VM, T>)
                {
                    (parentViewModel as ISupportTreeSelector<VM, T>).Selection.SetSelectedChild(
                        (viewModel as ISupportTreeSelector<VM, T>).Selection.Value);
                }
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

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (_hr.HasFlag(hr))
                if (viewModel is ISupportTreeSelector<VM, T>)
                    (viewModel as ISupportTreeSelector<VM, T>).Selection.SetSelectedChild(default(T));
            return true;
        }
    }

}
