using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class ToastBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var settings = ApplicationData.Current.LocalSettings;
            var deferral = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                string arg = details.Argument;
                //settings.Values["option"] = arg;// 获取选择的项
                if (arg == "ok")
                {
                    settings.Values["recordtime"] = settings.Values["ctime"];
                    string uriToLaunch = settings.Values["url"].ToString();
                    // Create a Uri object from a URI string 
                    var uri = new Uri(uriToLaunch);
                    // Launch the URI
                    async void DefaultLaunch()
                    {
                        // Launch the URI
                        var success = await Windows.System.Launcher.LaunchUriAsync(uri);
                        if (success)
                        {
                            // URI launched
                        }
                        else
                        {
                            // URI launch failed
                        }
                    }
                    DefaultLaunch();
                }
            }
            deferral.Complete();
        }
    }
}
