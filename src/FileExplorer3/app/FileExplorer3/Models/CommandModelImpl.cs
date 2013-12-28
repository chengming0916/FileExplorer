using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core.Script;

namespace FileExplorer.Models
{
    public class SeparatorCommandModel : CommandModel, ISeparatorModel
    {
        public SeparatorCommandModel() : base(null) { }


    }

    public class DirectoryCommandModel : CommandModel, IDirectoryCommandModel
    {
        #region Constructor

        public DirectoryCommandModel(IScriptCommand command, params ICommandModel[] commandModels)
            : this(command)
        {
            SubCommands = new List<ICommandModel>(commandModels);
        }

        protected DirectoryCommandModel(IScriptCommand command)
            : base(command)
        {
            SubCommands = new List<ICommandModel>();            
        }

        #endregion

        #region Methods


        #endregion

        #region Data

        List<ICommandModel> _subCommands;

        #endregion

        #region Public Properties

        public List<ICommandModel> SubCommands { get { return _subCommands; } set { _subCommands = value; NotifyOfPropertyChange(() => SubCommands); } }

        #endregion
    }

    public class SliderCommandModel : DirectoryCommandModel, ISliderCommandModel
    {

        #region Constructor

        public SliderCommandModel(IScriptCommand command, params ICommandModel[] commandModels)
            : base(command, commandModels)
        {
            var sliderCommandModels = commandModels.Cast<SliderStepCommandModel>();
            if (sliderCommandModels.Count() > 2)
            {
                sliderCommandModels.First().VerticalAlignment = VerticalAlignment.Top;
                sliderCommandModels.Last().VerticalAlignment = VerticalAlignment.Bottom;
            }

            foreach (ISliderStepCommandModel scm in commandModels)
            {
                if (scm.SliderStep < SliderMinimum) SliderMinimum = scm.SliderStep;
                if (scm.SliderStep > SliderMaximum) SliderMaximum = scm.SliderStep;
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private int _sliderMaximum;
        private int _sliderMinimum;
        private int _sliderValue;

        #endregion

        #region Public Properties

        public int SliderMaximum { get { return _sliderMaximum; } set { _sliderMaximum = value; NotifyOfPropertyChange(() => SliderMaximum); } }
        public int SliderMinimum { get { return _sliderMinimum; } set { _sliderMinimum = value; NotifyOfPropertyChange(() => SliderMinimum); } }
        public int SliderValue { get { return _sliderValue; } set { _sliderValue = value; NotifyOfPropertyChange(() => SliderValue); Header = value.ToString(); } }

        #endregion
    }

    public class SliderStepCommandModel : CommandModel, ISliderStepCommandModel
    {

        #region Constructor

        public SliderStepCommandModel(IScriptCommand command = null)
            : base(command)
        {
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private int _sliderStep;
        private double? _itemHeight;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;

        #endregion

        #region Public Properties

        public VerticalAlignment VerticalAlignment { get { return _verticalAlignment; } set { _verticalAlignment = value; NotifyOfPropertyChange(() => VerticalAlignment); } }

        public int SliderStep { get { return _sliderStep; } set { _sliderStep = value; NotifyOfPropertyChange(() => SliderStep); } }

        public double? ItemHeight
        {
            get { return _itemHeight; }
            set { _itemHeight = value; NotifyOfPropertyChange(() => ItemHeight); }
        }

        #endregion
    }
}
