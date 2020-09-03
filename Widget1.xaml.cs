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
using Microsoft.Gaming.XboxGameBar;
using System.Diagnostics;
using Microsoft.Gaming.XboxGameBar.Authentication;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TimeZoneNames;
using TimeZoneConverter;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WorldClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Widget1 : Page
    {
        private XboxGameBarWidget widget = null;
        private XboxGameBarWidgetControl widgetControl = null;
        DispatcherTimer clockTimer;
        SolidColorBrush current = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(33, 255, 255, 255));
        SolidColorBrush transparent = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(0, 0, 0, 0));
        private string timezones;

        private class Clock : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public TimeZoneInfo timezone { get; set; }
            private string _time;
            public string time
            {
                get
                {
                    return _time;
                }
                set
                {
                    if (_time != value)
                    {
                        _time = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("time"));
                        }
                    }
                }
            }
            private string _date;
            public string date
            {
                get
                {
                    return _date;
                }
                set
                {
                    if (_date != value)
                    {
                        _date = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("date"));
                        }
                    }
                }
            }
            public string iana { get; set; }
            public string name { get; set; }
            public string abbr { get; set; }
            public string city { get; set; }
            public string diff { get; set; }
            public SolidColorBrush color { get; set; }
            public void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public static string ToAbbreviation(TimeZoneInfo theTimeZone)
        {

            string timeZoneString = theTimeZone.StandardName;
            string result = string.Concat(System.Text.RegularExpressions.Regex
               .Matches(timeZoneString, "[A-Z]")
               .OfType<System.Text.RegularExpressions.Match>()
               .Select(match => match.Value));

            return result;
        }
        public static string ToCity(string theTimeZone)
        {
            return theTimeZone.Split("/")[1].Replace("_", " ");            
        }
        public static string ToDiff(TimeZoneInfo theTimeZone)
        {
            int offset = theTimeZone.GetUtcOffset(DateTime.Now).Hours;
            if (offset < 0)
                return "UTC" + offset.ToString();
            else if (offset > 0)
                return "UTC+" + offset.ToString();
            else
                return "UTC";
        }

        public void UpdateTimeZones()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values["timezones"] != null && settings.Values["timezones"].ToString() != timezones)
            {
                timezones = settings.Values["timezones"].ToString();
                SetTimeZones(timezones);
            }
        }

        private ObservableCollection<Clock> clocks;

        public void SetTimeZones(string timezones)
        {
            clocks.Clear();
            string[] tzs = timezones.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string tz in tzs)
            {
                Clock clock = new Clock();
                try
                {
                    string name = TZConvert.IanaToWindows(tz);
                    clock.iana = tz;
                    clock.timezone = TimeZoneInfo.FindSystemTimeZoneById(name);
                    clock.name = TZNames.GetNamesForTimeZone(tz, "en").Generic.Replace(" Time", "").Replace("Coordinated ", "");
                    clock.abbr = TZNames.GetAbbreviationsForTimeZone(tz, "en").Generic;
                    clock.city = ToCity(tz);
                    clock.diff = ToDiff(clock.timezone);
                    clock.color = clock.timezone.GetUtcOffset(DateTime.Now) == TimeZoneInfo.Local.GetUtcOffset(DateTime.Now) ? current : transparent;
                    UpdateClock(clock);
                    clocks.Add(clock);
                }
                catch(Exception e)
                {
                    
                }
            }
            if (widget != null)
            {
                Windows.Foundation.Size size;
                size.Height = 160;
                size.Width = 160 * clocks.Count;
                widget.TryResizeWindowAsync(size);
            }
        }

        public Widget1()
        {
            this.InitializeComponent();

            clocks = new ObservableCollection<Clock>();
            timezones = "";

            clockTimer = new DispatcherTimer();
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Interval = TimeSpan.FromMilliseconds(1000);
            clockTimer.Start();

            UpdateClocks();
            UpdateTimeZones();
        }
        private void ClockTimer_Tick(object sender, object e)
        {
            UpdateTimeZones();
            UpdateClocks();
        }

        private void UpdateClock(Clock clock)
        {
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clock.timezone);
            clock.time = time.ToString(@"HH\:mm");
            clock.date = time.ToString(@"ddd d\.M\.");
        }

        private void UpdateClocks()
        {
            foreach(Clock clock in clocks)
            {
                UpdateClock(clock);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            widget = e.Parameter as XboxGameBarWidget;
            widgetControl = new XboxGameBarWidgetControl(widget);

            if (widget != null)
            {
                Windows.Foundation.Size size;
                size.Height = 160;
                size.Width = 160 * clocks.Count;
                widget.TryResizeWindowAsync(size);
            }

            // Hook up events for when the ui is updated.
            widget.SettingsClicked += Widget_SettingsClicked;
            widget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;

            SetBackgroundOpacity();
        }

        private async void Widget_SettingsClicked(XboxGameBarWidget sender, object args)
        {
            await widget.ActivateSettingsAsync();
        }

        private async void Widget_RequestedOpacityChanged(XboxGameBarWidget sender, object args)
        {
            SetBackgroundOpacity();
        }

        private async void SetBackgroundOpacity()
        {
            await Page_Clock.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // adjust the opacity of your background as appropriate
                Background.Opacity = widget.RequestedOpacity;
            });
        }

    }
}
