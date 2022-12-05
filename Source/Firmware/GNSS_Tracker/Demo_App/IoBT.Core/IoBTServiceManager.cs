using System;
using IoBT.Core.ApiModels;
using Meadow;
using Meadow.GnssTracker.Core.Models.Logical;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Sensors.Location;
using Refit;

namespace IoBT.Core
{
    public class IoBTServiceManager
    {
        protected Logger Log { get => Resolver.Log; }
        protected IobtApi Service { get; set; }

        /// <summary>
        /// Personal Area Network ID
        /// </summary>
        public string PanID { get; protected set; }
        /// <summary>
        /// Unique sensor ID
        /// </summary>
        public Guid SourceGuid { get; protected set; }

        public IoBTServiceManager(string panID, Guid sourceGuid)
        {
            this.PanID = panID;
            this.SourceGuid = sourceGuid;

            this.Service = RestService.For<IobtApi>("https://iobtsquire1.azurewebsites.net/api");
        }

        /// <summary>
        /// IoBT::[/api/ClientHub/Position]
        /// </summary>
        /// <param name="location"></param>
        public async void PostPosition(LocationModel location)
        {
            Log?.Info($"IoBTServiceManager.PostPosition()");

            double latitude = -1;
            double longitude = -1;
            double altitude = -1;

            if (location?.PositionInformation?.Position?.Latitude is { } lat)
            {
                latitude = lat.ConvertDMSToDecimalDegrees();
            }

            if (location?.PositionInformation?.Position?.Longitude is { } lon)
            {
                longitude = lon.ConvertDMSToDecimalDegrees();
            }

            if (location?.PositionInformation?.Position?.Altitude is { } alt)
            {
                altitude = (double)alt;
            }

            var position = new SPEC_Position
            {
                Alt = altitude,
                Lat = latitude,
                Lng = longitude,
                PanID = this.PanID,
                SourceGuid = this.SourceGuid.ToString()
            };

            Log?.Info($"SPEC_Position: {position.ToString()}");

            await this.Service.SetPosition(position);
        }

        /// <summary>
        /// IoBT::[/api/ClientHub/Environment]
        /// </summary>
        /// <param name="atmosphericConditions"></param>
        public async void PostEnvironment(AtmosphericModel atmosphericConditions)
        {
            Log?.Info($"IoBTServiceManager.PostEnvironment()");

            double tempC = 0;
            double relativeHumidityPercent = 0;
            double pressureAtmos = 0;

            if (atmosphericConditions.Temperature?.Celsius is { } celsius) {
                tempC = celsius;
            }

            if (atmosphericConditions.RelativeHumidity?.Percent is { } humidity)
            {
                relativeHumidityPercent = humidity;
            }

            if (atmosphericConditions.Pressure?.StandardAtmosphere is { } atmos)
            {
                pressureAtmos = atmos;
            }

            var environment = new SPEC_Environment
            {
                Temperature = tempC,
                Humidity = relativeHumidityPercent,
                Pressure = pressureAtmos,
                PanID = this.PanID,
                SourceGuid = this.SourceGuid.ToString()
            };

            Log?.Info($"SPEC_Environment: {environment.ToString()}");

            await this.Service.SetEnvironment(environment);
        }
    }

    public static class Extensions
    {
        public static double ConvertDMSToDecimalDegrees(this DegreesMinutesSecondsPosition degreesMinutesSeconds)
        {
            double decimalDegrees = degreesMinutesSeconds.Degrees + ((double)degreesMinutesSeconds.Minutes / 60) + ((double)degreesMinutesSeconds.seconds / 3600);
            return decimalDegrees;
        }
    }
}