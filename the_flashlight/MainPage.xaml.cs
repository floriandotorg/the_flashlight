﻿using System;
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
        private bool _locked = false;
        private System.Windows.Threading.DispatcherTimer _dt;
        private Battery _battery;
        private readonly double rect_bat_width = 0;

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

        private void OnRemainingChargePercentChanged(object sender, object e)
        {
            this.rect_bat.Width = rect_bat_width * ((double)_battery.RemainingChargePercent / 100.0);
        }

        private void StatbarInit()
        {
            dt_Tick(null, null);
            _dt = new System.Windows.Threading.DispatcherTimer();
            _dt.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _dt.Tick += dt_Tick;
            _dt.Start();

            _battery = Battery.GetDefault();
            OnRemainingChargePercentChanged(null, null);
            _battery.RemainingChargePercentChanged += OnRemainingChargePercentChanged;
        }

        // Konstruktor
        public MainPage()
        {
            try
            {
                (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.White;
                (App.Current.Resources["PhoneDisabledBrush"] as SolidColorBrush).Color = Colors.White;
                (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color = Colors.Black;

                InitializeComponent();

                BuildApplicationBar();

                (App.Current as the_flashlight.App).main_page = this;

                AnimationContext = LayoutRoot;

                rect_bat_width = this.rect_bat.Width;

                FlashLight();

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

        private void StatusBar_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            SystemTray.IsVisible = !SystemTray.IsVisible;
            if (SystemTray.IsVisible)
            {
                //this.status_bar.Margin = new Thickness(this.status_bar.Margin.Left,100,this.status_bar.Margin.Right,this.status_bar.Margin.Bottom);
                this.statbar.Visibility = Visibility.Collapsed;
            }
            else
            {
                //this.status_bar.Margin = new Thickness(this.status_bar.Margin.Left, 3, this.status_bar.Margin.Right, this.status_bar.Margin.Bottom);
                this.statbar.Visibility = Visibility.Visible;
            }
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
            if (!_locked && _dev != null)
            {
                _dev.Dispose();
                _dev = null;
            }
        }

        public void Application_Obscured()
        {
            _locked = true;
        }

        public void Application_Unobscured()
        {
            _locked = false;
        }

        public void Application_Error()
        {
            this.error_txt.Text = AppResources.err_unknown;
        }
    }
}