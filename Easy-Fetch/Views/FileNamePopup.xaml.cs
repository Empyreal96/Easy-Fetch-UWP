using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileNamePopup : Page
    {
        public FileNamePopup()
        {
            this.InitializeComponent();
        }

        private void ConfirmName_Click(object sender, RoutedEventArgs e)
        {
            if (FileNameString.Text == "")
            {
                FileNameString.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                GlobalStrings.FileNameAccepted = false;
            }
            else
            {
                GlobalStrings.Downloaders_GeneralDownloaderFileName = FileNameString.Text;
                GlobalStrings.FileNameAccepted = true;
            }
            }
    }
}
