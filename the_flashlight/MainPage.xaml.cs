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
using WP7Contrib.View.Transitions.Animation;

namespace the_flashlight
{
    public partial class MainPage : AnimatedBasePage
    {
        private VideoCamera _videoCamera;
        private VideoCameraVisualizer _videoCameraVisualizer;

        // Konstruktor
        public MainPage()
        {
            (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.White;
            (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color = Colors.Black;

            InitializeComponent();

            AnimationContext = LayoutRoot;

            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
            {
                _videoCamera = new VideoCamera();

                // Event is fired when the video camera object has been initialized.
                _videoCamera.Initialized += VideoCamera_Initialized;

                // Add the photo camera to the video source
                _videoCameraVisualizer = new VideoCameraVisualizer();
                _videoCameraVisualizer.SetSource(_videoCamera);
            }
            else
            {
                MessageBox.Show("This device does not have a flashlight. Application won't work.");
            }

            var preload = new InfoPage();
        }

        private void VideoCamera_Initialized(object sender, EventArgs e)
        {
            _videoCamera.LampEnabled = true;
            _videoCamera.StartRecording();

            this.Dispatcher.BeginInvoke ( () =>
                {
                    this.image_on.Visibility = Visibility.Visible;
                }
            );
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
    }
}