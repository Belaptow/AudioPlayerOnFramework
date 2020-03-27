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
        public bool isTestSoundPlaying = false;
        public WaveFileReader[] testSoundsArray = new WaveFileReader[]
        {
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_200Hz__3dBFS_3s),
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_400Hz__3dBFS_3s),
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_600Hz__3dBFS_3s),
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_800Hz__3dBFS_3s),
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_1000Hz__3dBFS_3s),
            new WaveFileReader(Properties.Resources.audiocheck_net_sin_1200Hz__3dBFS_3s)
        };

        public SampleAggregator aggregator;
        public WaveOutEvent testEvent;
        public bool firstFire = true;
        public SpectrumAnalyser spectrumAnalyser = new SpectrumAnalyser();

        public Visualizer()
        {
            InitializeComponent();
            contentPresenterSpectrum.Content = spectrumAnalyser;
#if RELEASE
            playerControlsButtonsStack.Visibility = Visibility.Hidden;
            separatorBorder.Visibility = Visibility.Hidden;
            this.Height = 120;
            contentPresenterSpectrum.Margin = new Thickness(0, 0, 0, 0);
#endif
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

        public void FftCalculatedFired(object s, FftEventArgs a)
        {
            try
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    spectrumAnalyser.Update(a.Result);
                });
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

#region Отображение кнопок-картинок
        //Увеличение/уменьшение размера изображения при наведении мышью
        private void ImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)sender).Width += 1;
            ((Image)sender).Height += 1;
        }
        private void ImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Image)sender).Width -= 1;
            ((Image)sender).Height -= 1;
        }
#endregion

        //Остановка тестового звука
        private void playerControlStop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!isTestSoundPlaying) return;

                if (MainWindow.outputDevice == null) return;

                if (MainWindow.outputDevice.PlaybackState == PlaybackState.Paused || MainWindow.outputDevice.PlaybackState == PlaybackState.Stopped)
                {
                    isTestSoundPlaying = false;
                    return;
                }

                isTestSoundPlaying = false;
                MainWindow.outputDevice.Stop();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Воспроизведение тестового трека
        private void playerControlPlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Debug.WriteLine("Вход в нажатие");
                if (MainWindow.outputDevice == null) MainWindow.outputDevice = new WaveOutEvent();
                Debug.WriteLine("Входящий девайс не равен нулу или был пересоздан");
                if (MainWindow.outputDevice.PlaybackState == PlaybackState.Paused) isTestSoundPlaying = false;
                Debug.WriteLine("isTestSoundPlaying = " + isTestSoundPlaying);
                if (isTestSoundPlaying) return;
                Debug.WriteLine("звук не проигрывается");
                if (MainWindow.outputDevice.PlaybackState == PlaybackState.Playing) return;
                Debug.WriteLine("Плеер не в процессе воспроизведения");
                isTestSoundPlaying = true;

                MainWindow.outputDevice = new WaveOutEvent();
                MainWindow.equalizer = new SampleAggregator(testSoundsArray[testSoundComboBox.SelectedIndex].ToSampleProvider(), MainWindow.bands);
                MainWindow.equalizer.NotificationCount = testSoundsArray[testSoundComboBox.SelectedIndex].WaveFormat.SampleRate / 100;
                MainWindow.equalizer.PerformFFT = true;
                MainWindow.equalizer.FftCalculated += (s, a) => MainWindow.visualizerWindow.FftCalculatedFired(s, a);
                MainWindow.outputDevice.Init(MainWindow.equalizer);
                MainWindow.outputDevice.Play();
                new Task(() =>
                {
                    
                    while (isTestSoundPlaying)
                    {
                        Dispatcher.Invoke((Action)delegate ()
                        {
                            testSoundsArray[testSoundComboBox.SelectedIndex].CurrentTime = TimeSpan.FromSeconds(testSoundsArray[testSoundComboBox.SelectedIndex].TotalTime.TotalSeconds / 2);          
                        });
                        Thread.Sleep(100);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void testSoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!isTestSoundPlaying) return;
                if (MainWindow.outputDevice.PlaybackState != PlaybackState.Playing) return;
                MainWindow.outputDevice.Stop();
                isTestSoundPlaying = false;
                playerControlPlay_MouseUp(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }
}
