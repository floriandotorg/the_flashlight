using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Devices;
using System.Reflection;
using WP7Contrib.View.Transitions.Animation;
using Microsoft.Phone.Info;
using Windows.Phone.Media.Capture;
using Windows.Foundation;
using Microsoft.Phone.Shell;
using Windows.Phone.Devices.Power;

namespace the_flashlight
{
    public partial class MainPage : AnimatedBasePage
    {
        private AudioVideoCaptureDevice _dev;
        private System.Windows.Threading.DispatcherTimer _dt;
        private Battery _battery;
        private readonly double rect_bat_width = 0;
        private bool onInfoPage = false;

        private void FlashLight()
        {
            string deviceNameStr = "unknown device";
            object deviceName;
            if (DeviceExtendedProperties.TryGetValue("DeviceName", out deviceName))
            {
                deviceNameStr = deviceName.ToString();
            }

            if (deviceNameStr.Contains("Mozart"))
            {
                this.error_txt.Text = AppResources.err_xenon;
            }
            else if ((AudioVideoCaptureDevice.AvailableSensorLocations.Contains(CameraSensorLocation.Back)
                && AudioVideoCaptureDevice.GetSupportedPropertyValues(CameraSensorLocation.Back, KnownCameraAudioVideoProperties.VideoTorchMode).ToList().Contains((UInt32)VideoTorchMode.On))
                && Microsoft.Devices.Environment.DeviceType != DeviceType.Emulator)
            {
                IAsyncOperation<AudioVideoCaptureDevice> dev_async_op = AudioVideoCaptureDevice.OpenAsync(CameraSensorLocation.Back, AudioVideoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back).First());

                dev_async_op.Completed = (IAsyncOperation<AudioVideoCaptureDevice> dev, Windows.Foundation.AsyncStatus status) =>
                {
                    _dev = dev.GetResults();
                    _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchPower, AudioVideoCaptureDevice.GetSupportedPropertyRange(CameraSensorLocation.Back, KnownCameraAudioVideoProperties.VideoTorchPower).Max);
                    _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.On);
                };
            }
            else
            {
                this.error_txt.Text = AppResources.err_no_flash;
            }
        }

        void dt_Tick(object sender, EventArgs e)
        {
            string s = String.Format("{0:HH:mm}",DateTime.Now);
            this.time_txt.Text = s;
        }

        private void BatteryUpdate(object sender, object e)
        {
            if (!onInfoPage)
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        if (DeviceStatus.PowerSource == PowerSource.Battery || System.Diagnostics.Debugger.IsAttached)
                        {
                            SystemTray.IsVisible = false;
                            this.rect_bat.Width = rect_bat_width * ((double)_battery.RemainingChargePercent / 100.0);
                        }
                        else
                        {
                            SystemTray.IsVisible = true;
                        }

                        this.statbar.Visibility = SystemTray.IsVisible ? Visibility.Collapsed : Visibility.Visible;
                    
                        
                    }
                );
            }
        }

        private void StatbarInit()
        {
            dt_Tick(null, null);
            _dt = new System.Windows.Threading.DispatcherTimer();
            _dt.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _dt.Tick += dt_Tick;
            _dt.Start();

            _battery = Battery.GetDefault();
            BatteryUpdate(null, null);

            _battery.RemainingChargePercentChanged += BatteryUpdate;
            DeviceStatus.PowerSourceChanged += BatteryUpdate;
        }

        // Konstruktor
        public MainPage()
        {
            try
            {
                (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.White;
                (App.Current.Resources["PhoneDisabledBrush"] as SolidColorBrush).Color = Colors.White;
                (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color = Colors.Black;

                AnimationContext = LayoutRoot;

                InitializeComponent();

                (App.Current as the_flashlight.App).main_page = this;
                rect_bat_width = this.rect_bat.Width;

                FlashLight();

                BuildApplicationBar();
                StatbarInit();

                var preload = new InfoPage();
            }
            catch
            {
                Application_Error();
            }
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new Microsoft.Phone.Shell.ApplicationBar();

            ApplicationBar.ForegroundColor = System.Windows.Media.Color.FromArgb(0xFD, 0xFF, 0xFF, 0xFF);
            ApplicationBar.Opacity = 0;

            Microsoft.Phone.Shell.ApplicationBarMenuItem appBarMenuItem = new Microsoft.Phone.Shell.ApplicationBarMenuItem(AppResources.about);
            appBarMenuItem.Click += ApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        } 

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardOut)
            {
                return new SlideDownAnimator { RootElement = LayoutRoot };
            }

            if (animationType == AnimationType.NavigateBackwardOut)
            {
                return new SlideDownAnimator { RootElement = LayoutRoot };
            }

            if (animationType == AnimationType.NavigateForwardIn)
            {
                return null;
            }

            return new SlideUpAnimator { RootElement = this.LayoutRoot };
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            onInfoPage = true;
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
        }

        public void Application_Activated()
        {
            if (_dev == null)
            {
                FlashLight();
            }
        }

        public void Application_Deactivated()
        {
            if (_dev != null)
            {
                _dev.Dispose();
                _dev = null;
            }
        }

        public void Application_Error()
        {
            this.error_txt.Text = AppResources.err_unknown;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            onInfoPage = false;
            BatteryUpdate(null, null);
            base.OnNavigatedTo(e);
        }
    }
}