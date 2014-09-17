using FileExplorer;
using FileExplorer.Script;
using FileExplorer.UIEventHub;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Test_ShellDragDemo
{
    public class FileListViewModel : NotifyPropertyChanged, ISupportShellDrag, IContainer<ISelectable>
    {
         private ObservableCollection<FileViewModel> _items  = new ObservableCollection<FileViewModel>();

        public ObservableCollection<FileViewModel> Items { get { return _items; } }
        public dynamic Commands { get; private set; }

        public FileListViewModel()
        {
            Commands = new DynamicRelayCommandDictionary()
            {
                ParameterDicConverter = ParameterDicConverters.FromParameterDic(
                new ParameterDic()
                {
                    { "FileListVM", this }
                })
            };

            Commands.UnselectAll = new SimpleScriptCommand("UnselectAll",
               pd =>
               {
                   pd.GetValue<FileListViewModel>("{FileListVM}").UnselectAll();
                   return ResultCommand.NoError;
               });
            
            for (int i = 1; i < 20; i++)
                Items.Add(new FileViewModel("FileVM" + i + ".txt"));            
        }


        public void UnselectAll()
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }


        public System.Windows.IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            var da = new DataObject();
            var fList = new StringCollection();
            foreach (var d in draggables.Cast<FileViewModel>())
            {
                string fName = "C:\\Temp\\" + d.DisplayName ;
                if (!File.Exists(fName))
                    using (var sw = File.CreateText(fName))
                        sw.WriteLine(d.DisplayName);
                fList.Add(fName);
            }
            da.SetFileDropList(fList);
            return da;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, System.Windows.DragDropEffects effect)
        {
            Console.WriteLine("FileListVIewModel.OnShellDragCompleted");
        }

        public bool HasDraggables
        {
            get { return Items.Any(f => f.IsSelected); }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return Items.Where(f => f.IsSelected);
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return DragDropEffects.Copy;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, System.Windows.DragDropEffects effect)
        {
            Console.WriteLine("FileListVIewModel.OnDragCompleted");
        }

        IEnumerable<ISelectable> IContainer<ISelectable>.GetChildItems()
        {
            return Items;
        }
    }
}
