using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LightBuzz.Archiver;
using ExceptionHelper;
using System.Threading.Tasks;
using Windows.Storage.Search;
using SharpCompress.Readers;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives;
using SharpCompress.Common;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Archiver : Page
    {
        public StorageFile storageFile { get; set; }
        public StorageFolder outputFolder { get; set; }
        public int fileCount { get; set; }
        public double entryProgress { get; set; }
        public string outputText { get; set; }
        public string outputFolderName { get; set; }
        //public string extractedFolder { get; set; }
        public IReadOnlyList<StorageFile> fileList { get; set; }

        public Archiver()
        {
            this.InitializeComponent();
            
            ProgRing.Visibility = Visibility.Collapsed;
            ProgRing.IsEnabled = false;
            ProgRing.IsActive = false;
        }

            
        private async void OpenArchive_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpen = new FileOpenPicker();
            fileOpen.ViewMode = PickerViewMode.List;
            fileOpen.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fileOpen.FileTypeFilter.Add(".zip");
            fileOpen.FileTypeFilter.Add(".7z");
            fileOpen.FileTypeFilter.Add(".tar");
            fileOpen.FileTypeFilter.Add(".rar");
            fileOpen.FileTypeFilter.Add(".gz");
            fileOpen.FileTypeFilter.Add(".xz");
            storageFile = await fileOpen.PickSingleFileAsync();
            if (storageFile == null)
            {
                Fileheader.Text = "No File Chosen";
                return;
            }
            if(Path.GetExtension(storageFile.Name).Contains(".zip") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} Zip Archive";
                outputFolderName = storageFile.Name.Replace(".zip", "");
            }
            if (Path.GetExtension(storageFile.Name).Contains(".7z") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} 7Zip Archive";
                outputFolderName = storageFile.Name.Replace(".7z", "");
            }
            if (Path.GetExtension(storageFile.Name).Contains(".tar") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} Tarball Archive";
                outputFolderName = storageFile.Name.Replace(".tar", "");
            }
            if (Path.GetExtension(storageFile.Name).Contains(".rar") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} WINRAR Archive";
                outputFolderName = storageFile.Name.Replace(".rar", "");
            }
            if (Path.GetExtension(storageFile.Name).Contains(".gz") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} GZip Archive";
                outputFolderName = storageFile.Name.Replace(".gz", "");
            }
            if (Path.GetExtension(storageFile.Name).Contains(".xz") == true)
            {
                BasicProperties archiveinfo = await storageFile.GetBasicPropertiesAsync();
                OpenArchiveHeader.Text = storageFile.Name;
                Fileheader.Text = $"{((long)archiveinfo.Size).ToFileSize()} XZ Archive";
                outputFolderName = storageFile.Name.Replace(".xz", "");
            }



        }

        private async void OutputArchive_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folder = new FolderPicker();
            folder.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folder.FileTypeFilter.Add("*");
            outputFolder = await folder.PickSingleFolderAsync();
            if (outputFolder == null)
            {
                Fileheader.Text = "No output folder selected";
                return;
            }
            SelectOutputHeader.Text = outputFolder.Path;
        }


        private async void ExtractBtn_Click(object sender, RoutedEventArgs e)
        {
           /// ProgRing.Visibility = Visibility.Visible;
           // ProgRing.IsEnabled = true;
           // ProgRing.IsActive = true;

            ///
            /// Other types of archive
            /// 
            await ExtractArchive(storageFile.Path, outputFolder.Path, storageFile, outputFolder);
        


           //     ProgRing.Visibility = Visibility.Collapsed;
           //ProgRing.IsEnabled = false;
           //ProgRing.IsActive = false;
        }

        private void Reader_EntryExtractionProgress(object sender, ReaderExtractionEventArgs<IEntry> e)
        {
            entryProgress = e.ReaderProgress.PercentageReadExact;
           // Fileheader.Text = entryProgress.ToString();
        }

       /// <summary>
       /// Main Function for extracting archives
       /// </summary>
       /// <param name="Path"></param>
       /// <param name="OutPath"></param>
       /// <param name="storageFile"></param>
       /// <param name="outputFolder"></param>
       /// <returns></returns>
        private async Task ExtractArchive(string Path, string OutPath, StorageFile storageFile, StorageFolder outputFolder)
        {
            try
            {
                OutputBox.Text = $"Creating output folder in {outputFolder.Path}\\{outputFolderName}\n\n";
                await outputFolder.CreateFolderAsync(outputFolderName);

                Stream zipStream = await storageFile.OpenStreamForReadAsync();
                using (var zipArchive = ArchiveFactory.Open(zipStream))
                {
                    OutputBox.Text += "Extracting Archive\n\n";
                    //It should support 7z, zip, rar, gz, tar
                    var reader = zipArchive.ExtractAllEntries();
                    var totalExtractedFiles = 0;
                    var totalFiles = zipArchive.Entries.Where(item => !item.IsDirectory).Count();
                    var totalExtractedFolders = 0;
                    reader.EntryExtractionProgress += async (sender, e) =>
                    {
                        var entryProgress = e.ReaderProgress.PercentageReadExact;
                        //Set this progress bar value
                        await CoreApplication.MainView.CoreWindow.Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                        {
                            progress.Value = entryProgress;
                        });
                    };
                    while (reader.MoveToNextEntry())
                    {
                        
                        if (!reader.Entry.IsDirectory)
                        {
                            OutputBox.Text = $"Processing {totalExtractedFiles}/{totalFiles}:\nSize: {reader.Entry.Size.ToFileSize()} \n{reader.Entry.ToString()}\n";
                            await Task.Run(async () => {


                                await reader.WriteEntryToDirectory(await outputFolder.GetFolderAsync(outputFolderName), new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                totalExtractedFiles++;
                                
                                
                            });

                        }
                        if(reader.Entry.IsDirectory)
                        {
                            continue;
                        }
                    }


                    //totalExtractedFiles++;
                    //Output the value to text
                    outputText = $"\nExtracted {totalExtractedFiles} of {totalFiles} Files Successfully\n\n";
                }
                
                OutputBox.Text = outputText;
                OutputBox.Text += $"{storageFile.Name} has been extracted to:\n{outputFolder.Path}\\{outputFolderName}";

            }
            catch (Exception ex)
            {
                OutputBox.Text += $"\n\nAn Error Occured:\n\n{ex.Message}";
            }
        }

        /// <summary>
        /// Backup for .zip files
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="OutPath"></param>
        /// <param name="storageFile"></param>
        /// <param name="storageFolder"></param>
        /// <returns></returns>
        private async Task ExtractZip(string Path, string OutPath, StorageFile storageFile, StorageFolder storageFolder)
        {
            try
            {
                OutputBox.Text += "Creating Output Folder\n";
                await storageFolder.CreateFolderAsync($"{storageFile.Name}");
                OutputBox.Text += "Extracting Archive\n";
                await ArchiverPlus.Decompress(storageFile, await storageFolder.GetFolderAsync(storageFile.Name));
                
                
                OutputBox.Text += $"Successfully extracted to:\n {storageFolder.Path}\\{storageFile.Name}\n";



            } catch (Exception ex)
            {
                OutputBox.Text = GlobalStrings.Logger;
                Exceptions.ThrownExceptionError(ex);
                ProgRing.Visibility = Visibility.Collapsed;
                ProgRing.IsEnabled = false;
                ProgRing.IsActive = false;

            }
            
        }

        
    }
}
