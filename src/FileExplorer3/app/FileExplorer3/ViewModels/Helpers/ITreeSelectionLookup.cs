﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ITreeSelectionLookup<VM, T>
    {
        Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors);
    }

    public class SearchNextLevelOnly<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static SearchNextLevelOnly<VM, T> Instance = new SearchNextLevelOnly<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (parentViewModel is ISupportSubEntriesHelper<VM>)
            {
                var entries = (parentViewModel as ISupportSubEntriesHelper<VM>).Entries;
                foreach (VM current in await entries.LoadAsync())
                    if (current is ISupportSelectionHelper<VM, T>)
                    {
                        var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                            case HierarchicalResult.Child:
                                processors.Process(compareResult, parentViewModel, current);
                                return currentSelectionHelper;
                        }
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
            var current = (_viewModel as ISupportSelectionHelper<VM, T>).Selection;
            while (current != null)
            {
                _hierarchy.Push(current);
                current = current.ParentSelectionHelper;
            }
        }

        VM _viewModel;
        Stack<ITreeNodeSelectionHelper<VM, T>> _hierarchy;
        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
            Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (parentViewModel is ISupportSubEntriesHelper<VM>)
            {
                var entries = (parentViewModel as ISupportSubEntriesHelper<VM>).Entries;
                if (entries.IsLoaded)
                    foreach (VM current in entries.AllNonBindable)
                    if (current is ISupportSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Child : 
                            case HierarchicalResult.Current :
                                if (_hierarchy.Contains(currentSelectionHelper))
                                {
                                    processors.Process(compareResult, parentViewModel, current);
                                    return currentSelectionHelper;
                                }
                                break;
                        }
                    }
            }
            return null;
        }
    }

    public class RecrusiveSearchUntilFound<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveSearchUntilFound<VM, T> Instance = new RecrusiveSearchUntilFound<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
           Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (parentViewModel is ISupportSubEntriesHelper<VM>)
            {
                var entries = (parentViewModel as ISupportSubEntriesHelper<VM>).Entries;
                foreach (VM current in await entries.LoadAsync())
                    if (current is ISupportSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                    {
                        var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                        var compareResult = compareFunc(currentSelectionHelper.Value, value);
                        switch (compareResult)
                        {
                            case HierarchicalResult.Current:
                                processors.Process(compareResult, parentViewModel, current);
                                return currentSelectionHelper;

                            case HierarchicalResult.Child:
                                if (processors.Process(compareResult, parentViewModel, current))
                                    return await Lookup(value, current, compareFunc, processors);

                                break;
                        }
                    }
            }
            return null;
        }
    }

    public class RecrusiveSearchIfLoaded<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveSearchIfLoaded<VM, T> Instance = new RecrusiveSearchIfLoaded<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
          Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (parentViewModel is ISupportSubEntriesHelper<VM>)
            {
                var entries = (parentViewModel as ISupportSubEntriesHelper<VM>).Entries;
                if (entries.IsLoaded)
                    foreach (VM current in entries.AllNonBindable)
                        if (current is ISupportSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                        {
                            var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                            var compareResult = compareFunc(currentSelectionHelper.Value, value);
                            switch (compareResult)
                            {
                                case HierarchicalResult.Current:
                                    processors.Process(compareResult, parentViewModel, current);
                                    return currentSelectionHelper;

                                case HierarchicalResult.Child:
                                    if (processors.Process(compareResult, parentViewModel, current))
                                        return await Lookup(value, current, compareFunc, processors);
                                    break;
                            }

                        }
            }
            return null;
        }
    }

    public class RecrusiveBroadcastIfLoaded<VM, T> : ITreeSelectionLookup<VM, T>
    {
        public static RecrusiveBroadcastIfLoaded<VM, T> Instance = new RecrusiveBroadcastIfLoaded<VM, T>();

        public async Task<ITreeNodeSelectionHelper<VM, T>> Lookup(T value, VM parentViewModel,
          Func<T, T, HierarchicalResult> compareFunc, params ITreeSelectionProcessor<VM, T>[] processors)
        {
            if (parentViewModel is ISupportSubEntriesHelper<VM>)
            {
                var entries = (parentViewModel as ISupportSubEntriesHelper<VM>).Entries;
                if (entries.IsLoaded)
                    foreach (VM current in entries.AllNonBindable)
                        if (current is ISupportSelectionHelper<VM, T> && current is ISupportSubEntriesHelper<VM>)
                        {
                            var currentSelectionHelper = (current as ISupportSelectionHelper<VM, T>).Selection;
                            var compareResult = compareFunc(currentSelectionHelper.Value, value);
                            if (processors.Process(compareResult, parentViewModel, current))
                                return await Lookup(value, current, compareFunc, processors);
                            break;
                        }
            }
            return null;
        }
    }
}