using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Demo_App.Controllers;
using Meadow;
using Meadow.Devices;
using Meadow.Gateway.WiFi;
using Meadow.GnssTracker.Core;
using Meadow.Hardware;
using Meadow.Logging;

namespace Demo_App
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        protected GnssTrackerHardware Hardware { get; set; }
        protected Logger Log { get => Resolver.Log; }
        protected MainTrackerController MainController { get; set; }

        public override Task Initialize()
        {
            Log.Info("Initialize hardware...");

            //==== Connect to the network
            ConnectToNetwork().Wait();

            //==== Bring up the hardware
            Hardware = new GnssTrackerHardware(Device);

            //==== Bring up all the controllers
            //---- Database
            try { DatabaseController.ConfigureDatabase(); }
            catch (Exception e) { Log.Info($"Err bringing up database: {e.Message}"); }

            //---- Display Controller
            DisplayController.Initialize(Hardware.EPaperDisplay);

            //---- GNSS Controller
            GnssController.Initialize(Hardware.Gnss);

            //---- Main Tracker Controller (ties everything together)
            this.MainController = new MainTrackerController(Hardware);

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Running");

            //---- start the engines
            this.MainController.Start();

            return base.Run();
        }

        protected async Task ConnectToNetwork()
        {
            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            // connected event test.
            wifi.NetworkConnected += Wifi_NetworkConnected;

            try
            {
                await wifi.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception e)
            {
                Resolver.Log.Error(e.Message);
            }
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            DisplayNetworkInformation();

            Log?.Info("Connection request completed.");
        }

        protected void DisplayNetworkInformation()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            if (adapters.Length == 0)
            {
                Console.WriteLine("No adapters available.");
            }
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine();
                Console.WriteLine(adapter.Description);
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                Console.WriteLine($"  Adapter name: {adapter.Name}");
                Console.WriteLine($"  Interface type .......................... : {adapter.NetworkInterfaceType}");
                Console.WriteLine($"  Physical Address ........................ : {adapter.GetPhysicalAddress().ToString()}");
                Console.WriteLine($"  Operational status ...................... : {adapter.OperationalStatus}");
                string versions = String.Empty;
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                    {
                        versions += " ";
                    }
                    versions += "IPv6";
                }
                Console.WriteLine($"  IP version .............................. : {versions}");
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    IPv4InterfaceProperties ipv4 = properties.GetIPv4Properties();
                    Console.WriteLine("  MTU ..................................... : {0}", ipv4.Mtu);
                }
                if ((adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) || (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine($"  IP address .............................. : {ip.Address.ToString()}");
                            Console.WriteLine($"  Subnet mask ............................. : {ip.IPv4Mask.ToString()}");
                        }
                    }
                }
            }
        }
    }
}