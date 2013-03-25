using System;
using System.Collections.Generic;
using System.Linq;
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
using utility;
using System.IO.IsolatedStorage;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace the_flashlight
{
    public partial class MainPage : AnimatedBasePage
    {
        private VideoCamera _videoCamera;
        private VideoCameraVisualizer _videoCameraVisualizer;
        private Microphone _mic; //marketplace capabilities detector

        // Konstruktor
        public MainPage()
        {
            try
            {
                ThemeManager.ToDarkTheme();

                InitializeComponent();

                BuildApplicationBar();

                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    storage.DeletePath("");
                }

                (App.Current as the_flashlight.App).main_page = this;

                AnimationContext = LayoutRoot;

                turnOnFlash(false);

                var preload = new InfoPage();
            }
            catch
            {
                Application_Error();
            }
        }

        private void turnOnFlash(bool pauseMusic)
        {
            try
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
                else if (System.Environment.OSVersion.Version.Major != 7)
                {
                    this.error_txt.Text = AppResources.err_win8;
                }
                else if (!pauseMusic && Microsoft.Xna.Framework.Media.MediaPlayer.State == MediaState.Playing)
                {
                    this.error_txt.Text = AppResources.pause_music;
                    this.pauseButton.Visibility = Visibility.Visible;
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
            }
            catch
            {
                Application_Error();
            }
        }

        private void pauseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            turnOnFlash(true);
            this.error_txt.Text = "";
            this.pauseButton.Visibility = Visibility.Collapsed;
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
                    (App.Current.RootVisual as TransitionFrame).IsEnabled = true;
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
            if (_videoCamera != null)
            {
                _videoCameraVisualizer.SetSource(_videoCamera);
            }
            else
            {
                (App.Current.RootVisual as TransitionFrame).IsEnabled = true;
            }
        }

        public void Application_Deactivated()
        {
            (App.Current.RootVisual as TransitionFrame).IsEnabled = false;
            if (_videoCamera != null)
            {
                _videoCamera.StopRecording();
            }
        }

        public void Application_Error()
        {
            this.error_txt.Text = AppResources.err_unknown;
        }
    }
}