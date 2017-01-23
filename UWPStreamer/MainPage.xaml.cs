using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPStreamer.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPStreamer
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        NTR ntr;
        int visualState = 0;
        int rotation = 0;

        public MainPage()
        {
            this.InitializeComponent();
            ntr = new NTR();
            DataContext = ntr;
            Init();
        }

        private void Init()
        {
            try
            {
                var autoConnect = (bool)localSettings.Values["autoConnect"];
            }
            catch
            {
                ShowSettings();
                return;
            }

            if ((bool)localSettings.Values["autoConnect"])
                InitRemotePlay();
            else
                ShowSettings();
        }

        private async void ShowSettings()
        {
            SettingsDialog dialog = new SettingsDialog();
            await dialog.ShowAsync();

            if (dialog.ConnectSelected)
                InitRemotePlay();
        }

        private async void InitRemotePlay()
        {
            ProgressRing.IsActive = true;

            var ip = localSettings.Values["ip"].ToString();
            var priorityMode = Int32.Parse(localSettings.Values["priorityMode"].ToString());
            var priorityFactor = Int32.Parse(localSettings.Values["priorityFactor"].ToString());
            var quality = Int32.Parse(localSettings.Values["quality"].ToString());
            var qosValue = Int32.Parse(localSettings.Values["qosValue"].ToString());

            try
            {
                await ntr.InitRemoteplay(ip, priorityMode, priorityFactor, quality, qosValue);
            }
            catch(Exception e)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to connect to NTR Debugger on: \n" +  ip, "Connection Error");
                ProgressRing.IsActive = false;
                await messageDialog.ShowAsync();
                return;
            }

            try
            {
                ntr.NTRRemoteplayConnect();
            }
            catch(Exception e)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog("Error while streaming to remote 3DS on: \n" + ip, "Stream Interuppted");
                await messageDialog.ShowAsync();
            }

            ProgressRing.IsActive = false;
        }

        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RequestedTheme = ElementTheme.Light;
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.RequestedTheme = ElementTheme.Dark;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            //ShowHelp();
            bottomCommandBar.IsOpen = false;
        }

        private async void settingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void displayButton_Click(object sender, RoutedEventArgs e)
        {
            visualState++;
            switch(visualState)
            {
                case 0:
                    VisualStateManager.GoToState(this, "BothScreens", false);
                    break;
                case 1:
                    VisualStateManager.GoToState(this, "TopScreenOnly", false);
                    break;
                case 2:
                    VisualStateManager.GoToState(this, "BottomScreenOnly", false);
                    visualState = -1;
                    break;

            }
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            rotation = (rotation + 90) % 360;
            
            var c = new CompositeTransform();
            c.Rotation = rotation;
            screensGrid.RenderTransform = c;
        }
    }
}
