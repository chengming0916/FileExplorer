using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeSelectionLookup<VM, T>
    {
        Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors);
    }

    public class SearchNextLevelOnly<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static SearchNextLevelOnly<VM, T> Instance = new SearchNextLevelOnly<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
             Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            foreach (VM current in await selectionHelper.Entries.LoadAsync())
                if (current is ISupportNodeSelectionHelper<VM, T>)
                {
                    var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
                    var compareResult = compareFunc(currentSelectionHelper.Value, value);
                    switch (compareResult)
                    {
                        case HierarchicalResult.Current:
                        case HierarchicalResult.Child:
                            processors.Process(compareResult, selectionHelper, currentSelectionHelper);
                            return currentSelectionHelper;
                    }
                }

            return null;
        }
    }

    public class SearchNextUsingReverseLookup<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public SearchNextUsingReverseLookup(VM targetViewModel)
        {
            _viewModel = targetViewModel;
            _hierarchy = new Stack<ITreeNodeSelectionHelper<VM, T>>();
            var current = (_viewModel as ISupportNodeSelectionHelper<VM, T>).Selection;
            while (current != null)
            {
                _hierarchy.Push(current);
                current = current.ParentSelectionHelper;
            }
        }

        VM _viewModel;
        Stack<ITreeNodeSelectionHelper<VM, T>> _hierarchy;
        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {

            if (selectionHelper.Entries.IsLoaded)
                foreach (VM current in selectionHelper.Entries.AllNonBindable)
                    if (current is ISupportNodeSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Child:
                            case HierarchicalResult.Current:
                                if (_hierarchy.Contains(currentSelectionHelper))
                                {
                                    processors.Process(compareResult, selectionHelper, currentSelectionHelper);
                                    return currentSelectionHelper;
                                }
                                break;
                        }
                    }
            return null;
        }
    }

    //public class ReceusiveSearchUsingReverseLookup<VM, T> : ITreeSelectionLookup<VM, T>
    //{
    //    public ReceusiveSearchUsingReverseLookup(IEnumerable<ITreeNodeSelectionHelper<VM, T>> path)
    //    {
    //        _path = path == null ? null : new Queue<ITreeNodeSelectionHelper<VM, T>>(path);
    //    }

    //    Queue<ITreeNodeSelectionHelper<VM, T>> _path;

    //    public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
    //        Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
    //    {
    //        if (_path == null)
    //            return null;

    //        if (selectionHelper.Entries.IsLoaded)
    //        {
    //            var currentSelectionHelperFromPath = _path.Dequeue();
    //            foreach (VM current in selectionHelper.Entries.AllNonBindable)
    //            {
    //                var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;

    //                var compareResult2 = compareFunc(currentSelectionHelper.Value, currentSelectionHelperFromPath.Value);
    //                var compareResult = compareFunc(currentSelectionHelper.Value, value);
    //                switch (compareResult)
    //                {
    //                    case HierarchicalResult.Current:
    //                        processors.Process(compareResult, selectionHelper, currentSelectionHelper);
    //                        foreach (var p in _path)
    //                            processors.Process(HierarchicalResult.Parent, p, null);

    //                        return currentSelectionHelper;

    //                    case HierarchicalResult.Child:
    //                        if (compareResult2 == HierarchicalResult.Current)
    //                            if (processors.Process(compareResult, selectionHelper, currentSelectionHelper))
    //                                return await Lookup(value, currentSelectionHelper, compareFunc, processors);
    //                        break;
    //                }
    //            }

    //            throw new KeyNotFoundException(currentSelectionHelperFromPath.Value.ToString());
    //        }
    //        return null;
    //    }
    //}

    public class RecrusiveSearchUntilFound<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveSearchUntilFound<VM, T> Instance = new RecrusiveSearchUntilFound<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
           Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            foreach (VM current in await selectionHelper.Entries.LoadAsync())
                if (current is ISupportNodeSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                {
                    var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
                    var compareResult = compareFunc(currentSelectionHelper.Value, value);
                    switch (compareResult)
                    {
                        case HierarchicalResult.Current:
                            processors.Process(compareResult, selectionHelper, currentSelectionHelper);
                            return currentSelectionHelper;

                        case HierarchicalResult.Child:
                            if (processors.Process(compareResult, selectionHelper, currentSelectionHelper))
                                return await Lookup(value, currentSelectionHelper, compareFunc, processors);

                            break;
                    }
                }
            return null;
        }
    }

    public class RecrusiveSearchIfLoaded<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveSearchIfLoaded<VM, T> Instance = new RecrusiveSearchIfLoaded<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
           Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (selectionHelper.Entries.IsLoaded)
                foreach (VM current in selectionHelper.Entries.AllNonBindable)
                    if (current is ISupportNodeSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                                processors.Process(compareResult, selectionHelper, currentSelectionHelper);
                                return currentSelectionHelper;

                            case HierarchicalResult.Child:
                                if (processors.Process(compareResult, selectionHelper, currentSelectionHelper))
                                    return await Lookup(value, currentSelectionHelper, compareFunc, processors);
                                break;
                        }

                    }
            return null;
        }
    }

    public class RecrusiveBroadcastIfLoaded<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveBroadcastIfLoaded<VM, T> Instance = new RecrusiveBroadcastIfLoaded<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, ITreeNodeSelectionHelper<VM, T> selectionHelper,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {

            if (selectionHelper.Entries.IsLoaded)
                foreach (VM current in selectionHelper.Entries.AllNonBindable)
                    if (current is ISupportNodeSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportNodeSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        if (processors.Process(compareResult, selectionHelper, currentSelectionHelper))
                            return await Lookup(value, currentSelectionHelper, compareFunc, processors);
                        break;
                    }
            return null;
        }
    }
}
