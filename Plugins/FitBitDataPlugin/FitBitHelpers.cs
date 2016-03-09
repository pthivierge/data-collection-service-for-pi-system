using System;
using Fitbit.Models;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace FitBitDataPlugin
{
    public static class FitBitHelpers
    {
        /// <summary>
        /// Converts FitFibt Time Series Data to AF Time Series Data
        /// </summary>
        /// <param name="tsDataList">FitBit Time Series data</param>
        /// <param name="type">Type of fit Bit data - this will determine the attribute name to write into. It can also influence the conversion logic.</param>
        /// <param name="element">The element that contains the attribute to write into</param>
        /// <param name="attributeName">Name of the AF Attribute in which time series data will be written into.</param>
        /// <returns></returns>
        public static AFValues ConvertToAFValues(TimeSeriesDataList tsDataList, TimeSeriesResourceType type, AFElement element, string attributeName)
        {

            // creates the list of values
            AFValues values = new AFValues();
            foreach (var result in tsDataList.DataList)
            {
                AFValue val = null;
                if (type != TimeSeriesResourceType.TimeEnteredBed)
                {
                    val = new AFValue(Convert.ToSingle(result.Value), new AFTime(result.DateTime));
                }
                else
                {
                    val = new AFValue(result.Value, new AFTime(result.DateTime));
                }
                values.Add(val);

            }

            values.Attribute = element.Attributes[attributeName];
            
            return values;
        }


    }
}
