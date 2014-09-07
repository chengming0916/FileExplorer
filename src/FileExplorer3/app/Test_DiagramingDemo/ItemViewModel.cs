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
    public class ItemViewModel : NotifyPropertyChanged, IDraggablePositionAware, ISelectable
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
        private bool _isDragging;
        private bool _isVisible;
        private Point _position = new Point(rand.Next(500), rand.Next(500));
        private bool _isSelected;
        private bool _isSelecting;
        #endregion

        #region Public Properties
        public bool IsVisible 
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyOfPropertyChanged(() => IsVisible);
            }
        }

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
