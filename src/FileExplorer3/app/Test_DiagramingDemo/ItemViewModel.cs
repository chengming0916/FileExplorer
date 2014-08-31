using FileExplorer.UIEventHub;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int _left = rand.Next(500);
        private bool _isSelected;
        private int _top = rand.Next(500);
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

        public int Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
                NotifyOfPropertyChanged(() => Left);
            }
        }

        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
                NotifyOfPropertyChanged(() => Top);
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
