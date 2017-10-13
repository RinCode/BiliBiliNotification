using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BiliBili
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        System.Threading.Timer timer;
        public MainPage()
        {
            ApplicationView.PreferredLaunchViewSize = new Size(320, 180);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 180));
            this.InitializeComponent();
            ReadCookies();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;
            var val = settings.Values["option"];
            if (null != val)
            {
                string option = val.ToString();
            }
            base.OnNavigatedTo(e);
        }

        private void backgroundToast_Click(object sender, RoutedEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["cookies"] = cookies_text.Text;
            settings.Values["recordtime"] = 0;
            WriteCookies();
            var startTimeSpan = TimeSpan.Zero;
            try
            {
                var periodTimeSpan = TimeSpan.FromSeconds(Int32.Parse(seconds_text.Text));
                //每x秒执行一次
                timer = new System.Threading.Timer((e2) =>
                {
                    httpAsync();
                }, null, startTimeSpan, periodTimeSpan);
                start.IsEnabled = false;
                stop.IsEnabled = true;
                stop.IsFocusEngaged = false;
                //timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                DisplayErrorDialog();
            }
        }

        private void stopTimer_Click(Object sender, RoutedEventArgs e)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            start.IsEnabled = true;
            stop.IsEnabled = false;
            start.IsFocusEngaged = false;
        }

        private static void ShowToast(string xml)
        {
            // 创建XML文档
            XmlDocument doc = new XmlDocument();
            // 加载XML
            doc.LoadXml(xml);
            // 创建通知实例
            ToastNotification notification = new ToastNotification(doc);
            // 显示通知
            ToastNotifier nt = ToastNotificationManager.CreateToastNotifier();
            nt.Show(notification);
        }

        public async void httpAsync()
        {
            CookieContainer cookiecontainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            Uri uri = new Uri("https://api.bilibili.com/x/web-feed/feed?jsonp=jsonp&ps=5&type=0&_=");
            var settings = ApplicationData.Current.LocalSettings;
            string CookieStr = settings.Values["cookies"].ToString();
            string[] cookies = CookieStr.Split(';');
            foreach (string cookie in cookies)
                cookiecontainer.SetCookies(uri, cookie);
            handler.CookieContainer = cookiecontainer;
            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = client.GetAsync("https://api.bilibili.com/x/web-feed/feed?jsonp=jsonp&ps=5&type=0&_=").Result;
            string result = await response.Content.ReadAsStringAsync();

            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
            if (jo["code"].ToString() != "-101")
            {
                if (Int64.Parse(jo["data"][0]["archive"]["ctime"].ToString()) > Int64.Parse(settings.Values["recordtime"].ToString()))
                {
                    settings.Values["ctime"] = Int64.Parse(jo["data"][0]["archive"]["ctime"].ToString());
                    settings.Values["url"] = "https://www.bilibili.com/video/av" + jo["data"][0]["archive"]["aid"] + "/";
                    string name = jo["data"][0]["archive"]["owner"]["name"].ToString();
                    string title = jo["data"][0]["archive"]["title"].ToString();
                    string xml = "<toast>" +
                                "<visual>" +
                                    "<binding template=\"ToastGeneric\">" +
                                        "<text>" + name + "</text>" +
                                        "<text>" + title + "</text>" +
                                        //"<image placement = \"AppLogoOverride\" src = \"ms-appx:///Assets/StoreLogo.png\" />" +
                                        "<image placement = \"AppLogoOverride\" src = \""+jo["data"][0]["archive"]["owner"]["face"].ToString()+"\" />" +
                                       "</binding>" +
                                "</visual>" +
                                "<actions>" +
                                        "<action content = \"查看\" arguments = \"ok\" activationType=\"background\"/>" +
                                        "<action content = \"忽略\" arguments = \"cancel\" activationType=\"background\"/>" +
                                "</actions >" +
                             "</toast>";
                    ShowToast(xml);
                }
            }
            else
            {
                string xml = "<toast>" +
                            "<visual>" +
                                "<binding template=\"ToastGeneric\">" +
                                    "<text>Error</text>" +
                                    "<text>Cookies错误</text>" +
                                    "<image placement = \"AppLogoOverride\" src = \"ms-appx:///Assets/StoreLogo.png\" />" +
                                   "</binding>" +
                            "</visual>" +
                            "<actions>" +
                            "</actions >" +
                         "</toast>";
                ShowToast(xml);
            }
        }

        private async void DisplayErrorDialog()
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = "Check your cookies or time and try again.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await errorDialog.ShowAsync();
        }

        async void WriteCookies()
        {
            Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile settingFile = await storageFolder.GetFileAsync("setting.ini");
            await Windows.Storage.FileIO.WriteTextAsync(settingFile, cookies_text.Text);
        }

        async void ReadCookies()
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                Windows.Storage.StorageFile settingFile = await storageFolder.GetFileAsync("setting.ini");
                string text = await Windows.Storage.FileIO.ReadTextAsync(settingFile);
                cookies_text.Text = text;
            }
            catch
            {
                await storageFolder.CreateFileAsync("setting.ini");
            }
        }

    }

}
