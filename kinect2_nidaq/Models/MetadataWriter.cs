using kinect2_nidaq.ViewModels;
using Metadata;
using Newtonsoft.Json;
using Sensor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class MetadataWriter : IDataWriter
    {
        private string _destPath;
        private SettingsViewModel settings;
        private KinectViewModel kinect;

        public MetadataWriter(string destPath, SettingsViewModel settings, KinectViewModel kinect)
        {
            this._destPath = destPath;
            this.settings = settings;
            this.kinect = kinect;
        }

        public void Start()
        {
            this.Write();
        }

        public void Stop()
        {
            // do nothing
        }

        protected void Write()
        {
            kMetadata fMetadata = new kMetadata();
            fMetadata.SubjectName = this.settings.SubjectName;
            fMetadata.SessionName = this.settings.SessionName;
            fMetadata.ApparatusName = Properties.Settings.Default.ApparatusName;

            fMetadata.ColorResolution = new int[2] { this.kinect.ColorInfo.Width, this.kinect.ColorInfo.Height };
            fMetadata.ColorDataType = this.GetTypeForProperty(typeof(ColorFrameEventArgs), "ColorData");

            fMetadata.DepthResolution = new int[2] { this.kinect.DepthInfo.Width, this.kinect.DepthInfo.Height };
            fMetadata.IsLittleEndian = BitConverter.IsLittleEndian;
            fMetadata.DepthDataType = this.GetTypeForProperty(typeof(DepthFrameEventArgs), "DepthData");
            
            if (this.settings.AnalogNIDAQ.IsEnabled)
            {
                fMetadata.NidaqChannels = this.settings.AnalogNIDAQ.SelectedChannels.Count;
                fMetadata.NidaqTerminalConfiguration = this.settings.AnalogNIDAQ.TerminalConfiguration.ToString();
                fMetadata.NidaqChannelNames = this.settings.AnalogNIDAQ.SelectedChannels.ToArray();
                fMetadata.NidaqSamplingRate = this.settings.AnalogNIDAQ.SamplingRate;
                fMetadata.NidaqVoltageRange = this.settings.AnalogNIDAQ.VoltageRange.ToString();
                fMetadata.NidaqDataType = this.GetTypeForProperty(typeof(NidaqData), "Data");
            }

            fMetadata.StartTime = DateTime.Now;

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(this._destPath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, fMetadata);
            }
        }


        protected string GetTypeForProperty(Type type, string propertyName)
        {
            PropertyInfo prop = type.GetProperty(propertyName);
            return prop.PropertyType.Name;
        }
    }
}
