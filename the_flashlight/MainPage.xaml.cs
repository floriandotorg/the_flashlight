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
using Flashlight;
using System.Reflection;
using WP7Contrib.View.Transitions.Animation;
using Microsoft.Phone.Info;

namespace the_flashlight
{
    public partial class MainPage : AnimatedBasePage
    {
        private VideoCamera _videoCamera;
        private VideoCameraVisualizer _videoCameraVisualizer;

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

                string deviceNameStr = "unknown device";
                object deviceName;
                if(DeviceExtendedProperties.TryGetValue("DeviceName", out deviceName))
                {
                    deviceNameStr = deviceName.ToString(); 
                }

                if (deviceNameStr.Contains("Mozart"))
                {
                    this.error_txt.Text = AppResources.err_xenon;
                }
                else if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) && Microsoft.Devices.Environment.DeviceType != DeviceType.Emulator)
                {
                    _videoCamera = new VideoCamera();

                    // Event is fired when the video camera object has been initialized.
                    _videoCamera.Initialized += VideoCamera_Initialized;
                    _videoCamera.RecordingStarted += VideoCamera_RecordingStarted;

                    // Add the photo camera to the video source
                    _videoCameraVisualizer = new VideoCameraVisualizer();
                    _videoCameraVisualizer.SetSource(_videoCamera);
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

            ApplicationBar.ForegroundColor = Color.FromArgb(0xFD, 0xFF, 0xFF, 0xFF);
            ApplicationBar.Opacity = 0;

            Microsoft.Phone.Shell.ApplicationBarMenuItem appBarMenuItem = new Microsoft.Phone.Shell.ApplicationBarMenuItem(AppResources.about);
            appBarMenuItem.Click += ApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

        }

        private void VideoCamera_RecordingStarted(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    TransitionFrame frame = (TransitionFrame)App.Current.RootVisual;
                    frame.IsEnabled = true;
                }
            );
        }

        private void VideoCamera_Initialized(object sender, EventArgs e)
        {
            _videoCamera.LampEnabled = true;
            _videoCamera.StartRecording();
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
            _videoCameraVisualizer.SetSource(_videoCamera);
        }

        public void Application_Deactivated()
        {
            TransitionFrame frame = (TransitionFrame)App.Current.RootVisual;
            frame.IsEnabled = false;

            _videoCamera.StopRecording();
        }

        public void Application_Obscured(bool locked)
        {
            if (!locked)
            {
                _videoCamera.StopRecording();
            }
        }

        public void Application_Unobscured()
        {
            _videoCameraVisualizer.SetSource(_videoCamera);
        }

        public void Application_Error()
        {
            this.error_txt.Text = AppResources.err_unknown;
        }
    }
}