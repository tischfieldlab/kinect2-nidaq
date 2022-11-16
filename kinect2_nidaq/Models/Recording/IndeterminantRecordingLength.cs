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
    class IndeterminantRecordingLength : IRecordingLengthStrategy, INotifyPropertyChanged
    {
        protected Timer timer;
        protected DateTime startTime;
        public event PropertyChangedEventHandler PropertyChanged;


        public event EventHandler TriggerStop
        {
            //do this to suppress the "event never used" warning
            //in this implementation, the TriggerStop event is NEVER triggered!
            add { }
            remove { }
        }

        public IndeterminantRecordingLength()
        {
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
            this.timer.Enabled = true;
        }
        public void Stop()
        {
            this.timer.Enabled = false;
        }

        private void Check_condition(object sender, ElapsedEventArgs e)
        {
            this.NotifyPropertyChanged(null);
        }
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TimeSpan Duration { get => DateTime.UtcNow - this.startTime; }

        public double? Progress { get => null; }

        public TimeSpan? TimeRemaining { get => null; }
    }
}
