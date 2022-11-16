using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class FilePathHelper
    {
        
        public FilePathHelper(string SaveFolder)
        {
            this.SaveFolder = SaveFolder;
            this.Moniker = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        public string SaveFolder { get; protected set; }
        public string Moniker { get; protected set; }
        public string MoveFolder { get => Path.Combine(this.SaveFolder, String.Format("session_{0}", this.Moniker)); }
        public string ColorTS { get => Path.Combine(this.SaveFolder, String.Format("rgb_ts_{0}.txt", this.Moniker)); }
        public string ColorVid { get => Path.Combine(this.SaveFolder, String.Format("rgb_{0}.mp4", this.Moniker)); }
        public string DepthTS { get => Path.Combine(this.SaveFolder, String.Format("depth_ts_{0}.txt", this.Moniker)); }
        public string DepthVid { get => Path.Combine(this.SaveFolder, String.Format("depth_{0}.dat", this.Moniker)); }
        public string Nidaq { get => Path.Combine(this.SaveFolder, String.Format("nidaq_{0}.dat", this.Moniker)); }
        public string Metadata { get => Path.Combine(this.SaveFolder, String.Format("metadata_{0}.json", this.Moniker)); }

        public string Tar { get => Path.Combine(this.SaveFolder, String.Format("session_{0}.tar.gz", this.Moniker)); }
        public string TarMetadata { get => Path.Combine(this.SaveFolder, String.Format("session_{0}.json", this.Moniker)); }

        public List<string[]> FileToTarMemberMapping
        {
            get
            {
                return new List<string[]>() {
                    new string[] { this.ColorTS, "rgb_ts.txt" },
                    new string[] { this.ColorVid, "rgb.mp4" },
                    new string[] { this.DepthTS, "depth_ts.txt" },
                    new string[] { this.DepthVid, "depth.dat" },
                    new string[] { this.Metadata, "metadata.json" },
                    new string[] { this.Nidaq, "nidaq.dat" }
                };
            }
        }
    }
}
