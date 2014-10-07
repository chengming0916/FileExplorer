using FileExplorer.UIEventHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Test_ShellDragDemo
{
    public class FileModelDropHelper : ShellDropHelper<FileModel>
    {
        private FileListViewModel _flvm;
        public FileModelDropHelper(FileListViewModel flvm, string dropTargetLabel)
            : base(new LambdaValueConverter<FileViewModel, FileModel>(fvm => fvm.Model, fm => new FileViewModel(fm)))
        {
            DropTargetLabel = dropTargetLabel;
            _flvm = flvm;
        }

        public override QueryDropEffects QueryDrop(IEnumerable<FileModel> fms, System.Windows.DragDropEffects eff)
        {
           IEnumerable<FileModel> itemsModel =  _flvm.Items.Select(fvm => fvm.Model);
           if (fms.Any(f => itemsModel.Contains(f)))
               return QueryDropEffects.None;
           return QueryDropEffects.CreateNew(eff & (DragDropEffects.Link | DragDropEffects.Move),
               eff & DragDropEffects.Move);
        }


        public override DragDropEffects Drop(IEnumerable<FileModel> fms, DragDropEffects eff)
        {
            foreach (var existingFvm in _flvm.Items) existingFvm.IsSelected = false;
            foreach (var fm in fms) _flvm.Items.Add(ConvertBack(fm) as FileViewModel);
            if (eff.HasFlag(DragDropEffects.Move))
                return DragDropEffects.Move;
            if (eff.HasFlag(DragDropEffects.Copy))
                return DragDropEffects.Copy;
            return DragDropEffects.Link;
        }

        public override IEnumerable<FileModel> QueryDropModels(IDataObject da)
        {
            List<FileModel> retVal = new List<FileModel>();
            if ((da as DataObject).ContainsFileDropList())
                foreach (var file in (da as DataObject).GetFileDropList())
                    retVal.Add(new FileModel(file));
            return retVal;
        }

    }
}
