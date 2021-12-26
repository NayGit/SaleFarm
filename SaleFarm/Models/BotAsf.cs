using SaleFarm.Enums;

namespace SaleFarm.Models
{
    public class BotAsf : IBot
    {
        public string Name { get; set; }
        public string shared_secret { get; set; }
        public BotAsfStatus BotAsfStatus { get; set; } = BotAsfStatus.None;
    }
}
