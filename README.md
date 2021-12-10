
# Easy-Fetch for Windows 10 (Mobile)
A Simple utility to help users Manage their device with Device Portal, Download Flash Files, Update Cabs, Store Apps and more!



## What can it do?

- Access Windows Device Portal fully through 'localhost'/'127.0.0.1'
- Download WP FFU Files and Emergency files.
- Download W10M Update Cabs
- Download Apps direct from the MS store
- Download YouTube Videos (Low Quality only for now*)
- General File Downloader

*Features, Name and Layout may change over time, screenshots to be updated soon*
**Youtube: Videos fetched are set to lower quality, this is due to two limitations, 1) Need to learn how to Mux HD Audio and Video streams likely with FFMPEG. 2) The class used could be nicer, or more packages available that support W10M*



![](/screens.jpg)



## Requirements

-  Windows 10 Mobile Build 14393+

- Windows 10 Desktop Build 14393+



## Credits and Open Source Used

- Thanks to [@BAstifan](https://github.com/basharast) for help with getting the Device Portal Wrapper, FFU Downloader and StoreLib working and providing some of the info classes used.
- [WindowsDevicePortalWrapper](https://github.com/Microsoft/WindowsDevicePortalWrapper) sample from Microsoft to connect to Device Portal securely.
- Device Path info is using parts of [UWP-SystemInfoCollector](https://github.com/validvoid/UWP-SystemInfoCollector)
- [SoReFetch](https://github.com/gus33000/SoReFetch)/[WPInternals](https://github.com/ReneLergner/WPinternals) for their original "LumiaDownloadModel.cs" class
- [UWP Packages Getter](https://github.com/colinkiama/UWPPackagesGetter) for Installed Packages listing
- [StoreLib](https://github.com/StoreDev/StoreLib) for the original lib to connect to MS Servers
- [libvideo](https://github.com/omansak/libvideo) for the basic YT download functionallity

  
