using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models.Recording
{
    interface IRecordingLengthStrategy : INotifyPropertyChanged
    {
        event EventHandler TriggerStop;

        TimeSpan Duration { get; }
        double? Progress { get; }
        TimeSpan? TimeRemaining { get; }

        void Start();
        void Stop();
    }
}
