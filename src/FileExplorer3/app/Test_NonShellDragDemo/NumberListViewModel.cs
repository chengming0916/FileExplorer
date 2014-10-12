using FileExplorer.UIEventHub;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Test_NonShellDragDemo
{
    public class NumberListViewModel : NotifyPropertyChanged, ISupportDragHelper, ISupportDropHelper
    {
        public static NumberListViewModel GenerateRange(string displayName, int start, int end)
        {
            NumberListViewModel nvm = new NumberListViewModel();
            for (int i = start; i <= end; i++)
                nvm.Items.Add(new NumberViewModel(i));
            nvm.DropHelper.DisplayName = displayName;            
            return nvm;
        }

        public NumberListViewModel()
        {
            Items = new ObservableCollection<NumberViewModel>();

            var converter = new LambdaValueConverter<IDraggable, NumberModel>(
                d => (d is NumberViewModel) ? (d as NumberViewModel).Model : null,
                nm => new NumberViewModel(nm));

            DragHelper = new LambdaDragHelper<NumberModel>(converter,
                () => Items.Where(nvm => nvm.IsSelected).Select(nvm => nvm.Model).ToList(),
                nms => DragDropEffects.Move,
                (nms, eff) =>
                {
                    foreach (var nm in nms)
                        Items.Remove(new NumberViewModel(nm));
                }
                );
            DropHelper = new LambdaDropHelper<NumberModel>(converter,
                (nms, eff) =>
                {
                    foreach (var nm in nms)
                        if (Items.Any(ivm => ivm.Model.Equals(nm)))
                            return QueryDropEffects.None;
                    return QueryDropEffects.CreateNew(DragDropEffects.Move);
                },
                (nms, eff) =>
                {
                    foreach (var nm in nms)
                        Items.Add(new NumberViewModel(nm));
                    return DragDropEffects.Move;
                }) { DisplayName = "Unspecified" };
        }

        public ObservableCollection<NumberViewModel> Items { get; set; }
        public ISupportDrag DragHelper { get; set; }
        public ISupportDrop DropHelper { get; set; }
    }
}
