using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels;
using Moq;
using NUnit.Framework;

namespace FileExplorer.UnitTests
{
    [TestFixture]
    public class ActionTest
    {

        //public class TestEntryModel : EntryModelBase 
        //{
        //    public TestEntryModel(bool isDirectory, string label)
        //    {                
        //        this.IsDirectory = isDirectory;
        //        this.Label = label;
        //    }
        //}

        static Mock<IProfile> _profilevm;
        static Mock<IEntryViewModel> _parentvm, _child1vm, _child2vm;
        static List<IEntryModel> _entryList;

        static Mock<IEntryViewModel> setupViewModel(IProfile profile, string label, bool isDirectory)
        {
            var entryModel = new Mock<IEntryModel>();
            
            entryModel.Setup(em => em.Label).Returns(label);
            entryModel.Setup(em => em.IsDirectory).Returns(isDirectory);
            var retVal = new Mock<IEntryViewModel>();
            retVal.Setup(evm => evm.EntryModel).Returns(entryModel.Object);
            retVal.Setup(evm => evm.Profile).Returns(profile);
            return retVal;
        }

        static void setup()
        {
            _profilevm = new Mock<IProfile>();
            
            _parentvm = setupViewModel(_profilevm.Object, "parent", true);
            _child1vm = setupViewModel(_profilevm.Object, "child1", false);
            _child2vm = setupViewModel(_profilevm.Object, "child2", false);

            _entryList = new List<IEntryModel>() { _child1vm.Object.EntryModel, _child2vm.Object.EntryModel };
            _profilevm.Setup(foo => foo.ListAsync(It.IsAny<IEntryModel>(), null)).ReturnsAsync(_entryList);
        }

      

        [Test]
        public static void Test_LoadEntryList()
        {
            //Setup
            setup();
            var loadEL = new LoadEntryList(_parentvm.Object, null);
            var context = new ActionExecutionContext();
           
            //Action
            ResultCompletionEventArgs args = loadEL.ExecuteAndWait(context);

            //Assert
            Assert.IsNull(args.Error);
            Assert.IsNotNull(context["EntryList"]);
            Assert.IsInstanceOf(typeof(IEnumerable<IEntryModel>), context["EntryList"]);
            Assert.AreEqual(2, (context["EntryList"] as IEnumerable<IEntryModel>).Count());
        }

        [Test]
        public static void Test_AppendEntryList()
        {
            //Setup
            setup();
            List<IEntryViewModel> _addedList = new List<IEntryViewModel>();
            var _entryListvm = new Mock<IEntryListViewModel>();
            _entryListvm.Setup(elvm => elvm.Items.Add(It.IsAny<IEntryViewModel>()))
                .Callback((IEntryViewModel elvm) => { _addedList.Add(elvm); });

            var context = new ActionExecutionContext();
            context["EntryList"] = _entryList;
            var appendEL = new AppendEntryList(_parentvm.Object, _entryListvm.Object);

            //Action
            ResultCompletionEventArgs args = appendEL.ExecuteAndWait(context);

            //Assert
            Assert.IsNull(args.Error);
            Assert.IsTrue(_addedList.Count == 2);
        }

    }
}
