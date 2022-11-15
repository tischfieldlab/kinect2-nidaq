using System;
using System.Windows;
using System.IO;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Microsoft.Kinect;
using Sensor;
using NationalInstruments;
using NationalInstruments.DAQmx;
using Accord.Video.FFMPEG;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Metadata;
using GzTar;
using kinect2_nidaq.ViewModels;

namespace kinect2_nidaq
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        /// <summary>
        /// Startup
        /// </summary>
        public MainWindow2()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        // Solution from http://www.codeproject.com/Questions/284995/DragMove-problem-help-pls

        private bool inDrag = false;
        private Point anchorPoint;
        private bool iscaptured = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            anchorPoint = PointToScreen(e.GetPosition(this));
            inDrag = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (inDrag)
            {
                if (!iscaptured)
                {
                    CaptureMouse();
                    iscaptured = true;
                }
                Point currentPoint = PointToScreen(e.GetPosition(this));
                this.Left = this.Left + currentPoint.X - anchorPoint.X;
                this.Top = this.Top + currentPoint.Y - anchorPoint.Y;
                anchorPoint = currentPoint;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (inDrag)
            {
                inDrag = false;
                iscaptured = false;
                ReleaseMouseCapture();
            }
        }
    }
}