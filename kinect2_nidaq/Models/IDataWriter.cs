using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinect2_nidaq.Models
{
    public interface IDataWriter
    {
        void Start();
        void Stop();
    }
}
