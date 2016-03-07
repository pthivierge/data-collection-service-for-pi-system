using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace DCS.Core.DataReaders.FakeRandomDataReader
{
    /// <summary>
    /// This data reader can be used to generate random data when you need to test data collection.
    /// It requires a database that contains elements based on an ElementTemplate with the following Attributes: 
    /// value:     the PI Point where the data will be written
    /// LowValue:  An integer static attribute that defines the lowest random value
    /// HighValue: An integer static attribute that defines the highest random value
    /// </summary>
    public class FakeRandomBaseDataReader : BaseDataReader, IDataReader 
    {
        Random _randomGenerator=new Random();
        

        public override List<AFValue> ReadValues(AFElement element)
        {
            var attribute = element.Attributes["Value"];

            var min= (int)element.Attributes["LowValue"].GetValue().Value;
            var max = (int)element.Attributes["HighValue"].GetValue().Value;

            var newValue = _randomGenerator.Next(min,max);

            var value=new AFValue(attribute,newValue,AFTime.Now);

            return new List<AFValue>() {value};
        }

        public FakeRandomBaseDataReader(string afServerName, string afDataBaseName, string elementTemplateName) : base(afServerName, afDataBaseName, elementTemplateName)
        {
        }
    }
}
