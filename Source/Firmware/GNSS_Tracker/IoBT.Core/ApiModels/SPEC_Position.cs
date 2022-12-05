using System;
namespace IoBT.Core.ApiModels
{
    public partial class SPEC_Position
    {
        public string UdtoTopic { get; set; }

        public string SourceGuid { get; set; }

        public string RefGuid { get; set; }

        public string TimeStamp { get; set; }

        public string PanID { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Alt { get; set; }
    }
}