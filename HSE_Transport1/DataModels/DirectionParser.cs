using System.Collections.Generic;

namespace HSE_Transport1.Helpers
{
    public class Duration
    {
        public int value { get; set; }
    }

    public class Leg
    {
        public Duration duration { get; set; }
    }

    public class Route
    {
        public IList<Leg> legs { get; set; }
    }

    public class DirectionParser
    {
        public IList<Route> routes { get; set; }
    }
}