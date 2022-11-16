using kinect2_nidaq.ViewModels.AnalogDAQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            this._sessionName = Properties.Settings.Default.SessionName;
            this._subjectName = Properties.Settings.Default.SubjectName;
            this._saveDirectory = Properties.Settings.Default.FolderName;

            this.PropertyChanged += SettingsViewModel_PropertyChanged;
            this.IsSettingsEnabled = true;
        }

        private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Validate();
        }

        public bool IsSettingsEnabled
        {
            get => this._isSettingsEnabled;
            protected set => this.SetField(ref this._isSettingsEnabled, value);
        }
        protected bool _isSettingsEnabled;

        public string SessionName
        {
            get => this._sessionName;
            set => this.SetField(ref this._sessionName, value, () => Properties.Settings.Default.SessionName = value);
        }
        protected string _sessionName;

        public string SubjectName
        {
            get => this._subjectName;
            set => this.SetField(ref this._subjectName, value, () => Properties.Settings.Default.SubjectName = value);
        }
        protected string _subjectName;

        public string FolderName
        {
            get => this._saveDirectory;
            set => this.SetField(ref this._saveDirectory, value, () => Properties.Settings.Default.FolderName = value);
        }
        protected string _saveDirectory;

        public bool IsIndeterminateRecording
        {
            get => this._isIndeterminateRecording;
            set => this.SetField(ref this._isIndeterminateRecording, value);
        }
        protected bool _isIndeterminateRecording;

        public bool IsIndeterminateRecordingEnabled
        {
            get => this._isIndeterminateRecordingEnabled;
            set => this.SetField(ref this._isIndeterminateRecordingEnabled, value);
        }
        protected bool _isIndeterminateRecordingEnabled;

        public double RecordingDuration
        {
            get => this._recordingDuration;
            set => this.SetField(ref this._recordingDuration, value);
        }
        protected double _recordingDuration;
        public bool IsRecordingDurationEnabled
        {
            get => this._isRecordingDurationEnabled;
            protected set => this.SetField(ref this._isRecordingDurationEnabled, value);
        }
        protected bool _isRecordingDurationEnabled;

        public bool IsPreviewMode
        {
            get => this._isPreviewMode;
            set => this.SetField(ref this._isPreviewMode, value);
        }
        protected bool _isPreviewMode;

        public bool CompressSession
        {
            get => this._compressSession;
            set => this.SetField(ref this._compressSession, value);
        }
        protected bool _compressSession;

        public bool IsCompressSessionEnabled
        {
            get => this._isCompressSessionEnabled;
            protected set => this.SetField(ref this._isCompressSessionEnabled, value);
        }
        protected bool _isCompressSessionEnabled;

        public bool IsColorStreamEnabled
        {
            get => this._isColorStreamEnabled;
            set => this.SetField(ref this._isColorStreamEnabled, value);
        }
        private bool _isColorStreamEnabled;

        public bool IsDepthStreamEnabled
        {
            get => this._isDepthStreamEnabled;
            set => this.SetField(ref this._isDepthStreamEnabled, value);
        }
        private bool _isDepthStreamEnabled;


        public AnalogNIDAQSettingsViewModel AnalogNIDAQ { get; set; }


        public void ActivateSettings()
        {
            this.IsSettingsEnabled = false;
        }
        public void InactivateSettings()
        {
            this.IsSettingsEnabled = true;
        }

        public void Validate()
        {
            // First validate and force some combinations of settings
            if (this.IsPreviewMode)
            {
                this.IsDepthStreamEnabled = true;
                this.IsColorStreamEnabled = true;
                // TODO: Enable NIDAQ stream
                this.IsRecordingDurationEnabled = false;
                this.IsCompressSessionEnabled = false;
                this.IsIndeterminateRecordingEnabled = false;
            }
            else if (this.IsIndeterminateRecording)
            {
                this.IsRecordingDurationEnabled = false;
                this.IsCompressSessionEnabled = true;
            }
            else if (this.RecordingDuration > 0)
            {
                this.IsIndeterminateRecordingEnabled = false;
                this.IsCompressSessionEnabled = true;
            }
            else
            {
                this.IsIndeterminateRecordingEnabled = true;
                this.IsRecordingDurationEnabled = true;
                this.IsCompressSessionEnabled = true;
            }

            // Next, validate some of the arguments
            
        }
    }
}
