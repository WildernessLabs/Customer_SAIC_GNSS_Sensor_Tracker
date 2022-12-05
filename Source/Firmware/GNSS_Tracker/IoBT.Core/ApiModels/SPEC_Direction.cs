using System;
namespace IoBT.Core.ApiModels
{
    public partial class SPEC_Direction
    {
        public string UdtoTopic { get; set; }

        public string SourceGuid { get; set; }

        public string RefGuid { get; set; }

        public string TimeStamp { get; set; }

        public string PanID { get; set; }

        public double Speed { get; set; }

        public double Heading { get; set; }
    }
}