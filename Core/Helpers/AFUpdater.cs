using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF;

namespace DCS.Core.Helpers
{
    public class AFUpdater
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(AFUpdater));

        object _cookie = null;
        AFDatabase _database;

        public AFUpdater(AFDatabase database)
        {
            _database = database;
        }

        public void RefreshLoadedObjects()
        {
            var changes = _database.FindChangedItems(AFIdentity.Element, false, int.MaxValue, _cookie, out _cookie);

            _logger.InfoFormat("Found {0} changes in the database", changes.Count);
            foreach (var afChangeInfo in changes)
            {
                   _logger.DebugFormat("Update found element ID {1},Update is value update: {0}",afChangeInfo.IsValueUpdate,afChangeInfo.ID);
            }

            // Refresh objects that have been changed.
            AFChangeInfo.Refresh(_database.PISystem, changes);
            
        }
    }
}
