using System;
using System.Windows;
using KinectSabre.Render;
using Microsoft.Kinect;

namespace KinectSabre
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        RenderGame game;
        KinectSensor kinectSensor;

        byte[] bits;
        Skeleton[] skeletons;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensor = KinectSensor.KinectSensors[0];

            try
            {
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.ColorFrameReady += kinectRuntime_VideoFrameReady;

                kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                });
                kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

                kinectSensor.Start();
            }
            catch
            {
                kinectSensor = null;
            }

            using (game = new RenderGame())
            {
                game.Exiting += game_Exiting;
                game.Run();
            }

            if (kinectSensor != null)
                kinectSensor.Stop();
        }

        void kinectRuntime_VideoFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            var frame = e.OpenColorImageFrame();

            if (frame == null)
                return;

            if (bits == null || bits.Length != frame.PixelDataLength)
            {
                bits = new byte[frame.PixelDataLength];
            }
            frame.CopyPixelDataTo(bits);

            game.UpdateColorTexture(bits);
        }

        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame frame = e.OpenSkeletonFrame();

            if (frame == null)
                return;

            Kinect.Toolbox.Tools.GetSkeletons(frame, ref skeletons);

            bool player1 = true;

            foreach (Skeleton data in skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (Joint joint in data.Joints)
                    {
                        // Quality check
                        if (joint.TrackingState != JointTrackingState.Tracked)
                            continue;

                        switch (joint.JointType)
                        {
                            case JointType.HandLeft:
                                if (player1)
                                    game.P1LeftHandPosition = joint.Position.ToVector3();
                                else
                                    game.P2LeftHandPosition = joint.Position.ToVector3();
                                break;
                            case JointType.HandRight:
                                if (player1)
                                    game.P1RightHandPosition = joint.Position.ToVector3();
                                else
                                    game.P2RightHandPosition = joint.Position.ToVector3();
                                break;
                            case JointType.WristLeft:
                                if (player1)
                                    game.P1LeftWristPosition = joint.Position.ToVector3();
                                else
                                    game.P2LeftWristPosition = joint.Position.ToVector3();
                                break;
                            case JointType.ElbowLeft:
                                if (player1)
                                    game.P1LeftElbowPosition = joint.Position.ToVector3();
                                else
                                    game.P2LeftElbowPosition = joint.Position.ToVector3();
                                break;
                            case JointType.WristRight:
                                if (player1)
                                    game.P1RightWristPosition = joint.Position.ToVector3();
                                else
                                    game.P2RightWristPosition = joint.Position.ToVector3();
                                break;
                            case JointType.ElbowRight:
                                if (player1)
                                    game.P1RightElbowPosition = joint.Position.ToVector3();
                                else
                                    game.P2RightElbowPosition = joint.Position.ToVector3();
                                break;
                        }
                    }

                    if (player1)
                    {
                        player1 = false;
                        game.P1IsActive = true;
                    }
                    else
                    {
                        game.P2IsActive = true;
                        return;
                    }
                }
            }

            if (player1)
                game.P1IsActive = false;

            game.P2IsActive = false;
        }

        void game_Exiting(object sender, EventArgs e)
        {
            Close();
        }
    }
}
