using SaleFarm.Enums;
using SaleFarm.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaleFarm.Services
{
    public class AsfHttpService
    {
        private HttpClient _httpClient { get; set; }

        public AsfHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<string> GetAsync(HttpMethodEnum method, string uri, string param = "")
        {
            string result;
            try
            {
                switch (method)
                {
                    case HttpMethodEnum.Get:
                        result = await _httpClient.GetStringAsync(uri);
                        break;
                    case HttpMethodEnum.Post:
                        var a = await _httpClient.PostAsync(uri, new StringContent(param, Encoding.UTF8, "application/json"));
                        result = await a.Content.ReadAsStringAsync();
                        break;
                    default:
                        result = $"{nameof(HttpMethodEnum)}: ERROR";
                        break;
                };
            }
            catch (HttpRequestException e)
            {
                result = e.Message;
            }

            return result;
        }

        #region GetBotsAsfAsync
        internal async Task<List<BotAsf>> GetBotsAsfAsync(string urlAsf)
        {
            List<BotAsf> botAsf = new();

            try
            {
                var value = await GetAsync(HttpMethodEnum.Get, $"{urlAsf.TrimEnd('/')}/Api/Bot/asf");

                var bot = JsonSerializer.Deserialize<AsfBotAllReceive>(value);

                Dictionary<string, bool> tmp = new();

                foreach (var item in bot.Result.RootElement.EnumerateObject())
                {
                    var botResult = JsonSerializer.Deserialize<AsfBotResult>(item.Value.ToString());
                    tmp.Add(item.Name, botResult.IsConnectedAndLoggedOn);
                }

                Regex rgx = new(@"^θtf\d+client$");
                foreach (var item in tmp)
                {
                    if (!rgx.IsMatch(item.Key.ToLower()))
                    {
                        botAsf.Add(new BotAsf() { Name = item.Key });
                    }
                }
            }
            catch
            {
            }

            return botAsf;
        }
        #endregion

        #region GetBotTokenAsync
        internal async Task<string> GetBotTokenAsync(string urlAsf, BotAsf bot)
        {
            var value1 = await GetAsync(HttpMethodEnum.Get, $@"{urlAsf}/Api/Bot/{bot.Name}/TwoFactorAuthentication/Token");

            var botTmp2 = JsonSerializer.Deserialize<AsfBotAllReceive>(value1);
            var botTmp3 = JsonSerializer.Deserialize<AsfBotAllReceive>(botTmp2.Result.RootElement.EnumerateObject().ToArray()[0].Value.ToString());
            return botTmp3.Result.RootElement.ToString();
        }
        #endregion

        #region ConnectedBotAsfAsync
        internal async Task/*<BotAsfStatus>*/ ConnectedBotAsfAsync(string urlAsf, BotAsf bot)
        {
            try
            {
                string start = await GetAsync(HttpMethodEnum.Post, $@"{urlAsf}/Api/Bot/{bot.Name}/Start");
                AsfReceive command = JsonSerializer.Deserialize<AsfReceive>(start);

                bot.BotAsfStatus = command.Success ? BotAsfStatus.Disconnected : BotAsfStatus.ConnectedAndLoggedOn;
            }
            catch
            {
                bot.BotAsfStatus = BotAsfStatus.None;
            }

            //return bot.BotAsfStatus;
        }
        #endregion

        #region DisConnectedBotAsfAsync
        internal async Task DisConnectedBotAsfAsync(string urlAsf, BotAsf bot)
        {
            if (bot.shared_secret is null)
            {
                if (bot.BotAsfStatus == BotAsfStatus.Disconnected)
                {
                    _ = GetAsync(HttpMethodEnum.Post, $@"{urlAsf}/Api/Bot/{bot.Name}/Stop");
                }
            }
        }
        #endregion
    }
}
