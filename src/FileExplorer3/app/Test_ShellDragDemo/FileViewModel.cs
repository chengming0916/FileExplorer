using FileExplorer.UIEventHub;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_ShellDragDemo
{
    public class FileViewModel : NotifyPropertyChanged, IDraggable, ISelectable
    {
        public FileViewModel(string fileName)
        {
            DisplayName = fileName;
        }


        public string DisplayName { get; set; }
        private bool _isDragging = false;
        public bool IsDragging
        {
            get { return _isDragging; }
            set { _isDragging = value; NotifyOfPropertyChanged(() => IsDragging); }
        }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChanged(() => IsSelected); }
        }

        private bool _isSelecting = false;
        public bool IsSelecting
        {
            get { return _isSelecting; }
            set { _isSelecting = value; NotifyOfPropertyChanged(() => IsSelecting); }
        }
    }
}
