using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPStreamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsDialog : ContentDialog
    {

        public bool ConnectSelected = false;

        public SettingsDialog()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;


            //defaults
            if (!localSettings.Values.ContainsKey("ip"))
                localSettings.Values["ip"] = "0.0.0.0";
            if (!localSettings.Values.ContainsKey("priorityMode"))
                localSettings.Values["priorityMode"] = 1;
            if (!localSettings.Values.ContainsKey("priorityFactor"))
                localSettings.Values["priorityFactor"] = 1;
            if (!localSettings.Values.ContainsKey("quality"))
                localSettings.Values["quality"] = 75;
            if (!localSettings.Values.ContainsKey("qosValue"))
                localSettings.Values["qosValue"] = 15;
            if (!localSettings.Values.ContainsKey("autoConnect"))
                localSettings.Values["autoConnect"] = true;

            ipAdressTextBox.Text = (string)localSettings.Values["ip"];
            screenPriorityComboBox.SelectedIndex = 1 - (int)localSettings.Values["priorityMode"];
            priorityFactorTextBox.Text = localSettings.Values["priorityFactor"].ToString();
            imageQualityTextBox.Text = localSettings.Values["quality"].ToString();
            qosValueTextBox.Text = localSettings.Values["qosValue"].ToString();
            autoConnectCheckBox.IsChecked = (bool)localSettings.Values["autoConnect"];
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(saveSettingsCheckBox.IsChecked.Value)
            {
                SaveSettings();
            }

            ConnectSelected = true;
        }

        private void SaveSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ip"] = ipAdressTextBox.Text;
            localSettings.Values["priorityMode"] = 1 - screenPriorityComboBox.SelectedIndex;
            localSettings.Values["priorityFactor"] = priorityFactorTextBox.Text;
            localSettings.Values["quality"] = imageQualityTextBox.Text;
            localSettings.Values["qosValue"] = qosValueTextBox.Text;
            localSettings.Values["autoConnect"] = autoConnectCheckBox.IsChecked;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ConnectSelected = false;
        }

    }
}
