using kinect2_nidaq.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models.Recording
{
    public enum RecordingMode
    {
        Indeterminate,
        Timed
    }
    class Recorder : ObservableObject
    {
        protected RecordingMode _mode;
        protected TimeSpan _recordingLength;
        protected IRecordingLengthStrategy _terminator;
        protected List<IDataWriter> _writers;
        protected List<IDeviceViewModel> _devices;
        protected List<Action> _afterCompleteTasks;

        public Recorder(RecordingMode mode, TimeSpan duration)
        {
            this._mode = mode;
            this._recordingLength = duration;

            this._writers = new List<IDataWriter>();
            this._devices = new List<IDeviceViewModel>();
            this._afterCompleteTasks = new List<Action>();
        }

        public void AddWriter(IDataWriter writer)
        {
            this._writers.Add(writer);
        }
        public void AddDevice(IDeviceViewModel device)
        {
            this._devices.Add(device);
        }
        public void AddPostRecordTask(Action task)
        {
            this._afterCompleteTasks.Add(task);
        }

        public void Start()
        {
            this._writers.ForEach((w) => w.Start());
            this._devices.ForEach((d) => d.Start());
            this.TerminatorFactory().Start();
        }

        public void Stop()
        {
            this._terminator.Stop();
            this.DisposeTerminator();

            this._devices.ForEach((d) => d.Stop());
            this._writers.ForEach((w) => w.Stop());

            Task lastTask = Task.Factory.StartNew(() => { /* empty task */});
            foreach (var task in this._afterCompleteTasks)
            {
                lastTask = lastTask.ContinueWith(antecedent => task(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            lastTask.Wait();
        }


        protected IRecordingLengthStrategy TerminatorFactory()
        {
            switch (this._mode)
            {
                case RecordingMode.Timed:
                    this._terminator = new TimedRecordingLength(this._recordingLength);
                    break;
                case RecordingMode.Indeterminate:
                default:
                    this._terminator = new IndeterminantRecordingLength();
                    break;
            }
            this._terminator.TriggerStop += this.Terminator_TriggerStop;
            this._terminator.PropertyChanged += this.Terminator_PropertyChanged;
            return this._terminator;
        }

        protected void DisposeTerminator()
        {
            if (this._terminator == null)
            {
                throw new InvalidOperationException("Cannot dispose terminator, is already null!");
            }
            this._terminator.TriggerStop -= this.Terminator_TriggerStop;
            this._terminator.PropertyChanged -= this.Terminator_PropertyChanged;
            this._terminator = null;
        }


        private void Terminator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged(null);
        }

        private void Terminator_TriggerStop(object sender, EventArgs e)
        {
            this.Stop();
        }
    }
}
