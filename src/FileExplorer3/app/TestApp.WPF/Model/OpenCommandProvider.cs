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
        public Task<IEnumerable<ICommandModel>> GetCommandsAsync(IEntryModel[] appliedModels)
        {
            return Task.Run(() =>
                {
                    List<ICommandModel> retVal = new List<ICommandModel>();

                    if (appliedModels.Count() == 1)
                    {
                        FileSystemInfoExModel model = appliedModels.First() as FileSystemInfoExModel;
                        if (model != null)
                            retVal.Add(new OpenWithCommandEx(model.Profile, model));
                    }

                    return (IEnumerable<ICommandModel>)retVal;
                });
        }
    }


    public class OpenWithScriptCommand : ScriptCommandBase
    {
        string _appliedFileName;
        bool _isFolder;
        OpenWithInfo _info;

        public OpenWithScriptCommand(string appliedFileName, bool isFolder, OpenWithInfo info)
            : base("OpenWith")
        {
            _appliedFileName = appliedFileName;
            _isFolder = isFolder;
            _info = info;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
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
    }

    public class OpenWithCommandEx : DirectoryCommandModel
    {
        IProfile _profile;
        FileSystemInfoExModel _appliedModel;

        public OpenWithCommandEx(IProfile profile, FileSystemInfoExModel appliedModel)
            : base(null)
        {
            _profile = profile;
            _appliedModel = appliedModel;

            Header = "Open";
            //HeaderIcon = System.Drawing.Icon.ExtractAssociatedIcon(
            HeaderImageFunc = (cm) => 
                
                AsyncUtils.RunSync(() => GetFromIconExtractIcon.Instance.GetIconForModelAsync(appliedModel));                                    
            //Symbol = Convert.ToChar(0xE188);
            Command = new OpenWithScriptCommand(appliedModel.FullPath, appliedModel.IsDirectory, null);
        }

        //public override System.Windows.Media.ImageSource GetHeaderImage()
        //{
        //    return _profile.GetIconExtractSequence(_appliedModel).Last().GetIconForModel(_appliedModel).Result;
        //}

        protected override IEnumerable<ICommandModel> GetCommands()
        {
            if (!_appliedModel.IsDirectory)
            {
                string ext = PathEx.GetExtension(_appliedModel.Name);
                foreach (OpenWithInfo info in FileTypeInfoProvider.GetFileTypeInfo(ext).OpenWithList)
                    if (info.OpenCommand != null)
                    {
                        string executePath = OpenWithInfo.GetExecutablePath(info.OpenCommand);
                        string exeName = Path.GetFileNameWithoutExtension(executePath);

                        if (info.OpenCommand != null && File.Exists(executePath))
                        {
                            IEntryModel exeModel = AsyncUtils.RunSync(() => _profile.ParseAsync(executePath));
                            yield return new CommandModel(new OpenWithScriptCommand(_appliedModel.FullPath, false, info))
                                {
                                    Header = String.Format("{0} ({1})", exeName, info.KeyName),
                                    ToolTip = info.Description,
                                    HeaderImageFunc = (cm) => AsyncUtils.RunSync(() =>
                                        _profile.GetIconExtractSequence(exeModel).Last().GetIconForModelAsync(exeModel))
                                };

                        }
                    }

                yield return new CommandModel(new OpenWithScriptCommand(_appliedModel.FullPath, false, OpenWithInfo.OpenAs))
                {
                    Header = "Open with..."
                };

            }
        }
    }
}
