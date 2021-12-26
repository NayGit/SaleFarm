using System.Text.Json;

namespace SaleFarm.Models
{
    public class AsfBotAllReceive
    {
        public JsonDocument Result { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; }
    }

    public class AsfBotResult
    {
        public string BotName { get; set; }
        public bool IsConnectedAndLoggedOn { get; set; }
    }
}
