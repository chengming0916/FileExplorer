using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DirectoryCommandModel(IScriptCommand command, Func<IEnumerable<ICommandModel>> getCommandFunc)
            : base(command)
        {
            _getCommandFunc = getCommandFunc;
        }

        public DirectoryCommandModel(IScriptCommand command, params ICommandModel[] commandModels)
            : this(command, () => commandModels)
        {
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private Func<IEnumerable<ICommandModel>> _getCommandFunc;

        #endregion

        #region Public Properties

        public IEnumerable<ICommandModel> SubCommands
        {
            get { return _getCommandFunc(); }
        }

        #endregion
    }

    public class SliderCommandModel : DirectoryCommandModel, ISliderCommandModel
    {

        #region Constructor

        public SliderCommandModel(IScriptCommand command, params ICommandModel[] commandModels)
            : base(command, commandModels)
        {
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

        #endregion

        #region Public Properties

        public int SliderStep { get { return _sliderStep; } set { _sliderStep = value; NotifyOfPropertyChange(() => SliderStep); } }
        
        public double? ItemHeight
        {
            get { return _itemHeight; }
            set { _itemHeight = value; NotifyOfPropertyChange(() => ItemHeight); }
        }

        #endregion
    }
}
