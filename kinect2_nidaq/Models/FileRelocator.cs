using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public class FileRelocator
    {
        private FilePathHelper filePaths;

        public FileRelocator(FilePathHelper filePaths)
        {
            this.filePaths = filePaths;
        }

        public void RelocateFiles()
        {
            Console.WriteLine("Relocating acquisition files...");
            foreach (string[] FileName in this.filePaths.FileToTarMemberMapping)
            {
                if (File.Exists(FileName[0]))
                {
                    var newDest = Path.Combine(this.filePaths.MoveFolder, FileName[1]);
                    Console.WriteLine(String.Format("Moving {0} to {1}", FileName[0], newDest));
                    File.Move(FileName[0], newDest);
                }
                else
                {
                    Console.WriteLine(String.Format("Skipping {0}, since it seems to not exist!", FileName[0]));
                }
            }
        }
    }
}
