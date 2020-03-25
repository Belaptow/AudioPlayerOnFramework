using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Axes;
using NAudio;
using NAudio.Dsp;
using NAudio.Wave;


namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for Visualizer.xaml
    /// </summary>
    public partial class Visualizer : Window
    {

        public SampleAggregator aggregator;
        public WaveOutEvent testEvent;

        public Visualizer()
        {
            InitializeComponent();
        }

        public void UpdatePlot()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        public void Timer()
        {
            
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    while (this.IsVisible == true)
                    {
                        if (aggregator != null)
                        {

                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("\n" + ex + "\n");
                }
            }).Start();
        }

        public void CreateAggregator()
        {
            try
            {
                Debug.WriteLine("Aggregator creation");
                aggregator = new SampleAggregator(MainWindow.mainReader, MainWindow.bands);
                aggregator.NotificationCount = MainWindow.mainReader.WaveFormat.SampleRate / 100;
                aggregator.PerformFFT = true;
                aggregator.FftCalculated += (s, a) => FftCalculatedFired(s, a);
                WaveOutEvent testEvent = new WaveOutEvent();
                testEvent.Init(aggregator);
                //testEvent.Volume = 0;
                //testEvent.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        public void FftCalculatedFired(object s, FftEventArgs a)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.IsVisible == true)
                {
                    //Timer();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }
}
