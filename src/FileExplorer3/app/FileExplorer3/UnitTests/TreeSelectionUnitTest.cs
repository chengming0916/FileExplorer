using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.ViewModels.Helpers;
using Moq;
using NUnit.Framework;

//namespace FileExplorer.UnitTests
//{
    //internal class TreeVM : TreeNodeVM
    //{        
    //}

    //internal class TreeNodeVM : ISupportNodeSelectionHelper<TreeNodeVM, int>
    //{        
    //    public string Value { get; set; }
    //    public ITreeNodeSelectionHelper<TreeNodeVM, int> Selection { get; set; }
    //    public ISubEntriesHelper<TreeNodeVM> Entries { get; set; }

    //    public static IEnumerable<TreeNodeVM> CreateNodes(int level, int start, int count)
    //    {
    //        for (int i = start; i < start + count; i++)
    //            yield return new TreeNodeVM() { Level = level, Value = i };
    //    }

    //    public static Task<IEnumerable<TreeNodeVM>> CreateNodesAsync(int level, int start, int count)
    //    {
    //        return Task<IEnumerable<TreeNodeVM>>.FromResult(CreateNodes(level, start, count));
    //    }
    //}


    //[TestFixture]
    //public class TreeSelectionUnitTest
    //{
    //    //static 
    //    //static ITreeNodeSelectionHelper<string, string>

    //    static Mock<ISupportNodeSelectionHelper<TreeNodeVM, int>> _rootvm;

    //    [Test]
    //    public static async void TestEntryHelper()
    //    {
    //        ISubEntriesHelper<TreeNodeVM> entryHelper = 
    //            new SubEntriesHelper<TreeNodeVM>(() => TreeNodeVM.CreateNodesAsync(0, 0, 10));

    //        bool preIsLoaded = entryHelper.IsLoaded;
    //        int preLoadCount = entryHelper.All.Count();

    //        await entryHelper.LoadAsync();

    //        bool postIsLoaded = entryHelper.IsLoaded;
    //        int postLoadCount = entryHelper.All.Count();

    //        Assert.IsFalse(preIsLoaded);
    //        Assert.AreEqual(1, preLoadCount); //DummyNode
    //        Assert.IsTrue(postIsLoaded);
    //        Assert.AreEqual(10, postLoadCount);
    //    }

    //    public static async void TestSelectoionHelper()
    //    {
    //         ISubEntriesHelper<TreeNodeVM> entryHelper = new SubEntriesHelper<TreeNodeVM>(() => TreeNodeVM.CreateNodesAsync(0, 0, 10));

    //         ITreeNodeSelectionHelper<TreeNodeVM, int> selHelper =
    //             new TreeSelectionHelper<TreeNodeVM, int>(entryHelper, (first, second) => ;

    //        selHelper.
    //    }


        //static Mock<IEntryViewModel> _parentvm, _child1vm, _child2vm;
        //static List<IEntryModel> _entryList;
        //static List<IEntryViewModel> _entryVMList;

        //static TreeNodeVM setupSubVM(int level, int item, int itemPerLevel)
        //{
        //    //var entryHelperVM = new Mock<ISubEntriesHelper<TreeNodeVM>>();
            
        //    //List<TreeNodeVM> retVal = new List<TreeNodeVM>();
        //    //if (level > 0)
        //    //    for (int i = 0; i < itemPerLevel; i++)
        //    //        retVal.Add(setupSubVM(level - 1, i, itemPerLevel));            
        //    //entryHelperVM.Setup(e => e.LoadAsync(It.IsAny<bool>()))
        //    //        .ReturnsAsync(retVal);
        //    //else
        //    //    entryHelperVM.Setup(e => e.LoadAsync(It.IsAny<bool>()))
        //    //        .ReturnsAsync(setupSubVM(level-1, itemPerLevel)

        //    //var nodeVM = new Mock<ISupportNodeSelectionHelper<TreeNodeVM, int>>();
        //    //nodeVM.Setup(r => r.Entries).Returns(() => 
        //}

        //static TreeVM setupViewModel()
        //{
        //    var rootVM = new Mock<ISupportNodeSelectionHelper<TreeNodeVM, int>>();
        //    rootVM.Setup(r => r.Entries).Returns

        //    return rootVM;
        //    //entryModel.Setup(em => em.Label).Returns(label);
        //    //entryModel.Setup(em => em.IsDirectory).Returns(isDirectory);
        //    //entryModel.Setup(em => em.Profile).Returns(profile);
        //    //var retVal = new Mock<IEntryViewModel>();
        //    //retVal.Setup(evm => evm.EntryModel).Returns(entryModel.Object);
            
        //    //return retVal;
        //}
//    }
//}
