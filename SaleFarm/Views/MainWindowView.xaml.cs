using Microsoft.Web.WebView2.Core;
using SaleFarm.Enums;
using SaleFarm.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SaleFarm.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private MainWindowViewModel Vm { init; get; }

        #region ctor
        public MainWindowView(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            Vm = mainWindowViewModel;

            DataContext = Vm;

            webView.NavigationCompleted += NavigationCompleted;
        }
        #endregion

        #region NavigationCompleted
        private async void NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //if (!Vm.IsEnabled)
            //    return;

            if (webView.Source.ToString() == "https://store.steampowered.com/" || webView.Source.ToString() == "https://steamcommunity.com/")
            {
                //Vm.Source = "https://store.steampowered.com/explore";
                // OR
                //webView.CoreWebView2.Navigate("https://store.steampowered.com/explore");

                //webView.CoreWebView2.Navigate("https://store.steampowered.com/steamawards");
                //webView.CoreWebView2.Navigate("https://store.steampowered.com/login/?redir=steamawards&redir_ssl=1");

                //await webView.CoreWebView2.ExecuteScriptAsync($" Logout() ");

                return;
            }

            #region /login/
            else if (webView.Source.ToString().Contains("/login/") && Vm.ModeStatus != ModeStatus.None)
            {
                if (Vm.botAsf.Count < 1)
                {
                    return;
                }
                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('input_username').value = '{Vm.botAsf[0].Name}' ");
                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('input_password').value = '{Login2Pass(Vm.botAsf[0].Name)}' ");

                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('remember_login').checked = true ");

                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('login_btn_signin').getElementsByClassName('login_btn')[0].click() ");

                while (true)
                {
                    await Task.Delay(1000);

                    var tmpLogin2FA = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('login_twofactorauth_message_entercode_accountname').value ");
                    if (!string.IsNullOrEmpty(tmpLogin2FA))
                    {
                        string fa2 = "";

                        while (webView.Source.ToString().Contains("/login/"))
                        {
                            string fa2Token = await Vm.GetToken();
                            if (fa2Token != fa2)
                            {
                                fa2 = fa2Token;
                                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('twofactorcode_entry').value = '{fa2}' ");

                                await Task.Delay(2000);

                                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('login_twofactorauth_buttonset_entercode').getElementsByClassName('auth_button leftbtn')[0].click() ");
                            }
                            else
                            {
                                await Task.Delay(5000);
                            }
                        }

                        break;
                    }
                }

                return;
            }
            #endregion

            #region /explore
            else if (webView.Source.ToString().Contains("/explore") && Vm.IsExplore)
            {
                var checkContinue = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('discovery_queue_ctn').style.display ");
                checkContinue = DelReplace(checkContinue);

                var checkFinish = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('subtext')[0].innerText ");

                if (!string.IsNullOrEmpty(checkContinue) && checkContinue.ToLower() == "block")
                {
                    var link = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('discovery_queue_start_link').href ");

                    webView.CoreWebView2.Navigate(DelReplace(link));
                }
                else if (!string.IsNullOrEmpty(checkFinish) && (checkFinish.ToLower().Contains("завтра") || checkFinish.ToLower().Contains("tomorrow")))
                {
                    await webView.CoreWebView2.ExecuteScriptAsync($" console.log('Logout()') ");

                    await Task.Delay(2000);

                    Vm.goFlagsCopy &= ~GoFlagsStatus.Wait;
                }
                else
                {
                    await webView.CoreWebView2.ExecuteScriptAsync($" console.log('#refresh_queue_btn') ");

                    await Task.Delay(2000);

                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('refresh_queue_btn').click() ");
                }

                return;

                //await Task.Delay(15000);

                //webView.CoreWebView2.Navigate("https://store.steampowered.com/explore/startnew");
            }
            #endregion

            #region /app & /agecheck
            else if (webView.Source.ToString().Contains("/app"))
            {
                if (webView.Source.ToString().Contains("/agecheck"))
                {
                    // https://store.steampowered.com/agecheck/app/418370
                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('ageYear').getElementsByTagName('option')[0].selected = 'selected' ");
                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('view_product_page_btn').click() ");

                    // https://store.steampowered.com/app/324800/agecheck
                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementById('remember').checked = true ");
                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('btn_grey_white_innerfade')[0].click() ");
                }
                else
                {

                    await webView.CoreWebView2.ExecuteScriptAsync($" document.cookie='bGameHighlightAutoplayDisabled=true;path=/' ");

                    await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('btn_next_in_queue')[0].click() ");
                }

                return;
            }
            #endregion

            #region /steamawards
            else if (webView.Source.ToString().Contains("/steamawards") && Vm.IsSteamAward)
            {
                string xhr_request = @"
function reqXHR()
{
    let r = new XMLHttpRequest();
    r.open('GET', 'https://www.bing.com/', false);
    r.send();
    var reply = r.responseText;
    return reply;
};

reqXHR();
";
                //string raw_data = await webView.CoreWebView2.ExecuteScriptAsync(xhr_request);



                string awards = @"
function awards()
{
    var Res = false;

    for (const child of document.getElementsByClassName('category_nominations_ctn')) {
        console.log(child.innerText.toLowerCase());
    
        if (child.innerText.toLowerCase().includes('голосовать')) {
            Res = true;
        }
    
        console.log('Result: ' + Res);
    
        if(Res) {
            let n = child.getElementsByClassName('btn_vote').length;

            //Рандомное голосование SteamAwards
            let random = Math.floor(Math.random()*n);    //при n=5 -> 0..4
            console.log('All: ' + n + ', №: ' + random);

            child.getElementsByClassName('btn_vote')[random].click();

            break;
        }
    }

    return Res;
};

awards();
";
                string raw_data = await webView.CoreWebView2.ExecuteScriptAsync(awards);

                if (raw_data == "true")
                {
                    await Task.Delay(1500);

                    var login = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('btn_green_steamui btn_medium')[0].innerText ");

                    await Task.Delay(2500);

                    if (!string.IsNullOrEmpty(login) && login.ToLower().Contains("войти"))
                    {
                        await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('btn_green_steamui btn_medium')[0].click() ");
                        //webView.CoreWebView2.Navigate("https://store.steampowered.com/login/?redir=steamawards&redir_ssl=1");

                        return;
                    }

                    webView.CoreWebView2.Reload();
                }
                else
                {
                    Vm.goFlagsCopy &= ~GoFlagsStatus.Wait;
                }

                return;

            }
            #endregion

            #region promotion/cottage_2018
            else if (webView.Source.ToString().Contains("promotion/cottage_2018"))
            {
                await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('cottage_doorset')[0].click() ");

                await Task.Delay(20000);

                string raw_data = await webView.CoreWebView2.ExecuteScriptAsync($" document.getElementsByClassName('surprise_desc')[0].innerText.toLowerCase().includes('полученные сюрпризы') ");

                if (raw_data == "false")
                {
                    webView.CoreWebView2.Reload();
                }
                else
                {
                    Vm.goFlagsCopy &= ~GoFlagsStatus.Wait;
                }

                return;
            }
            #endregion
        }
        #endregion

        #region Login2Pass
        private static string Login2Pass(string l)
        {
            //login -> logi!
            l = l.Remove(l.Length - 1) + "!";
            string r = l;

            //Если логин < 8 символов (logi! -> logi!logi!)
            while (r.Length < 8)
            {
                r += l;
            }
            return r;
        }
        #endregion

        #region DelReplace
        private string DelReplace(string s)
        {
            return !string.IsNullOrEmpty(s) ? s.Replace("\"", string.Empty).Trim('"') : s;
        }
        #endregion
    }
}
