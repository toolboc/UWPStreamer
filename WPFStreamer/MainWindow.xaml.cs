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
using Windows.System;
using Windows.UI.ViewManagement;

namespace WPFStreamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool connectionError = false;
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

            Init();

            Closing += MainWindow_Closing;
        }

        private void Init()
        {
            bool autoConnect = false;

            try
            {
                autoConnect = (bool)Properties.Settings.Default["autoconnect"];
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(ct.CanBeCanceled)
                tokenSource.Cancel();

            ntr.disconnect();
            App.ntrClient.disconnect();
        }

        private async void InitRemotePlay()
        {
            ProgressBar.Visibility = Visibility.Visible;

            var ip = Properties.Settings.Default["ip"].ToString();
            var priorityMode = Int32.Parse(Properties.Settings.Default["priorityMode"].ToString());
            var priorityFactor = Int32.Parse(Properties.Settings.Default["priorityFactor"].ToString());
            var quality = Int32.Parse(Properties.Settings.Default["quality"].ToString());
            var qosValue = Int32.Parse(Properties.Settings.Default["qosValue"].ToString());

            try
            {
                await ntr.InitRemoteplay(ip, priorityMode, priorityFactor, quality, qosValue);
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to NTR Debugger on: \n" + ip, "Connection Error");
                ProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                ntr.NTRRemoteplayConnect(ip);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while streaming to remote 3DS on: \n" + ip, "Stream Interrupted");
            }
            finally
            {
                startNTRinputRedirection(ip);
            }

            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void startNTRinputRedirection(string ip)
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
                ntrInputRedirection = new NTRInputRedirection(ip);

                ntrInputRedirectionTask = new Task(() => { while (true) ntrInputRedirection.ReadMain(); }, ct);

                ntrInputRedirection.CheckConnection();
                ntrInputRedirectionTask.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Initiating NTR Input Redirection on: \n" + ip, "Input Redirection Error");
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
                LoadSettings();
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
            ToggleMenu();
        }

        private void ToggleMenu()
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

            if (saveSettingsCheckBox.IsChecked == true)
                saveSettings();

            InitRemotePlay();
        }

        private void LoadSettings()
        {
            ipAddressTextBox.Text = Properties.Settings.Default["ip"].ToString();
            screenPriorityComboBox.SelectedIndex = 1 - (int)Properties.Settings.Default["priorityMode"];
            priorityFactorTextBox.Text = Properties.Settings.Default["priorityFactor"].ToString();
            imageQualityTextBox.Text = Properties.Settings.Default["quality"].ToString();
            qosValueTextBox.Text = Properties.Settings.Default["qosValue"].ToString();
            autoConnectCheckBox.IsChecked = (bool)Properties.Settings.Default["autoconnect"];
        }

        private void saveSettings()
        {
            Properties.Settings.Default["ip"] = ipAddressTextBox.Text;
            Properties.Settings.Default["priorityMode"] = 1 - screenPriorityComboBox.SelectedIndex;
            Properties.Settings.Default["priorityFactor"] = Int32.Parse(priorityFactorTextBox.Text);
            Properties.Settings.Default["quality"] = Int32.Parse(imageQualityTextBox.Text);
            Properties.Settings.Default["qosValue"] = Int32.Parse(qosValueTextBox.Text);
            Properties.Settings.Default["autoconnect"] = autoConnectCheckBox.IsChecked;

            Properties.Settings.Default.Save();
        }

        private void settingsCancel_Click(object sender, RoutedEventArgs e)
        {
            HideSettings();
        }
    }
}
