using SaleFarm.Commands;
using SaleFarm.Enums;
using SaleFarm.Models;
using SaleFarm.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaleFarm.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private AsfHttpService AsfHttpService { init; get; }

        #region ctor
        public MainWindowViewModel(AsfHttpService asfHttpService)
        {
            AsfHttpService = asfHttpService;


            IsEnabled = true;
            isfarm = false;

            ModeStatus = ModeStatus.None;
            UrlAsf = @$"http://192.168.1.11:1242";

            IsExplore = false;
            IsSteamAward = false;

            Source = "https://store.steampowered.com/";

            //_ = AsfHttpService.GetBotTokenAsync("", new() { Name = "aleksnadt" });
        }
        #endregion

        #region prop
        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        //private bool farm;
        private bool isfarm;
        public bool Isfarm
        {
            get { return isfarm; }
            set { SetProperty(ref isfarm, value); }
        }

        public ModeStatus ModeStatus { get; set; }
        public string UrlAsf { get; set; }

        public GoFlagsStatus goFlagsCopy;
        public bool IsExplore { get; set; }
        public bool IsSteamAward { get; set; }

        private string source;
        public string Source
        {
            get { return source; }
            set
            {
                SetProperty(ref source, value);
            }
        }

        public List<BotAsf> botAsf = new();
        public async Task<string> GetToken()
        {
            return await AsfHttpService.GetBotTokenAsync(UrlAsf, botAsf[0]);
        }
        #endregion

        #region Start Command
        private ICommand _CommandGo;
        public ICommand CommandGo => _CommandGo ?? (_CommandGo =
                    new RelayCommandAsync(GoStopAsync, p => CanGoStop()));


        private CancellationTokenSource tokenSource;
        private async Task GoStopAsync()
        {
            if (!Isfarm)
            {
                Isfarm = true;
                IsEnabled = false;
                tokenSource = new CancellationTokenSource();

                if (ModeStatus == ModeStatus.ASF)
                {
                    try
                    {
                        botAsf = await AsfHttpService.GetBotsAsfAsync(UrlAsf);
                        if (botAsf.Count < 1)
                        {
                            Stop("Error: Bots < 1");
                            return;
                        }
                    }
                    catch
                    {
                        Stop("Error: AsfUrl");
                        return;
                    }
                }

                GoFlagsStatus goFlagsStatus = GoFlagsStatus.None;
                if (IsExplore)
                {
                    goFlagsStatus |= GoFlagsStatus.Explore;
                }
                if (IsSteamAward)
                {
                    goFlagsStatus |= GoFlagsStatus.SteamAward;
                }

                if (goFlagsStatus == GoFlagsStatus.None)
                {
                    Stop("Pls, check");
                    return;
                }

                _ = GoWhileAsync(goFlagsStatus, tokenSource.Token);
            }
            else
            {
                tokenSource.Cancel();
                Isfarm = false;
            }
        }

        private bool CanGoStop()
        {
            return IsEnabled || (Isfarm && !IsEnabled);
        }
        #endregion

        #region GoWhileAsync
        public async Task GoWhileAsync(GoFlagsStatus goFlagsStatus, CancellationToken ct)
        {
            //X | Q устанавливает бит(ы) Q
            //X & ~Q очищает бит(ы) Q
            //~X переворачивает / инвертирует все биты в X

            goFlagsCopy = goFlagsStatus;

            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    // Clean up here, then...
                    //ct.ThrowIfCancellationRequested();

                    Stop("Cancel");
                    return;
                }

                else if (goFlagsCopy == GoFlagsStatus.None)
                {
                    if (ModeStatus == ModeStatus.None)
                    {
                        Stop("Game over");
                        return;
                    }
                    else
                    {
                        try
                        {
                            Source = ("javascript:Logout()");

                            await Task.Delay(2500, CancellationToken.None);

                            if (botAsf.Count > 0)
                            {
                                botAsf.RemoveAt(0);
                                goFlagsCopy = goFlagsStatus;
                            }
                            else
                            {
                                Stop("Game over: Bots");
                                return;
                            }

                        }
                        catch (Exception EX)
                        {
                            Stop("Game Over: Catch");
                            return;
                        }
                    }
                }

                else if (goFlagsCopy.HasFlag(GoFlagsStatus.Wait))
                {
                    await Task.Delay(1500, CancellationToken.None);
                    continue;
                }

                else if (goFlagsCopy.HasFlag(GoFlagsStatus.Explore))
                {
                    goFlagsCopy |= GoFlagsStatus.Wait;
                    goFlagsCopy &= ~GoFlagsStatus.Explore;

                    Source = "https://store.steampowered.com/explore";
                    continue;
                }

                else if (goFlagsCopy.HasFlag(GoFlagsStatus.SteamAward))
                {
                    goFlagsCopy |= GoFlagsStatus.Wait;
                    goFlagsCopy &= ~GoFlagsStatus.SteamAward;
                    Source = "https://store.steampowered.com/steamawards";
                    continue;
                }

                else
                {
                    Stop("Error");
                    return;
                }

                //if (IsExplore && GoStatus == GoStatus.None)
                //{
                //    GoStatus = GoStatus.StartExplore;
                //    Source = "https://store.steampowered.com/explore";
                //}

                //else if (IsSteamAward && (GoStatus == GoStatus.None || GoStatus == GoStatus.FinishExplore))
                //{
                //    GoStatus = GoStatus.StartSteamAward;
                //    Source = "https://store.steampowered.com/steamawards";
                //}

                //else if(GoStatus == GoStatus.StartExplore || GoStatus == GoStatus.StartSteamAward)
                //{
                //    await Task.Delay(1500, CancellationToken.None);
                //}

                //else if (ModeStatus != ModeStatus.None && (GoStatus == GoStatus.FinishExplore || GoStatus == GoStatus.FinishSteamAward))
                //{
                //    try
                //    {
                //        Source = ("javascript:Logout()");

                //        await Task.Delay(2500, CancellationToken.None);

                //        botAsf.RemoveAt(0);
                //        GoStatus = GoStatus.None;

                //    }
                //    catch (Exception EX)
                //    {
                //        Source = ("javascript:alert('Game Over: Catch')");
                //        IsEnabled = true;
                //        GoStatus = GoStatus.None;
                //        Isfarm = false;
                //        return;
                //    }
                //}

                //else
                //{
                //    Source = ("javascript:alert('Game Over')");
                //    IsEnabled = true;
                //    GoStatus = GoStatus.None;
                //    Isfarm = false;
                //    return;
                //}
            }
        }
        #endregion

        #region Stop
        private void Stop(string text = "")
        {
            if (!string.IsNullOrEmpty(text))
            {
                Source = ($"javascript:alert('{text}')");
            }
            IsEnabled = true;
            Isfarm = false;
            return;
        }
        #endregion
    }
}
