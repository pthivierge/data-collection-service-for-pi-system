using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;

namespace DCS.Core.DataReaders
{
    public interface IDataReader
    {
        List<AFValue> ReadValues(AFElement element);
        void RefreshConfiguration();
        void CollectData();
        void Inititialize();
        
    }
}
