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

namespace Phone_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Acknowledgements : Page
    {
        public Acknowledgements()
        {
            this.InitializeComponent();
            CreditsBox.Text = "Below are the users which assisted and whose Open Source software used in this project:";
            CreditsBox.Text += "\n" + "\n" + "Easy-Fetch - Empyreal96";
            CreditsBox.Text += "\n" + "• Help porting Various parts of code and general help - @basharast";

            CreditsBox.Text += "\n\n" + "Third Party Open Source Code used:";
            CreditsBox.Text += "\n" + "• (SoReFetch) Copyright 2021 (c) Gustave Monce";
            CreditsBox.Text += "\n" + "• (WPInternals) Copyright (c) 2018, Rene Lergner";
            
            CreditsBox.Text += "\n" + "• (StoreLib) Copyright (c) @StoreDev";
            CreditsBox.Text += "\n" + "• (SharpCompressUWP) @basharast";

            CreditsBox.Text += "\n\n Icon and Splash images: https://icons8.com";
            //CreditsBox.Text += "\n App Background by: https://unsplash.com/@iangvalerio";
         }
    }
}
