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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWPStreamer.Services;
using Windows.Storage;

namespace WPFStreamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            ntr.PropertyChanged += Ntr_PropertyChanged;

            InitRemotePlay();

            Screen1.SourceUpdated += Screen1_SourceUpdated;
            Screen0.SourceUpdated += Screen0_SourceUpdated;
        }

        private void Screen0_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void Screen1_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }

        private void Ntr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Screen1.Source = ntr.Screen1;
        }

        private async void InitRemotePlay()
        {
            //ProgressBar.IsEnabled = true;

            var ip = "192.168.1.147";//localSettings.Values["ip"].ToString();
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
    }
}
