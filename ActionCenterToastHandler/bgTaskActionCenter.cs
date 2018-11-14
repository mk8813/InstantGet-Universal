using System;
using System.Linq;
//using System.Net.Http;
using System.Xml.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;

namespace ActionCenterToastHandler
{
    public sealed class ActionCenterIntractiveToastCheck : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        //IBackgroundTaskInstance _taskInstance = null;
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        private volatile bool cancelRequested = false;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                if (cancelRequested == false)
                {


                    _deferral = taskInstance.GetDeferral();

                    taskInstance.Canceled += TaskInstance_Canceled;

                    ToastNotificationHistory th = ToastNotificationManager.History;

                    if (th.GetHistory().Where(t => t.Tag == "AgentToast").Count() == 0)
                    {

                        Sendnotidication();
                    }
                    _deferral.Complete();
                }

            }
            catch (Exception)
            {

                if (_deferral != null)
                {
                    _deferral.Complete();
                }
            }

        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            cancelRequested = true;
            cancelReason = reason;

            ToastNotificationHistory th = ToastNotificationManager.History;
            switch (reason)
            {

                case BackgroundTaskCancellationReason.IdleTask:

                    th.Remove("AgentToast");

                    break;
                case BackgroundTaskCancellationReason.SystemPolicy:

                    th.Remove("AgentToast");

                    break;
            }

            if (_deferral != null)
            {
                _deferral.Complete();
            }
        }

        public static void Sendnotidication()
        {
            var xmdock = CreateIntractiveToast();
            var toast = new ToastNotification(xmdock);
            toast.ExpirationTime = DateTime.Now.AddDays(1);
            toast.NotificationMirroring = NotificationMirroring.Allowed;
            toast.SuppressPopup = true;

            toast.Tag = "AgentToast";
            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);
            ////////////////////
        }
        public static Windows.Data.Xml.Dom.XmlDocument CreateIntractiveToast()
        {
            var xDoc = new XDocument(
               new XElement("toast", new XAttribute("launch", "interactivenotification"),
               new XElement("visual",
               new XElement("binding", new XAttribute("template", "ToastGeneric"),
               new XElement("text", ResourceLoader.GetForViewIndependentUse().GetString("PasteShareUrl"))
            )
            ),// actions  
            new XElement("actions",
            new XElement("input", new XAttribute("id", "inputurl"), new XAttribute("type", "text"), new XAttribute("placeHolderContent", "https://www.instagram.com/AbCde123"), new XAttribute("arguments", "actionUrl")),
            new XElement("action", new XAttribute("activationType", "background"), new XAttribute("imageUri", "Assets/postpone-96.png"), new XAttribute("content", ResourceLoader.GetForViewIndependentUse().GetString("PostponeButton")), new XAttribute("arguments", "postpone")),
            new XElement("action", new XAttribute("activationType", "background"), new XAttribute("imageUri", "Assets/Download-96.png"), new XAttribute("content", ResourceLoader.GetForViewIndependentUse().GetString("DownloadButton")), new XAttribute("arguments", "download")))

            ));

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

    }

}
