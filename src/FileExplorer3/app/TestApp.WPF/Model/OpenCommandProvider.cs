using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Tools;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;

namespace FileExplorer.Models
{
    public class OpenCommandProviderEx : ICommandProvider
    {
        FileSystemInfoExProfile _profile;
        public OpenCommandProviderEx(FileSystemInfoExProfile profile)
        {
            _profile = profile;
        }

        public List<ICommandModel> CommandModels
        {
            get
            {
                return new List<ICommandModel>()
                    {
                        new OpenWithCommandEx(_profile)
                    };
            }
        }
    }


    public class OpenWithScriptCommand : ScriptCommandBase
    {
        //string _appliedFileName;
        //bool _isFolder;
        OpenWithInfo _info;

        public OpenWithScriptCommand(/*string appliedFileName, bool isFolder, */OpenWithInfo info = null)
            : base("OpenWith")
        {
            //_appliedFileName = appliedFileName;
            //_isFolder = isFolder;
            _info = info;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return base.CanExecute(pm);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var parameter = pm["Parameter"] as IEntryModel[];
            if (parameter != null && parameter.Count() == 1)
            {
                bool _isFolder = parameter[0].IsDirectory;
                string _appliedFileName = parameter[0].FullPath;

                if (_isFolder)
                {
                    if (Directory.Exists(_appliedFileName))
                        try { Process.Start(_appliedFileName); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }
                }
                else
                {

                    ProcessStartInfo psi = null;
                    if (File.Exists(_appliedFileName))
                        if (_info != null)
                        {
                            if (!_info.Equals(OpenWithInfo.OpenAs))
                                psi = new ProcessStartInfo(OpenWithInfo.GetExecutablePath(_info.OpenCommand), _appliedFileName);
                            else
                            {
                                //http://bytes.com/topic/c-sharp/answers/826842-call-windows-open-dialog
                                psi = new ProcessStartInfo("Rundll32.exe");
                                psi.Arguments = String.Format(" shell32.dll, OpenAs_RunDLL {0}", _appliedFileName);
                            }
                        }
                        else psi = new ProcessStartInfo(_appliedFileName);

                    if (psi != null)
                        try { Process.Start(psi); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }

                }

                return ResultCommand.OK;
            }
            else return ResultCommand.Error(new Exception("Wrong Parameter type or more than one item."));
        }
    }

    public class OpenWithCommandEx : DirectoryCommandModel
    {
        FileSystemInfoExProfile _profile;
        //FileSystemInfoExModel _appliedModel;        

        public OpenWithCommandEx(FileSystemInfoExProfile profile)
            : base(null)
        {
            _profile = profile;
            Header = "Open";
            //HeaderIcon = System.Drawing.Icon.ExtractAssociatedIcon(

            //Symbol = Convert.ToChar(0xE188);
            Command = new OpenWithScriptCommand();
        }

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            base.NotifySelectionChanged(appliedModels);

            List<ICommandModel> subCommands = new List<ICommandModel>();

            IsEnabled = appliedModels.Count() == 1;
            if (IsEnabled)
            {
                var appliedModel = appliedModels[0];
                HeaderImageFunc = (cm) =>
                    AsyncUtils.RunSync(() => GetFromSystemImageList.Instance.GetIconForModelAsync(appliedModel));

                if (appliedModel is FileSystemInfoExModel)
                    subCommands.AddRange(GetCommands(appliedModel as FileSystemInfoExModel));
            }

            SubCommands = subCommands;
        }

        //public override System.Windows.Media.ImageSource GetHeaderImage()
        //{
        //    return _profile.GetIconExtractSequence(_appliedModel).Last().GetIconForModel(_appliedModel).Result;
        //}

        public IEnumerable<ICommandModel> GetCommands(FileSystemInfoExModel appliedModel)
        {

            if (!appliedModel.IsDirectory)
            {
                string ext = PathEx.GetExtension(appliedModel.Name);
                foreach (OpenWithInfo info in FileTypeInfoProvider.GetFileTypeInfo(ext).OpenWithList)
                    if (info.OpenCommand != null)
                    {
                        string executePath = OpenWithInfo.GetExecutablePath(info.OpenCommand);
                        string exeName = Path.GetFileNameWithoutExtension(executePath);

                        if (info.OpenCommand != null && File.Exists(executePath))
                        {
                            IEntryModel exeModel = AsyncUtils.RunSync(() => _profile.ParseAsync(executePath));
                            if (exeModel != null)
                                yield return new CommandModel(new OpenWithScriptCommand(info))
                                    {
                                        Header = String.Format("{0} ({1})", exeName, info.KeyName),
                                        ToolTip = info.Description,
                                        HeaderImageFunc = (cm) => AsyncUtils.RunSync(() =>
                                            _profile.GetIconExtractSequence(exeModel).Last().GetIconForModelAsync(exeModel)),
                                        IsEnabled = true
                                    };

                        }
                    }

                yield return new CommandModel(new OpenWithScriptCommand(OpenWithInfo.OpenAs))
                {
                    Header = "Open with...",
                    IsEnabled = true
                };

            }
        }
    }
}
