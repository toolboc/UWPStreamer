using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XAMStreamer.Services;

namespace XAMStreamer
{
    public partial class MainPage : ContentPage
    {
        //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        NTR ntr;
        int visualState = 0;
        int rotation = 0;

        public MainPage()
        {
            this.InitializeComponent();
            ntr = new NTR();
            BindingContext = ntr;
            Init();
        }

        private void Init()
        {
            //try
            //{
            //    var autoConnect = (bool)localSettings.Values["autoConnect"];
            //}
            //catch
            //{
            //    ShowSettings();
            //    return;
            //}

            //if ((bool)localSettings.Values["autoConnect"])
            //    InitRemotePlay();
            //else
                ShowSettings();
        }

        private async void ShowSettings()
        {
            //SettingsDialog dialog = new SettingsDialog();
            //await dialog.ShowAsync();

            //if (dialog.ConnectSelected)
                InitRemotePlay();
        }

        private async void InitRemotePlay()
        {
            ActivityIndicator.IsRunning = true;

            var ip = "192.168.1.147";
            var priorityMode = 1;
            var priorityFactor = 1;
            var quality = 75;
            var qosValue = 15;

            try
            {
                await ntr.InitRemoteplay(ip, priorityMode, priorityFactor, quality, qosValue);
            }
            catch (Exception e)
            {
                //var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to connect to NTR Debugger on: \n" + ip, "Connection Error");
                ActivityIndicator.IsRunning = false;
                //await messageDialog.ShowAsync();
                return;
            }

            try
            {
                ntr.NTRRemoteplayConnect();
            }
            catch (Exception e)
            {
                //var messageDialog = new Windows.UI.Popups.MessageDialog("Error while streaming to remote 3DS on: \n" + ip, "Stream Interuppted");
                //await messageDialog.ShowAsync();
            }

            ActivityIndicator.IsRunning = false;
        }
    }
}
