using InputRedirectionNTR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UWPStreamer.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
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

        CancellationTokenSource tokenSource;
        CancellationToken ct;
        NTRInputRedirection ntrInputRedirection;
        Task ntrInputRedirectionTask;

        public MainPage()
        {
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;

            this.InitializeComponent();

            ntr = new NTR();
            DataContext = ntr;
            Init();                    
        }

        private void CoreWindow_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            if(args.CurrentPoint.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.RightButtonReleased)
            {
                if (bottomCommandBar.ClosedDisplayMode == AppBarClosedDisplayMode.Minimal)
                {
                    bottomCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
                }
                else
                    bottomCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.GamepadRightTrigger && !bottomCommandBar.IsOpen)
            {
                if (bottomCommandBar.ClosedDisplayMode == AppBarClosedDisplayMode.Minimal)
                {
                    bottomCommandBar.IsEnabled = false;
                    bottomCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
                }
                else
                {
                    bottomCommandBar.IsEnabled = true;
                    bottomCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                }
            }

            if (args.VirtualKey == VirtualKey.GamepadLeftTrigger)
            {
                ntrInputRedirection.useGamePad = ntrInputRedirection.useGamePad ? false : true;
            }
        }

        private void Init()
        {

            bool autoConnect = false;

            try
            {
                autoConnect = (bool)localSettings.Values["autoConnect"];
            }
            catch
            {
                ShowSettings();
                return;
            }

            if (autoConnect)
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
            finally
            {
                startNTRinputRedirection();
            }

            ProgressRing.IsActive = false;
        }

        private void startNTRinputRedirection()
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
                ntrInputRedirection = new NTRInputRedirection();

                bottomCommandBar.GotFocus += bottomCommandBar_GotFocus;
                bottomCommandBar.LostFocus += bottomCommandBar_LostFocus;
                bottomCommandBar.Opening += bottomCommandBar_Opening;
                bottomCommandBar.Closed += bottomCommandBar_Closed;
                helpPopup.Opened += helpPopup_Opened;

                ntrInputRedirectionTask = new Task(() => { while (true) ntrInputRedirection.ReadMain(); }, ct);

                ntrInputRedirection.CheckConnection();
                ntrInputRedirectionTask.Start();
            }
            catch(Exception e)
            {
                var ip = localSettings.Values["ip"].ToString();
                var messageDialog = new Windows.UI.Popups.MessageDialog("Error Initiating NTR Input Redirection on: \n" + ip, "Input Redirection Error");
            }
        }

        private void FullScreenToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var appView = ApplicationView.GetForCurrentView();

            if (appView.TryEnterFullScreenMode())
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        private void FullScreenToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var appView = ApplicationView.GetForCurrentView();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHelp();
            bottomCommandBar.IsOpen = false;
        }

        private void ShowHelp()
        {
            if (!helpPopup.IsOpen)
            {
                rootPopupBorder.Width = 346;
                rootPopupBorder.Height = this.ActualHeight;
                helpPopup.HorizontalOffset = Window.Current.Bounds.Width - 346;
                helpPopup.IsOpen = true;
            }
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

        private void bottomCommandBar_GotFocus(object sender, RoutedEventArgs e)
        {
            ntrInputRedirection.useGamePad = false;
        }

        private void bottomCommandBar_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!bottomCommandBar.IsOpen && !helpPopup.IsOpen)
                ntrInputRedirection.useGamePad = true;
        }

        private void bottomCommandBar_Opening(object sender, object e)
        {
            ntrInputRedirection.useGamePad = false;
        }

        private void bottomCommandBar_Closed(object sender, object e)
        {
            if(!helpPopup.IsOpen)
                ntrInputRedirection.useGamePad = true;
        }

        private void helpPopup_Opened(object sender, object e)
        {
            ntrInputRedirection.useGamePad = false;
        }
    }
}
