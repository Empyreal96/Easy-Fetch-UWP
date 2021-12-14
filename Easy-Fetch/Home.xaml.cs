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
using Octokit;
using System.Threading;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Windows.UI.Xaml.Controls.Page
    {
        public static string CurrentBuildVersion = "1.13.15-prerelease";
        public static string PreviousBuildVersion = "1.13.14-prerelease";
        public static string UploadedFileName = "Easy-Fetch_1.13.15.0_Debug_Test.zip";
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;

        BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        public Home()
        {
            this.InitializeComponent();
            DLUpdate.IsEnabled = true;
            DLUpdate.Visibility = Visibility.Visible;
            //CheckForUpdate();

            HomePage.Text = $"[Version: {CurrentBuildVersion}]\n"; 
            HomePage.Text += "This is in ongoing development, suggestions and feedback are welcome. Features and UI not final" + "\n\n";
            HomePage.Text += "A simple tool to help users:" + "\n"
                           + "• Use Windows Device Portal to Install Apps etc." + "\n" 
                           + "• Search for FFU Files and Download" + "\n"
                           + "• Download Update Cabs for W10M" + "\n"
                           + "• Search and Download Appx files from MS Store" + "\n"
                           + "• Download Files and Youtube Videos" + "\n"
                           + "• Extract Archives easily";

        }

        private async void CheckForUpdate()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Easy-Fetch-UWP"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Empyreal96", "Easy-Fetch-UWP");
            var latestRelease = releases[0];
            if (latestRelease.Assets != null && latestRelease.Assets.Count > 0)
            {


                var UpdateAvailable = new MessageDialog("An update is available");
                UpdateAvailable.Commands.Add(new UICommand("Close"));
                await UpdateAvailable.ShowAsync();
                DLUpdate.IsEnabled = true;
            }
        }

       

        private async void DLUpdate_Click(object sender, RoutedEventArgs e)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("Easy-Fetch-UWP"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Empyreal96", "Easy-Fetch-UWP");
            var latestRelease = releases[0];
            if (latestRelease.Assets != null && latestRelease.Assets.Count > 0)
            {
                if (latestRelease.TagName == CurrentBuildVersion)
                {
                    UpdateOut.Text = "No Updates Found! Check back later.";
                }
                //Test Function
               // if(latestRelease.TagName == PreviousBuildVersion)
               // {
               //     UpdateOut.Text = "You are on an unreleased build";
               // }

                else
                {
                    var updateURL = latestRelease.Assets[0].BrowserDownloadUrl;
                    string UpdateURL = $"https://github.com/Empyreal96/Easy-Fetch-UWP/releases/download/{latestRelease.TagName}/{UploadedFileName}";
                    UpdateOut.Text = $"Latest Build: {latestRelease.TagName}\n";
                    UpdateOut.Text += $"Current Build: {CurrentBuildVersion}\n";
                    UpdateOut.Text += $"An Update is available!\n";
                    
                    FolderPicker folderPicker = new FolderPicker();
                    folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                    folderPicker.ViewMode = PickerViewMode.Thumbnail;
                    folderPicker.FileTypeFilter.Add("*");
                    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                    if (folder == null)
                    {
                        return;
                    }
                    StorageFile file = await folder.CreateFileAsync($"{UploadedFileName}", CreationCollisionOption.GenerateUniqueName);

                    downloadOperation = backgroundDownloader.CreateDownload(new Uri(UpdateURL), file);
                    //Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
                    cancellationToken = new CancellationTokenSource();
                    var UpdateAvailable = new MessageDialog("Update downloaded");
                    UpdateAvailable.Commands.Add(new UICommand("Close"));
                    await UpdateAvailable.ShowAsync();
                }
               // DLUpdate.IsEnabled = true;
            } 
        }
    }
    
}
