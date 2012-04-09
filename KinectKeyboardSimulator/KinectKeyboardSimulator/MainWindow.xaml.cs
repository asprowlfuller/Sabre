using System;
using System.Windows;
using System.Windows.Media.Animation;
using Kinect.Toolbox;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace KinectKeyboardSimulator
{
    public partial class MainWindow
    {
        KinectSensor kinectRuntime;
        readonly DepthStreamManager depthStreamManager = new DepthStreamManager();
        SwipeGestureDetector leftHandGestureRecognizer;
        SwipeGestureDetector rightHandGestureRecognizer;
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper(30);
        bool leftHandOverHead;
        bool rightHandOverHead;
        Skeleton[] skeletons;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Animations
            Storyboard sb = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(1500));
            Storyboard.SetTarget(animation, lstWindows);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            sb.Children.Add(animation);

            sb.Begin();

            sb.Completed += sb_Completed;

            animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(800))
            {
                EasingFunction = new BackEase { Amplitude = 0.8, EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(animation, Title);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            sb.Children.Add(animation);

            sb.Begin();

            // Windows
            lstWindows.ItemsSource = WindowsFinder.NativeWindows;
        }

        void rightHandGestureRecognizer_OnGestureDetected(string gesture)
        {
            if (!leftHandOverHead)
                return;

            BringSelectedToFront();

            switch (gesture)
            {
                case "SwipeToRight":
                    SendKeys.SendWait("{PGDN}");
                    Storyboard sb = (Storyboard)FindResource("showGesture");
                    sb.Begin();
                    break;
            }
        }

        void leftHandGestureRecognizer_OnGestureDetected(string gesture)
        {
            if (!rightHandOverHead)
                return;

            BringSelectedToFront();

            switch (gesture)
            {
                case "SwipeToLeft":
                    SendKeys.SendWait("{PGUP}");
                    Storyboard sb = (Storyboard)FindResource("showGesture");
                    sb.Begin();
                    break;
            }
        }

        void BringSelectedToFront()
        {
            NativeWindow nativeWindow = lstWindows.SelectedItem as NativeWindow;

            if (nativeWindow != null)
            {
                nativeWindow.BringToFront();
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            // Init Kinect
            try
            {
                kinectRuntime = KinectSensor.KinectSensors[0];
                kinectRuntime.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
                kinectRuntime.DepthFrameReady += kinectRuntime_DepthFrameReady; 

                kinectRuntime.DepthStream.Enable();
                kinectRuntime.SkeletonStream.Enable(new TransformSmoothParameters()
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                });

                // Depth Stream
                DepthImage.DataContext = depthStreamManager;

                // Events
                leftHandGestureRecognizer = new SwipeGestureDetector();
                rightHandGestureRecognizer = new SwipeGestureDetector();
                leftHandGestureRecognizer.OnGestureDetected += leftHandGestureRecognizer_OnGestureDetected;
                rightHandGestureRecognizer.OnGestureDetected += rightHandGestureRecognizer_OnGestureDetected;


                kinectRuntime.Start();
            }
            catch
            {
                kinectRuntime = null;
            }    
        }

        void kinectRuntime_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            var frame = e.OpenDepthImageFrame();

            if (frame == null)
                return;
            
            depthStreamManager.Update(frame);
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var frame = e.OpenSkeletonFrame();
            Tools.GetSkeletons(frame, ref skeletons);

            foreach (Skeleton data in skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {

                    barycenterHelper.Add(data.Position.ToVector3(), data.TrackingId);
                    if (!barycenterHelper.IsStable(data.TrackingId))
                        return;

                    Vector3 leftHandPosition = Vector3.Zero;
                    Vector3 rightHandPosition = Vector3.Zero;
                    Vector3 headPosition = Vector3.Zero;

                    foreach (Joint joint in data.Joints)
                    {
                        // Quality check
                        if (joint.TrackingState != JointTrackingState.Tracked)
                            continue;
                        switch (joint.JointType)
                        {
                            case JointType.HandLeft:
                                leftHandPosition = joint.Position.ToVector3();
                                leftHandGestureRecognizer.Add(joint.Position, kinectRuntime);
                                break;
                            case JointType.Head:
                                headPosition = joint.Position.ToVector3();
                                break;
                            case JointType.HandRight:
                                rightHandPosition = joint.Position.ToVector3();
                                rightHandGestureRecognizer.Add(joint.Position, kinectRuntime);
                                break;
                        }
                    }
                    leftHandOverHead = Math.Abs(leftHandPosition.Y - headPosition.Y) < 0.1f;
                    rightHandOverHead = Math.Abs(rightHandPosition.Y - headPosition.Y) < 0.1f;

                    return;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectRuntime != null)
                kinectRuntime.Stop();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            lstWindows.ItemsSource = WindowsFinder.NativeWindows;
        }
    }
}
