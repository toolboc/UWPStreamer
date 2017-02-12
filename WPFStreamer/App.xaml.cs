using InputRedirectionNTR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UWPStreamer.Services;

namespace WPFStreamer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NTRClient ntrClient;
        public static ScriptHelper scriptHelper;
        public static Boolean Connected = false;
    }
}
