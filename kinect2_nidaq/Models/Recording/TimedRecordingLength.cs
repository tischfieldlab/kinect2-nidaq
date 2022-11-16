using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace kinect2_nidaq.Models.Recording
{
    class TimedRecordingLength : IRecordingLengthStrategy, INotifyPropertyChanged
    {
        protected Timer timer;
        protected TimeSpan targetLength;
        protected DateTime startTime;
        protected DateTime endTime;
        protected bool hasFired;

        public event EventHandler TriggerStop;
        public event PropertyChangedEventHandler PropertyChanged;

        public TimedRecordingLength(TimeSpan recordingLength)
        {
            this.hasFired = false;
            this.targetLength = recordingLength;
            this.timer = new Timer()
            {
                Interval = 10, //10 milliseconds
                AutoReset = true
            };
            this.timer.Elapsed += this.Check_condition;
        }
        public void Start()
        {
            this.startTime = DateTime.UtcNow;
            this.endTime = this.startTime.Add(this.targetLength);
            this.timer.Enabled = true;
        }
        public void Stop()
        {
            this.timer.Enabled = false;
        }

        private void Check_condition(object sender, ElapsedEventArgs e)
        {
            if (this.endTime <= DateTime.UtcNow && !this.hasFired)
            {
                this.hasFired = true; //prevent multiple firings!
                this.TriggerStop?.Invoke(this, e);
            }
            this.NotifyPropertyChanged(null);
        }
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TimeSpan Duration { get => DateTime.UtcNow - this.startTime; }

        public double? Progress { get => this.Duration.TotalSeconds / this.targetLength.TotalSeconds; }

        public TimeSpan? TimeRemaining { get => this.targetLength - this.Duration; }
    }
}
