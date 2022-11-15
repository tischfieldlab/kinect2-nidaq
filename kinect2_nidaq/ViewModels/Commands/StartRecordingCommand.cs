using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kinect2_nidaq.ViewModels.Commands
{
    public class StartRecordingCommand : ICommand
    {
        protected MainWindowViewModel viewModel;
        public StartRecordingCommand(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //this.viewModel.Recording.
        }
    }
}
