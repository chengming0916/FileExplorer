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
using System.Windows.Data;

namespace Test_ShellDragDemo
{
    public class FileListViewModel : NotifyPropertyChanged,
        ISupportDropHelper, ISupportDragHelper,
        /* ISupportShellDrag, ISupportShellDrop, */IContainer<ISelectable>
    {
        private ObservableCollection<FileViewModel> _items = new ObservableCollection<FileViewModel>();
        private bool _isDraggingOver, _isDraggingFrom;
        private ISupportDrop _dropHelper;

        public ObservableCollection<FileViewModel> Items { get { return _items; } }
        public ISupportDrag DragHelper { get; set; }
        public ISupportDrop DropHelper
        {
            get { return _dropHelper; }
            set { _dropHelper = value; }
        }
        public dynamic Commands { get; private set; }

        public FileListViewModel(string label)
        {
            DropTargetLabel = label;
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

            IValueConverter dataObjectConverter = new LambdaValueConverter<IEnumerable<FileModel>, IDataObject>(fmList =>
                        {
                            var da = new DataObject();
                            var fList = new StringCollection();
                            foreach (var fm in fmList)
                            {
                                if (!File.Exists(fm.FileName))
                                    using (var sw = File.CreateText(fm.FileName))
                                        sw.WriteLine(fm.FileName);
                                fList.Add(fm.FileName);
                            }
                            da.SetFileDropList(fList);
                            return da;
                        },
                        da =>
                        {
                            List<FileModel> retVal = new List<FileModel>();
                            if ((da as DataObject).ContainsFileDropList())
                                foreach (var file in (da as DataObject).GetFileDropList())
                                    retVal.Add(new FileModel(file));
                            return retVal;
                        }
                        );

            DropHelper = new FileModelDropHelper(this, label);
            DragHelper = new LambdaShellDragHelper<FileModel>(
                new LambdaValueConverter<FileViewModel, FileModel>(fvm => fvm.Model, fm => new FileViewModel(fm)),
                dataObjectConverter,
                () => Items.Where(fvm => fvm.IsSelected).Select(fvm => fvm.Model),
                (fmList) => DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move,
                (fmList, da, effect) =>
                {
                    if (effect == DragDropEffects.Move)
                        foreach (var f in fmList)
                        {
                            var foundItem = Items.FirstOrDefault(fvm => fvm.Model.Equals(f));
                            if (foundItem != null)
                                Items.Remove(foundItem);
                        }
                });


            //DropHelper = new ShellDropHelper<FileViewModel>(() => DropTargetLabel,
            //    (fvms, eff) =>
            //    {
            //        if (fvms.Any(f => Items.Contains(f)))
            //            return QueryDropEffects.None;
            //        return QueryDropEffects.CreateNew(eff & (DragDropEffects.Link | DragDropEffects.Move),
            //            eff & DragDropEffects.Move);
            //    },

            //     (fvms, da, eff) =>
            //     {
            //         foreach (var existingFvm in Items) existingFvm.IsSelected = false;
            //         foreach (var fvm in fvms) this.Items.Add(fvm);
            //         if (eff.HasFlag(DragDropEffects.Move))
            //             return DragDropEffects.Move;
            //         if (eff.HasFlag(DragDropEffects.Copy))
            //             return DragDropEffects.Copy;
            //         return DragDropEffects.Link;
            //     },

            //    (da) =>
            //       {
            //           List<FileViewModel> retVal = new List<FileViewModel>();
            //           if ((da as DataObject).ContainsFileDropList())
            //               foreach (var file in (da as DataObject).GetFileDropList())
            //                   retVal.Add(new FileViewModel(file));
            //           return retVal;
            //       }
            //     );
        }


        public void UnselectAll()
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }


        public IDataObject GetDataObject(IEnumerable<IDraggable> draggables)
        {
            var da = new DataObject();
            var fList = new StringCollection();
            foreach (var d in draggables.Cast<FileViewModel>())
            {
                if (!File.Exists(d.Model.FileName))
                    using (var sw = File.CreateText(d.Model.FileName))
                        sw.WriteLine(d.DisplayName);
                fList.Add(d.Model.FileName);
            }
            da.SetFileDropList(fList);
            return da;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, IDataObject da, System.Windows.DragDropEffects effect)
        {
            OnDragCompleted(draggables, effect);
        }

        public bool HasDraggables
        {
            get { return Items.Any(fvm => fvm.IsSelected); }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return Items.Where(fvm => fvm.IsSelected).Select(fvm => fvm).ToArray();
        }

        public DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move;
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, System.Windows.DragDropEffects effect)
        {
            if (effect == DragDropEffects.Move)
                foreach (var f in draggables.Cast<FileViewModel>())
                    if (Items.Contains(f))
                        Items.Remove(f);
        }

        IEnumerable<ISelectable> IContainer<ISelectable>.GetChildItems()
        {
            return Items;
        }

        //#region ISupportShellDrop

        //public IEnumerable<IDraggable> QueryDropDraggables(IDataObject dataObj)
        //{
        //    List<IDraggable> retVal = new List<IDraggable>();
        //    DataObject da = dataObj as DataObject;
        //    if (da.ContainsFileDropList())
        //    {
        //        foreach (var file in da.GetFileDropList())                
        //            retVal.Add(new FileViewModel(file));                                                            
        //    }
        //    return retVal;
        //}

        //public DragDropEffects Drop(IEnumerable<IDraggable> draggables, IDataObject da, DragDropEffects allowedEffects)
        //{
        //    foreach (var fvm in Items)
        //        fvm.IsSelected = false;
        //    foreach (var fvm in draggables.Cast<FileViewModel>())
        //        this.Items.Add(fvm);
        //    if (allowedEffects.HasFlag(DragDropEffects.Move))
        //        return DragDropEffects.Move;
        //    if (allowedEffects.HasFlag(DragDropEffects.Copy))
        //        return DragDropEffects.Copy;
        //    return DragDropEffects.Link;
        //}

        public bool IsDraggingFrom
        {
            get { return _isDraggingFrom; }
            set { _isDraggingFrom = value; NotifyOfPropertyChanged(() => IsDraggingFrom); }
        }

        //public bool IsDraggingOver
        //{
        //    get { return _isDraggingOver; }
        //    set { _isDraggingOver = value; NotifyOfPropertyChanged(() => IsDraggingOver); }
        //}

        //public bool IsDroppable
        //{
        //    get { return true; ; }
        //}

        public string DropTargetLabel
        {
            get;
            set;
        }

        //public QueryDropEffects QueryDrop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        //{
        //    if (draggables.Any(d => Items.Contains(d)))
        //        return QueryDropEffects.None;

        //    return QueryDropEffects.CreateNew(allowedEffects & (DragDropEffects.Link | DragDropEffects.Move), 
        //        allowedEffects & DragDropEffects.Move);
        //}

        //public DragDropEffects Drop(IEnumerable<IDraggable> draggables, DragDropEffects allowedEffects)
        //{
        //    if (draggables.Any(d => Items.Contains(d)))
        //        return DragDropEffects.None;

        //    return Drop(draggables, null, allowedEffects);
        //}
        //#endregion
    }
}
