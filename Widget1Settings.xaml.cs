using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
using TimeZoneNames;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WorldClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Widget1Settings : Page
    {
        public IDictionary<string, string> timezones;
        public IDictionary<string, string> countries = TZNames.GetCountryNames("en");


        public Widget1Settings()
        {
            this.InitializeComponent();
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values["timezones"] != null)
            {
                TimeZones.Text = settings.Values["timezones"].ToString();
            }
        }

        private void TimeZones_TextChanged(object sender, TextChangedEventArgs e)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["timezones"] = TimeZones.Text;
        }

        private void CountryChanged(object sender, SelectionChangedEventArgs e)
        {
            string country = ((KeyValuePair<string, string>)e.AddedItems[0]).Key;
            DateTimeOffset offset = DateTime.Now;
            timezones = TZNames.GetTimeZonesForCountry(country, "en", offset);
            ZonesBox.ItemsSource = timezones;
        }

        private void TimeZoneChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                KeyValuePair<string, string> pair = (KeyValuePair<string, string>)e.AddedItems[0];
                TimeZones.Text = (TimeZones.Text.Trim() + "\n" + pair.Key).Trim();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
