﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ExceptionHelper
{
    public static class Exceptions
    {
        public static async void ThrownExceptionError(System.Exception ex)
        {

            var ThrownException = new MessageDialog(ex.Message + "\n" + "\n" + ex.ToString());
            ThrownException.Commands.Add(new UICommand("Close"));
            await ThrownException.ShowAsync();
        }

        public static async Task URLNameBlank(string textbox, Exception ex)
        {
            var UrlException = new MessageDialog($"{textbox} is blank! Please enter a URL\n\n {ex}");
            UrlException.Commands.Add(new UICommand("Close"));
            await UrlException.ShowAsync();
        }
    }
}
