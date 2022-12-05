using System;
using IoBT.Core;
using Meadow;
using Meadow.GnssTracker.Core.Models.Logical;
using Meadow.Logging;

namespace Demo_App.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class IoBTController
    {
        protected Logger Log { get => Resolver.Log; }

        protected IoBTServiceManager ServiceManager { get; set; }

        public string PanID { get; set; }
        public Guid SensorID { get; set; }

        public IoBTController(string panID, Guid sensorID)
        {
            this.SensorID = sensorID;
            this.PanID = panID;

            ServiceManager = new IoBTServiceManager(PanID, sensorID);
        }

        public void PostLocation(LocationModel locationInfo)
        {
            Log?.Info($"IoBTController.PostLocation()");
            ServiceManager.PostPosition(locationInfo);
        }

        public void PostAtmosphericConditions(AtmosphericModel atmosphericConditions)
        {
            Log?.Info($"IoBTController.PostAtmosphericConditions()");
            ServiceManager.PostEnvironment(atmosphericConditions);
        }
    }
}

