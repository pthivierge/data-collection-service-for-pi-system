using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using log4net;

namespace WSR.Core.Helpers
{
    public static class AFSDKHelpers
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(DataReadersManager));

        /// <summary>
        /// Loads Elements from an AF Database in a manner that is not blocking the application
        /// and is more efficient with big databases
        /// </summary>
        /// <param name="database">The AFdatabase that contains the elements to load</param>
        /// <param name="template"> the Element Template associated with the elements to load</param>
        /// <param name="elementsConcurrentQueue">Concurrent queue in which the elements will be loaded</param>
        public static void LoadElementsByTemplate(AFDatabase database, AFElementTemplate template, ConcurrentQueue<AFElement> elementsConcurrentQueue)
        {

            // set variables
            const int chunkSize = 10000;
            int index = 0;
            int total;

            do
            {
                // loads elements by chunk of 10K values 
                var elements = template.FindInstantiatedElements(true,
                    AFSortField.Name, AFSortOrder.Ascending, index, chunkSize, out total);

                var elementCount = elements.Count;
                if (elementCount == 0)
                    break;

                // Convert a list of AFBaseElement to a list of AFElement
                List<AFElement> elementsList = elements.Select(e => (AFElement)e).ToList();

                // forces full load of elements
                AFElement.LoadElements(elementsList);

                // if you'd like to filter the elements by attributes... 
                // however this would be sub-optimal, it would be better to filter directly on the FindInstanciated Elements query.
                //elementsList = elementsList.Where(e => (bool)e.Attributes["ReadEnabled"].GetValue().Value == true).ToList();
                foreach (var afElement in elementsList)
                {
                    elementsConcurrentQueue.Enqueue(afElement);
                }

                _logger.InfoFormat(" Load Elements by Template | StartIndex = {1} | Found a chunk of {2}  elements", DateTime.Now, index, elementCount);

                index += chunkSize;

            } while (index < total);

            // the findElements call we are using returns a paged collection to lower the memory foortprint





        }



    }
}
