using System;

namespace HSE_Transport1
{
    [Serializable]
    public class Bus
    {
        public DateTime DepartureTime { get; set; }
        public string DeparturePlace { get; set; }
        public string ArrivalPlace { get; set; }
        public string Occupancy { get; set; }
        public string Day { get; set; }
    }
}