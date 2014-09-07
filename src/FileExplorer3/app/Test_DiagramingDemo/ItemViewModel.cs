using FileExplorer.UIEventHub;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiagramingDemo
{
    public class ItemViewModel : NotifyPropertyChanged, IPositionAware, IDraggable, ISelectable, IResizable
    {

        #region Constructor

        public ItemViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private static Random rand = new Random();
        private bool _isDragging = false;
        private Point _position = new Point(rand.Next(500), rand.Next(500));
        private Size _size = new Size(rand.Next(25) + 25, rand.Next(25) + 25);
        private bool _isSelected;
        private bool _isSelecting;
        #endregion

        #region Public Properties
      

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChanged(() => IsSelected);
            }
        }

        public bool IsSelecting
        {
            get
            {
                return _isSelecting;
            }
            set
            {
                _isSelecting = value;
                NotifyOfPropertyChanged(() => IsSelecting);
            }
        }

        public Point Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                NotifyOfPropertyChanged(() => Position);
            }
        }

        public Size Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                NotifyOfPropertyChanged(() => Size);
            }
        }

        public string DisplayName
        {
            get;
            private set;
        }

        public bool IsDragging
        {
            get
            {
                return _isDragging;
            }
            set
            {
                _isDragging = value;
                NotifyOfPropertyChanged(() => IsDragging);
            }
        }
        #endregion
    }
}
