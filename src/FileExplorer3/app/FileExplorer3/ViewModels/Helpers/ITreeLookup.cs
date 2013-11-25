using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeLookup<VM, T>
    {
        Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors);
    }
   

    public class SearchNextLevelOnly<VM, T> : ITreeLookup<VM, T>
    {
        public static SearchNextLevelOnly<VM, T> Instance = new SearchNextLevelOnly<VM, T>();

        public async Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors)
        {
                foreach (VM current in await parentSelector.EntryHelper.LoadAsync())
                    if (current is ISupportTreeSelector<VM, T>)
                    {
                        var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                            case HierarchicalResult.Child:
                                processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                return currentSelectionHelper;
                        }
                    }            
            return null;
        }
    }

    public class SearchNextUsingReverseLookup<VM, T> : ITreeLookup<VM, T>
    {
        public SearchNextUsingReverseLookup(VM targetViewModel)
        {
            _viewModel = targetViewModel;
            _hierarchy = new Stack<ITreeSelector<VM, T>>();
            var current = (_viewModel as ISupportTreeSelector<VM, T>).Selection;
            while (current != null)
            {
                _hierarchy.Push(current);
                current = current.ParentSelector;
            }
        }

        VM _viewModel;
        Stack<ITreeSelector<VM, T>> _hierarchy;
        public async Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors)
        {
                if (parentSelector.EntryHelper.IsLoaded)
                    foreach (VM current in parentSelector.EntryHelper.AllNonBindable)
                    if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Child : 
                            case HierarchicalResult.Current :
                                if (_hierarchy.Contains(currentSelectionHelper))
                                {
                                    processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                    return currentSelectionHelper;
                                }
                                break;
                        }
                    }          
            return null;
        }
    }

    public class RecrusiveSearchUntilFound<VM, T> : ITreeLookup<VM, T>
    {
        public static RecrusiveSearchUntilFound<VM, T> Instance = new RecrusiveSearchUntilFound<VM, T>();

        public async Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
           Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors)
        {

            foreach (VM current in await parentSelector.EntryHelper.LoadAsync())
                    if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                                processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                return currentSelectionHelper;

                            case HierarchicalResult.Child:
                                if (processors.Process(compareResult, parentSelector, currentSelectionHelper))
                                    return await Lookup(value, currentSelectionHelper, compareFunc, processors);

                                break;
                        }
            }
            return null;
        }
    }

    public class RecrusiveSearchIfLoaded<VM, T> : ITreeLookup<VM, T>
    {
        public static RecrusiveSearchIfLoaded<VM, T> Instance = new RecrusiveSearchIfLoaded<VM, T>();

        public async Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
          Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors)
        {

            if (parentSelector.EntryHelper.IsLoaded)
                    foreach (VM current in parentSelector.EntryHelper.AllNonBindable)
                        if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                        {
                            var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                            var compareResult = compareFunc(currentSelectionHelper.Value, value);
                            switch (compareResult)
                            {
                                case HierarchicalResult.Current:
                                    processors.Process(compareResult, parentSelector, currentSelectionHelper);
                                    return currentSelectionHelper;

                                case HierarchicalResult.Child:
                                    if (processors.Process(compareResult, parentSelector, currentSelectionHelper))
                                        return await Lookup(value, currentSelectionHelper, compareFunc, processors);
                                    break;
                            }
            }
            return null;
        }
    }

    public class RecrusiveBroadcastIfLoaded<VM, T> : ITreeLookup<VM, T>
    {
        public static RecrusiveBroadcastIfLoaded<VM, T> Instance = new RecrusiveBroadcastIfLoaded<VM, T>();

        public async Task<ITreeSelector<VM, T>> Lookup(T value, ITreeSelector<VM,T> parentSelector,
          Func<T, T, HierarchicalResult> compareFunc, params ITreeProcessor<VM, T>[] processors)
        {


            if (parentSelector.EntryHelper.IsLoaded)
                foreach (VM current in parentSelector.EntryHelper.AllNonBindable)
                        if (current is ISupportTreeSelector<VM, T> && current is ISupportEntriesHelper<VM>)
                        {
                            var currentSelectionHelper = (current as ISupportTreeSelector<VM, T>).Selection;
                            var compareResult = compareFunc(currentSelectionHelper.Value, value);
                            if (processors.Process(compareResult, parentSelector, currentSelectionHelper))
                                return await Lookup(value, currentSelectionHelper, compareFunc, processors);
                            break;
                        }
            return null;
        }
    }
}
