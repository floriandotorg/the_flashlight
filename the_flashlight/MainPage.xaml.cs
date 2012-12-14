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

namespace the_flashlight
{
    public partial class MainPage : AnimatedBasePage
    {
        private AudioVideoCaptureDevice _dev;
        private bool _locked = false;

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

                ((the_flashlight.App)App.Current).main_page = this;

                AnimationContext = LayoutRoot;

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
                else if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) && Microsoft.Devices.Environment.DeviceType != DeviceType.Emulator)
                {
                    IReadOnlyList<Windows.Foundation.Size> cap_res = AudioVideoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back);
                    Windows.Foundation.Size max_s = new Windows.Foundation.Size(double.MaxValue, double.MaxValue);

                    foreach (Windows.Foundation.Size s in cap_res)
                    {
                        if (s.Height * s.Width < max_s.Height * max_s.Width)
                        {
                            max_s = s;
                        }
                    }

                    IAsyncOperation<AudioVideoCaptureDevice> dev_async_op = AudioVideoCaptureDevice.OpenAsync(CameraSensorLocation.Back, max_s);
                    
                    dev_async_op.Completed = (IAsyncOperation<AudioVideoCaptureDevice> dev, Windows.Foundation.AsyncStatus status) =>
                        {
                            _dev = dev.GetResults();
                            _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchPower, AudioVideoCaptureDevice.GetSupportedPropertyRange(CameraSensorLocation.Back, KnownCameraAudioVideoProperties.VideoTorchPower).Min);
                            _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.On);
                        };
                }
                else
                {
                    this.error_txt.Text = AppResources.err_no_flash;
                }

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
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
        }

        public void Application_Activated()
        {
            _locked = false;

            _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.On);
        }

        public void Application_Deactivated()
        {
            if (!_locked)
            {
                _dev.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.Off);
            }
        }

        public void Application_Obscured()
        {
            _locked = true;
        }

        public void Application_Error()
        {
            this.error_txt.Text = AppResources.err_unknown;
        }
    }
}