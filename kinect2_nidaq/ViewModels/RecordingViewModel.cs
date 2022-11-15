using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels
{
    public class RecordingViewModel : ObservableObject
    {
        protected string _sessionName;
        protected string _subjectName;
        protected string _saveDirectory;

        protected bool _isIndeterminateRecording;
        protected double _recordingDuration;

        protected bool _isPreviewMode;
        protected bool _compressSession;

        public RecordingViewModel()
        {
            this._sessionName = Properties.Settings.Default.SessionName;
            this._subjectName = Properties.Settings.Default.SubjectName;
            this._saveDirectory = Properties.Settings.Default.FolderName;
        }

        public string SessionName
        {
            get => this._sessionName;
            set => this.SetField(ref this._sessionName, value, () => Properties.Settings.Default.SessionName = value);
        }
        public string SubjectName
        {
            get => this._subjectName;
            set => this.SetField(ref this._subjectName, value, () => Properties.Settings.Default.SubjectName = value);
        }
        public string FolderName
        {
            get => this._saveDirectory;
            set => this.SetField(ref this._saveDirectory, value, () => Properties.Settings.Default.FolderName = value);
        }
        public bool IsIndeterminateRecording
        {
            get => this._isIndeterminateRecording;
            set => this.SetField(ref this._isIndeterminateRecording, value);
        }
        public double RecordingDuration
        {
            get => this._recordingDuration;
            set => this.SetField(ref this._recordingDuration, value);
        }
        public bool IsPreviewMode
        {
            get => this._isPreviewMode;
            set => this.SetField(ref this._isPreviewMode, value);
        }
        public bool CompressSession
        {
            get => this._compressSession;
            set => this.SetField(ref this._compressSession, value);
        }


    }
}
