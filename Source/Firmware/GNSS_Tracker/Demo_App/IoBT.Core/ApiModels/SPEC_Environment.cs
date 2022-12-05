using System;
namespace IoBT.Core.ApiModels
{
    public partial class SPEC_Environment
    {
        public string UdtoTopic { get; set; }

        public string SourceGuid { get; set; }

        public string RefGuid { get; set; }

        public string TimeStamp { get; set; }

        public string PanID { get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        public double Pressure { get; set; }

        public SPEC_Direction Wind { get; set; }
    }
}