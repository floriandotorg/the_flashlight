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

namespace the_flashlight
{
    public partial class MainPage : PhoneApplicationPage
    {
        private VideoCamera _videoCamera;
        private VideoCameraVisualizer _videoCameraVisualizer;

        private bool ready = false;

        // Konstruktor
        public MainPage()
        {
            InitializeComponent();

            this.off_to_on.Completed += (object sender, EventArgs e) =>
            {
                ready = true;
            };
            this.on_to_off.Completed += (object sender, EventArgs e) =>
            {
                ready = true;
            };

            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
            {
                _videoCamera = new VideoCamera();

                // Event is fired when the video camera object has been initialized.
                _videoCamera.Initialized += VideoCamera_Initialized;

                // Add the photo camera to the video source
                _videoCameraVisualizer = new VideoCameraVisualizer();
                _videoCameraVisualizer.SetSource(_videoCamera);

                ready = false;
                this.off_to_on.Begin();
            }
            else
            {
                MessageBox.Show("This device does not have a flashlight. Application won't work.");
            }
        }

        private void switch_lamp()
        {


            if (_videoCamera.LampEnabled)
            {
                if (Microsoft.Devices.Environment.DeviceType != DeviceType.Emulator)
                {
                    _videoCamera.LampEnabled = false;
                    _videoCamera.StopRecording();
                }

                ready = false;
                this.on_to_off.Begin();
            }
            else
            {
                if (Microsoft.Devices.Environment.DeviceType != DeviceType.Emulator)
                {
                    _videoCamera.LampEnabled = true;
                    _videoCamera.StartRecording();
                }

                ready = false;
                this.off_to_on.Begin();
            }
        }

        void OnTouchFrameReported(object sender, TouchFrameEventArgs args)
        {
            TouchPoint primaryTouchPoint = args.GetPrimaryTouchPoint(null);

            if(ready && primaryTouchPoint != null && primaryTouchPoint.Action == TouchAction.Down)
            {
                if(primaryTouchPoint.Position.X > 420 && primaryTouchPoint.Position.Y > 740)
                {
                    ready = false;
                    NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.Relative));
                }
                else
                {
                    switch_lamp();
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ready = true;
        }

        private void VideoCamera_Initialized(object sender, EventArgs e)
        {
            _videoCamera.LampEnabled = true;
            _videoCamera.StartRecording();

            this.Dispatcher.BeginInvoke(() =>
            {
                Touch.FrameReported += OnTouchFrameReported;
            }
            );
        }
    }
}