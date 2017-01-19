using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPStreamer.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        NTR ntr;

        public MainPage()
        {
            this.InitializeComponent();
            ntr = new NTR();
            ntr.PropertyChanged += Ntr_PropertyChanged;
            InitRemotePlay();
        }

        private void Ntr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            var source = sender as NTR;
            Screen1.Source = source.Image;
        }

        private async void InitRemotePlay()
        {
            //Change the line below to the ip of your N3Ds
            await ntr.InitRemoteplay("192.168.1.148");
            ntr.NTRRemoteplayConnect();
        }
    }
}
