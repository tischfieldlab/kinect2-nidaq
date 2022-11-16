using GzTar;
using Sensor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class CompressionProgressEventArgs
    {
        public double ElapsedSeconds { get; set; }
        public long TotalBytes { get; set; }
        public long WrittenBytes { get; set; }
        public double Progress { get => this.WrittenBytes / this.TotalBytes; }
        public double ProgressRate { get => (this.ElapsedSeconds / this.Progress); }
        public double InstantaneousETA { get => (1 - this.Progress) * this.ProgressRate; }
        public double SmoothedETA { get; set; }
    }
    public class SessionCompressor
    {
        private FilePathHelper filePaths;
        public event EventHandler<CompressionProgressEventArgs> Progress;

        public SessionCompressor(FilePathHelper filePaths)
        {
            this.filePaths = filePaths;
        }

        public void CompressSession()
        {
            // First, estimate the total number of bytes to be compressed
            long totalBytes = 0;
            foreach (string[] FileName in this.filePaths.FileToTarMemberMapping)
            {
                if (File.Exists(FileName[0]))
                {
                    FileInfo tmpInfo = new FileInfo(FileName[0]);
                    totalBytes += tmpInfo.Length;
                }
            }

            // start a stopwatch to keep track of time elapsed, and som other vars for tracking progress
            long totalWritten = 0;
            var ETAQueue = new RunningAverage(Constants.etaMaxBuffer);
            Stopwatch ETATgz = new Stopwatch();
            ETATgz.Start();

            using (AGZTar tarball = new AGZTar(this.filePaths.Tar))
            {
                if (totalBytes != 0)
                {
                    tarball.WriteEvent += (sender, args) =>
                    {
                        totalWritten += args.Written;
                        var progress = new CompressionProgressEventArgs()
                        {
                            ElapsedSeconds = ETATgz.Elapsed.TotalSeconds,
                            TotalBytes = totalBytes,
                            WrittenBytes = totalWritten,
                        };

                        ETAQueue.PutNextSample(progress.InstantaneousETA);
                        progress.SmoothedETA = ETAQueue.Average;

                        this.Progress?.Invoke(this, progress);
                    };
                }

                // Write each file to the TAR file
                foreach (string[] FileName in this.filePaths.FileToTarMemberMapping)
                {
                    Console.WriteLine(String.Format("{0} {1}", FileName[0], FileName[1]));

                    if (File.Exists(FileName[0]))
                    {
                        Console.WriteLine(String.Format("{0} {1}", FileName[0], FileName[1]));
                        tarball.Write(FileName[0], FileName[1]);
                    }
                }
            }

            //  Copy the metadata file to the TAR metadata location (as a "sidecar" file)
            File.Copy(this.filePaths.Metadata, this.filePaths.TarMetadata);

            // delete each of the origional files, which are now saved in the TAR file
            foreach (string[] FileName in this.filePaths.FileToTarMemberMapping)
            {
                if (File.Exists(FileName[0]))
                {
                    File.Delete(FileName[0]);
                }
            }
        }
    }

    public class RunningAverage
    {
        protected int numSamples;
        private Queue<double> _queue;

        public RunningAverage(int numSamples)
        {
            this.numSamples = numSamples;
            this._queue = new Queue<double>(this.numSamples);
        }

        public void PutNextSample(double sample)
        {
            this._queue.Enqueue(sample);
            while (this._queue.Count > this.numSamples)
            {
                this._queue.Dequeue();
            }
        }

        public double Average
        {
            get => this._queue.Average();
        }
    }
}
