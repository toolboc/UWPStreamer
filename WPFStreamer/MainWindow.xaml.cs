using GalaSoft.MvvmLight.Threading;
using InputRedirectionNTR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWPStreamer.Services;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace WPFStreamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isHelpOpen = false;
        bool isSettingsOpen = false;

        NTR ntr;
        int visualState = 0;
        int rotation = 0;

        CancellationTokenSource tokenSource;
        CancellationToken ct;
        NTRInputRedirection ntrInputRedirection;
        Task ntrInputRedirectionTask;

        public MainWindow()
        {
            DispatcherHelper.Initialize();

            InitializeComponent();

            ntr = new NTR();
            DataContext = ntr;

            InitRemotePlay();
            
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(ct.CanBeCanceled)
                tokenSource.Cancel();

            ntr.disconnect();
            App.ntrClient.disconnect();
        }

        private async void InitRemotePlay()
        {
            //ProgressBar.IsEnabled = true;

            var ip = "192.168.1.145";//Properties.Settings.Default["ip"].ToString();
            //var priorityMode = Int32.Parse(localSettings.Values["priorityMode"].ToString());
            //var priorityFactor = Int32.Parse(localSettings.Values["priorityFactor"].ToString());
            //var quality = Int32.Parse(localSettings.Values["quality"].ToString());
            //var qosValue = Int32.Parse(localSettings.Values["qosValue"].ToString());

            try
            {
                await ntr.InitRemoteplay(ip);
            }
            catch (Exception e)
            {
                //var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to connect to NTR Debugger on: \n" + ip, "Connection Error");
                //ProgressBar.IsEnabled = false;
                //await messageDialog.ShowAsync();
                return;
            }

            try
            {
                ntr.NTRRemoteplayConnect(ip);
            }
            catch (Exception e)
            {
                //var messageDialog = new Windows.UI.Popups.MessageDialog("Error while streaming to remote 3DS on: \n" + ip, "Stream Interuppted");
                //await messageDialog.ShowAsync();
            }
            finally
            {
                startNTRinputRedirection(ip);
            }

            //ProgressBar.IsEnabled = false;
        }

        private void startNTRinputRedirection(string ip)
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
                ntrInputRedirection = new NTRInputRedirection(ip);

                //bottomCommandBar.GotFocus += bottomCommandBar_GotFocus;
                //bottomCommandBar.LostFocus += bottomCommandBar_LostFocus;
                //bottomCommandBar.Opening += bottomCommandBar_Opening;
                //bottomCommandBar.Closed += bottomCommandBar_Closed;
                //helpPopup.Opened += helpPopup_Opened;

                ntrInputRedirectionTask = new Task(() => { while (true) ntrInputRedirection.ReadMain(); }, ct);

                ntrInputRedirection.CheckConnection();
                ntrInputRedirectionTask.Start();
            }
            catch (Exception e)
            {
                //var ip = localSettings.Values["ip"].ToString();
                //var messageDialog = new Windows.UI.Popups.MessageDialog("Error Initiating NTR Input Redirection on: \n" + ip, "Input Redirection Error");
            }
        }

        private async void settingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void HideSettings()
        {
            if (isSettingsOpen)
            {
                Storyboard sb = Resources["sbHideTopMenu"] as Storyboard;
                sb.Begin(pnlTopMenu);
                isSettingsOpen = false;
            }
        }

        private void ShowSettings()
        {
            if (!isSettingsOpen)
            {
                Storyboard sb = Resources["sbShowTopMenu"] as Storyboard;
                sb.Begin(pnlTopMenu);
                isSettingsOpen = true;
            }
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            rotation = (rotation + 90) % 360;

            var c = new RotateTransform();
            c.Angle = rotation;
            screensGrid.RenderTransform = c;
        }

        private void displayButton_Click(object sender, RoutedEventArgs e)
        {
      
            visualState++;
            switch (visualState)
            {
                //both
                case 0:
                    Screen1.Visibility = Visibility.Visible;
                    Grid.SetRow(Screen1, 0);
                    Grid.SetRowSpan(Screen1, 1);
                    Screen0.Visibility = Visibility.Visible;
                    Grid.SetRow(Screen0, 1);
                    Grid.SetRowSpan(Screen0, 1);
                    break;
                //top
                case 1:
                    Screen1.Visibility = Visibility.Visible;
                    Grid.SetRowSpan(Screen1, 2);
                    Screen0.Visibility = Visibility.Collapsed;
                    break;
                //bottom
                case 2:
                    Screen0.Visibility = Visibility.Visible;
                    Grid.SetRow(Screen0, 0);
                    Grid.SetRowSpan(Screen0, 2);
                    Screen1.Visibility = Visibility.Collapsed;
                    visualState = -1;
                    break;

            }
        }

        private void fullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;

            if (WindowStyle == WindowStyle.None)
                WindowStyle = WindowStyle.SingleBorderWindow;
            else
                WindowStyle = WindowStyle.None;
        }

        private void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (toolBar.Visibility == Visibility.Visible)
            {
                toolBar.Visibility = Visibility.Collapsed;
            }
            else
                toolBar.Visibility = Visibility.Visible;

        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isHelpOpen)
            {
                Storyboard sb = Resources["sbHideRightMenu"] as Storyboard;
                sb.Begin(pnlRightMenu);
                isHelpOpen = false;
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = Resources["sbShowRightMenu"] as Storyboard;
            sb.Begin(pnlRightMenu);
            isHelpOpen = true;
        }

        private void settingsOK_Click(object sender, RoutedEventArgs e)
        {
            HideSettings();
        }
        private void settingsCancel_Click(object sender, RoutedEventArgs e)
        {
            HideSettings();
        }
    }
}
