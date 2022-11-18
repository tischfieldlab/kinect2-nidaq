using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kinect2_nidaq.ViewModels.Commands
{
    public class StartRecordingCommand : BaseCommand
    {
        public StartRecordingCommand(MainWindowViewModel ViewModel) : base(ViewModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            var cfg = this.ViewModel.Settings;

            if (this.ViewModel.Recording.IsRecording)
                return false;

            if (cfg.IsPreviewMode)
            {
                return true;
            }
            else
            {
                if (!(cfg.IsColorStreamEnabled || cfg.IsDepthStreamEnabled || cfg.AnalogNIDAQ.IsEnabled))
                    // At least one stream should be enabled!
                    return false;

                return true;
            }
        }

        public override void Execute(object parameter)
        {
            this.ViewModel.Recording.StartRecording();
        }
    }
}
