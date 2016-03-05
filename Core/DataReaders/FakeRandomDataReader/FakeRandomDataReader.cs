using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace WSR.Core.DataReaders.FakeRandomDataReader
{
    public class FakeRandomDataReader : DataReader
    {
        Random _randomGenerator=new Random();

        public FakeRandomDataReader(string afServerName, string afDataBaseName, string elementTemplateName) : base(afServerName, afDataBaseName, elementTemplateName)
        {
            
        }

        protected override List<AFValue> ReadValues(AFElement element)
        {
            var attribute = element.Attributes["Value"];

            var min= (int)element.Attributes["LowValue"].GetValue().Value;
            var max = (int)element.Attributes["HighValue"].GetValue().Value;

            var newValue = _randomGenerator.Next(min,max);

            var value=new AFValue(attribute,newValue,AFTime.Now);

            return new List<AFValue>() {value};
        }
    }
}
