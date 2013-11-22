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

    public class SetSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetSelected<VM, T> Instance = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (hr == HierarchicalResult.Current)
                if (viewModel is ISupportNodeSelectionHelper<VM, T>)
                    (viewModel as ISupportNodeSelectionHelper<VM, T>).Selection.IsSelected = true;
            return true;
        }
    }

    public class SetExpanded<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetSelected<VM, T> Instance = new SetSelected<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel, VM viewModel)
        {
            if (hr == HierarchicalResult.Child)
                if (viewModel is ISupportNodeSelectionHelper<VM, T>)
                    (viewModel as ISupportNodeSelectionHelper<VM, T>).Entries.IsExpanded = true;
            return true;
        }
    }

    public class SetChildSelected<VM, T> : ITreeSelectionProcessor<VM, T>
    {
        public static SetChildSelected<VM, T> Instance = new SetChildSelected<VM, T>();

        public bool Process(HierarchicalResult hr, VM parentViewModel,VM viewModel)
        {
            if (hr == HierarchicalResult.Child)
                if (parentViewModel is ISupportNodeSelectionHelper<VM, T>)
                    (parentViewModel as ISupportNodeSelectionHelper<VM, T>).Selection.SelectedChild = 
                        (viewModel as ISupportNodeSelectionHelper<VM,T>).Selection.Value;
            return true;
        }
    }
}
