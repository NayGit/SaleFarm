using System;

namespace SaleFarm.Enums
{
    [Flags]
    public enum GoFlagsStatus : short
    {
        None = 0,
        Wait = 1,
        Explore = 2,
        SteamAward = 4
    }
}
