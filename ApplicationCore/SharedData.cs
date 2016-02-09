using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;

namespace FDS.Core
{
    public static class SharedData
    {
        public static readonly ConcurrentQueue<List<AFElement>> FitBitDevices =new ConcurrentQueue<List<AFElement>>();

        public static readonly ConcurrentQueue<List<AFValue>> DataQueue = new ConcurrentQueue<List<AFValue>>();

    }
}
