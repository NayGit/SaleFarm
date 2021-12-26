using SaleFarm.Enums;

namespace SaleFarm.Models
{
    public interface IBot
    {
        string Name { get; }
        string shared_secret { get; }
        BotAsfStatus BotAsfStatus { get; set; }
    }
}
