﻿using System;
using Meadow;
using Meadow.GnssTracker.Core;
using Meadow.GnssTracker.Core.Models.Logical;
using Meadow.Logging;

namespace Demo_App.Controllers
{
    /// <summary>
    /// This is the main tracker application controller. It's responsible for
    /// orchestrating the entire operation of the application.
    /// </summary>
    public class MainTrackerController
    {
        protected Logger Log { get => Resolver.Log; }
        protected GnssTrackerHardware Hardware { get; set; }
        protected AtmosphericModel? LastAtmosphericConditions { get; set; }
        protected LocationModel? LastLocationInfo { get; set; }
        protected IoBTController IoBTController { get; set; }

        public MainTrackerController(GnssTrackerHardware hardware)
        {
            this.Hardware = hardware;
            GnssController.GnssPositionInfoUpdated += GnssPositionInfoUpdated;

            var panID = "Wilderness Labs - Portland";
            Guid sensorID = Guid.Parse("d5505ddc-8d3e-43e5-ab3e-04ca0e4e5d08");

            IoBTController = new IoBTController(panID, sensorID);
        }

        /// <summary>
        /// Starts updating all the things
        /// </summary>
        public void Start()
        {
            //==== start updating everything

            //---- Heartbeat LED
            Hardware.OnboardLed.StartPulse();

            //---- BME688 Atmo sensor
            if (Hardware.AtmosphericSensor is { } bme)
            {
                bme.Updated += AtmosphericSensorUpdated;
                bme.StartUpdating(TimeSpan.FromSeconds(10));
            }

            //---- GPS
            GnssController.StartUpdating();
        }

        void GnssPositionInfoUpdated(object sender, Meadow.Peripherals.Sensors.Location.Gnss.GnssPositionInfo result)
        {
            // update
            this.LastLocationInfo = new LocationModel { PositionInformation = result };

            //---- Console/Debug Output
            if (result.Position is { } pos)
            {
                if (pos.Latitude is { } lat && pos.Longitude is { } lon)
                {
                    Log.Info($"RM: lat: [{pos.Latitude}], long: [{pos.Longitude}]");
                    ////---- save to database
                    //SaveConditions(this.CurrentConditions);
                }
                else { Log.Info("RM Position lat/long empty."); }
            }
            else { Log.Info("RM Position not yet found."); }

            //---- Update system time from SPAAAAACE
            if (result.TimeOfReading is { } timeOfReading)
            {
                //Resolver.Device.SetClock(timeOfReading);
                Log.Info($"UTC DateTime from GPS: {timeOfReading.ToShortDateString()} :: {timeOfReading.ToShortTimeString()}");
                Log.Info($"Device DateTime: {DateTime.Now.ToShortDateString()} :: {DateTime.Now.ToShortTimeString()}");

                if (timeOfReading.Date != DateTime.Now.Date)
                {
                    Log.Info($"Device date is different than GPS time. Updating.");
                    Resolver.Device.SetClock(timeOfReading);
                    Log.Info($"Device time set. {DateTime.Now.ToShortDateString()}");
                }
            }

            //---- update the display and save to the database
            DatabaseController.SaveLocationInfo(LastLocationInfo);
            DisplayController.UpdateGnssPositionInformation(LastLocationInfo);
            IoBTController.PostLocation(LastLocationInfo);
        }

        void AtmosphericSensorUpdated(object sender, IChangeResult<(Meadow.Units.Temperature? Temperature, Meadow.Units.RelativeHumidity? Humidity, Meadow.Units.Pressure? Pressure, Meadow.Units.Resistance? GasResistance)> result)
        {
            Log.Info($"BME688: Temperature: {result.New.Temperature?.Celsius:N2}C,");
            Log.Info($"BME688: Relative Humidity: {result.New.Humidity:N2}%, ");
            Log.Info($"BME688: Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.StandardAtmosphere:N2}atm)");

            //---- update CurrentConditions
            if (LastAtmosphericConditions == null) { this.LastAtmosphericConditions = new AtmosphericModel(); }
            var newConditions = new AtmosphericModel
            {
                Temperature = result.New.Temperature,
                RelativeHumidity = result.New.Humidity,
                Pressure = result.New.Pressure,
                Timestamp = DateTime.Now
            };
            this.LastAtmosphericConditions.Update(newConditions);

            //---- update display and save to database
            DatabaseController.SaveAtmosphericConditions(this.LastAtmosphericConditions);
            DisplayController.UpdateAtmosphericConditions(this.LastAtmosphericConditions);
            IoBTController.PostAtmosphericConditions(this.LastAtmosphericConditions);
        }
    }
}