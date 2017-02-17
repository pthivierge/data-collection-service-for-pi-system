using System.Collections.Generic;
using OSIsoft.AF.Asset;
using DCS.Core.Configuration;

namespace DCS.Core.DataCollectors
{
    public interface IDataCollector
    {
        List<AFValue> ReadValues(AFElement element);
        void RefreshConfiguration();
        void CollectData();
        void Inititialize();

        DataCollectorSettings GetSettings();

        void SetSettings(DataCollectorSettings settings);

    }
}
