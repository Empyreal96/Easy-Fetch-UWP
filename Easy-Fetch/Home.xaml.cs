using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Cryptography.Certificates;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static Phone_Helper.MainPage;
using Windows.System;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        public Home()
        {
            this.InitializeComponent();
            HomePage.Text = "[Prerelease]" + "\n"; 
            HomePage.Text += "This is in ongoing development, suggestions and feedback are welcome. Features and UI not final" + "\n\n";
            HomePage.Text += "A simple tool to help users:" + "\n"
                           + "• Use Windows Device Portal to Install Apps etc." + "\n" 
                           + "• Search for FFU Files and Download" + "\n"
                           + "• Download Update Cabs for W10M" + "\n"
                           + "• Search and Download Appx files from MS Store" + "\n"
                           + "• Download Files and Youtube Videos";

        }

       
    }
    
}
