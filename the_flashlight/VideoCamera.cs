using System;
using System.Reflection;

namespace Flashlight
{
	/// <summary>
	/// Wrapper around the Microsoft.Phone.VideoCamera object in Microsoft.Phone.Media.Extended.
	/// </summary>
	public class VideoCamera
	{
		private object _videoCamera;
		private PropertyInfo _videoCameraLampEnabledPropertyInfo;
        private MethodInfo _videoCameraStartRecordingMethod;
        private MethodInfo _videoCameraStopRecordingMethod;
        private EventHandler _videoCameraInitialized;
        private EventHandler _videoCameraRecordingStarted;

		public object InnerCameraObject
		{
			get { return _videoCamera; }
		}

		public bool LampEnabled
		{
			get { return (bool)_videoCameraLampEnabledPropertyInfo.GetGetMethod().Invoke(_videoCamera, new object[0]); }
			set { _videoCameraLampEnabledPropertyInfo.GetSetMethod().Invoke(_videoCamera, new object[] { value }); }
		}

		public VideoCamera()
		{
			// Load the media extended assembly which contains the extended video camera object.
			Assembly mediaExtendedAssembly = Assembly.Load("Microsoft.Phone.Media.Extended, Version=7.0.0.0, Culture=neutral, PublicKeyToken=24eec0d8c86cda1e");

			// Get the camera source type (primary camera).
			Type cameraSourceType = mediaExtendedAssembly.GetType("Microsoft.Phone.CameraSource");
			FieldInfo field = cameraSourceType.GetField("PrimaryCamera");
			object cameraSource = Enum.ToObject(cameraSourceType, (int)field.GetValue(cameraSourceType));

			// Create the video camera object.
			Type videoCameraType = mediaExtendedAssembly.GetType("Microsoft.Phone.VideoCamera");
			ConstructorInfo videoCameraConstructor = videoCameraType.GetConstructor(new Type[] { cameraSourceType });
			_videoCamera = videoCameraConstructor.Invoke(new object[] { cameraSource });

			// Set the properties and methods.
			_videoCameraLampEnabledPropertyInfo = videoCameraType.GetProperty("LampEnabled");
            _videoCameraStartRecordingMethod = videoCameraType.GetMethod("StartRecording");
            _videoCameraStopRecordingMethod = videoCameraType.GetMethod("StopRecording");

            MethodInfo addInitializedEventMethodInfo;

			// Let the initialize event bubble through.
            _videoCameraRecordingStarted = new EventHandler(VideoCamera_RecordingStarted);
			addInitializedEventMethodInfo = videoCameraType.GetMethod("add_RecordingStarted");
            addInitializedEventMethodInfo.Invoke(_videoCamera, new object[] { _videoCameraRecordingStarted });

            _videoCameraInitialized = new EventHandler(VideoCamera_Initialized);
            addInitializedEventMethodInfo = videoCameraType.GetMethod("add_Initialized");
            addInitializedEventMethodInfo.Invoke(_videoCamera, new object[] { _videoCameraInitialized });
		}

		/// <summary>
		/// Occurs when the camera object has been initialized.
		/// </summary>
		public event EventHandler Initialized;

		/// <summary>
		/// Videoes the camera_ initialized.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The event args.</param>
		private void VideoCamera_Initialized(object sender, object eventArgs)
		{
			if (Initialized != null)
			{
				Initialized.Invoke(this, new EventArgs());
			}
		}


        public event EventHandler RecordingStarted;

        private void VideoCamera_RecordingStarted(object sender, object eventArgs)
        {
            if (RecordingStarted != null)
            {
                RecordingStarted.Invoke(this, new EventArgs());
            }
        }

		/// <summary>
		/// Start recording.
		/// </summary>
		public void StartRecording()
		{
			// Invoke the start recording method on the video camera object.
			_videoCameraStartRecordingMethod.Invoke(_videoCamera, new object[0]);
		}

        public void StopRecording()
        {
            // Invoke the start recording method on the video camera object.
            _videoCameraStopRecordingMethod.Invoke(_videoCamera, new object[0]);
        }
	}
}
