using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kinect2_nidaq.ViewModels.Commands
{
    public class StopRecordingCommand : BaseCommand
    {
        public StopRecordingCommand(MainWindowViewModel ViewModel) : base(ViewModel)
        {
            this.ViewModel.Recording.PropertyChanged += Recording_PropertyChanged;
        }

        private void Recording_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null || e.PropertyName.Equals(nameof(this.ViewModel.Recording.IsRecording)))
            {
                this.RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.ViewModel.Recording.IsRecording;
        }

        public override void Execute(object parameter)
        {
            this.ViewModel.Recording.StopRecording();
        }
    }
}
