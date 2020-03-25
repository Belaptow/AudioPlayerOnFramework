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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using NAudio;
using NAudio.Wave;
using System.ComponentModel;
using System.Globalization;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using System.Threading;


namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Переменные
        Visualizer visualizerWindow;

        public List<Track> tracksList = new List<Track>(); //List of tracks in datagrid

        public List<Window> childWindows = new List<Window>(); //All side-child windows
        public List<Window> childWindowsLeft = new List<Window>(); //left ones
        public List<Window> childWindowsRight = new List<Window>(); //right ones

        public string[] playbackOptionsArray = new string[] { "loop.png", "repeat.png", "one.png", "random.png" }; //playback options, used in switch
        public int selectedPlaybackOptionIndex = 0; //current selected option index

        public string workingFolderPath = @"defaultWorkingFolder"; //path to folder with audio files
        public string exePath = AppDomain.CurrentDomain.BaseDirectory; //path to executable
        public string[] allowedExtensions = new string[] { ".wav", ".mp3" }; //filter of allowed extensions

        public static AudioFileReader mainReader; //main audio reader and stream provider
        public WaveOutEvent outputDevice; //audio output

        //Settings file to keep data between launches
        //Stores:
        //status firstLaunch = true/false
        //path to audio folder
        //Bands values, if equalizer settings were saved by user
        public static XmlDocument settingsFile = new XmlDocument(); 
        public static string pathToSettingsFile = AppDomain.CurrentDomain.BaseDirectory + @"\Settings.xml"; //path to settings file

        //Equalizer inherits from ISampleProvider, pass stream provider and bands array
        //Update on bands gain value change
        public static Equalizer equalizer; 
        //Equalizer band, add to array
        //Values to set: Bandwidth (default 0.8f), Frequency, Gain
        public static EqualizerBand[] bands;
        #endregion

        //Logic on startup
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                settingsFile.Load(pathToSettingsFile); //Load settings file
                Debug.WriteLine("\n INITIALIZING \n");
                Debug.WriteLine("\nПуть к исполняемогу файлу: " + exePath + "\n");
                if (settingsFile.DocumentElement["firstLaunch"].InnerText == "true") //if first launch
                {
                    Debug.WriteLine("\nПЕРВЫЙ ЗАПУСК\n");
                    //System.Windows.MessageBox.Show("Первый запуск");
                    settingsFile.DocumentElement["firstLaunch"].InnerText = "false"; //set first launch status to false
                    settingsFile.DocumentElement["workingFolder"].InnerText = exePath + workingFolderPath; //set working folder path to default
                    System.IO.Directory.CreateDirectory(settingsFile.DocumentElement["workingFolder"].InnerText); //Create folder if not exists
                }
                workingFolderPath = settingsFile.DocumentElement["workingFolder"].InnerText;
                RefreshDataGrid(); //refreshes datagrid
                settingsFile.Save(pathToSettingsFile); //save settings file

                bands = new EqualizerBand[] //assign equalizer bands to bands array
                {
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = float.Parse(settingsFile.DocumentElement["Band1"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = float.Parse(settingsFile.DocumentElement["Band2"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = float.Parse(settingsFile.DocumentElement["Band3"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = float.Parse(settingsFile.DocumentElement["Band4"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = float.Parse(settingsFile.DocumentElement["Band5"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = float.Parse(settingsFile.DocumentElement["Band6"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = float.Parse(settingsFile.DocumentElement["Band7"].InnerText)},
                    new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = float.Parse(settingsFile.DocumentElement["Band8"].InnerText)},
                };
                //Debug.WriteLine("\nКол-во полос = " + (bands.Length) + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //После отрисовки содержимого окна
        //creating side panels only ofter content render is essential because they assign its owner value to mainwindow
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("\n CONTENT RENDERED \n");
                CreateToggleHideableWindow(">", "Equalizer", "Эквалайзер"); //create equalizer on the right
                CreateToggleHideableWindow("<", "AudioSearch", "Поиск аудио");
                CreateToggleHideableWindow("<", "AudioEditor", "Редактор трека");
                CreateToggleHideableWindow(">", "Visualizer", "Визуализатор");
                foreach(Window w in childWindows)
                {
                    if (((ToggleHideableWindow)w).window == "Visualizer")
                    {
                        visualizerWindow = (Visualizer)((ToggleHideableWindow)w).windowToOpen;
                    }
                }
                CreateToggleHideableWindow(">", "null", "Окно 5");
                CreateToggleHideableWindow("<", "null", "Окно 6");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Обновление датагрида (избегать излишних вызовов, занимает файлы секунды на 2)
        public void RefreshDataGrid()
        {
            try
            {
                //resetting datagrid and list of tracks
                tracksDataGrid.ItemsSource = null; 
                tracksList.Clear();

                //for each file that satisfies set mask
                foreach(string file in System.IO.Directory.GetFiles(workingFolderPath).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)))
                {
                    //Debug.WriteLine("filename = " + file);
                    var newTrack = new Track //create new track based on file
                    {
                        name = System.IO.Path.GetFileName(file), //name of track with extension
                        duration = GetAudioLength(file), //audiofile length in string format mm:ss
                        extension = System.IO.Path.GetExtension(file), //extension of file
                        filePath = file, //path to file
                        audioTimeSpanLength = GetAudioTimeSpanLength(file) //audio length in seconds (TimeSpan)
                    };
                    tracksList.Add(newTrack);
                }
                tracksDataGrid.ItemsSource = tracksList; //setting ItemSource automatically rerenders datagrid
                if (tracksDataGrid.Items.Count > 0)
                {
                    tracksDataGrid.SelectedIndex = 0; //default selected track on startup is first
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Создание кнопки доп окна
        //pass:
        //arrow which will be assigned to button content, can be either "<" for left or ">" for right
        //windowName - name of window to create, based on it new side panel will be created, corresponds to class of window to create
        //if incorrect window name is passed - button will be disabled. Check is done in switch in ToggleHideadbleWindow
        //tooltip - tooltip that will be assigned to the button
        private void CreateToggleHideableWindow(string arrow, string windowName, string toolTip)
        {
            try
            {
                Window newSideBarWindow = new ToggleHideableWindow(arrow, windowName, toolTip); //Create new sidebar window
                newSideBarWindow.Show();
                if (arrow == ">") //Если окно справа
                {
                    childWindowsRight.Add(newSideBarWindow);
                }
                else if (arrow == "<") //Если окно слева
                {
                    childWindowsLeft.Add(newSideBarWindow);
                }
                childWindows.Add(newSideBarWindow);
                Window_LocationChanged(null, null); //update position of all child side windows
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        //Смена положения окна на экране
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                foreach(Window sideWindow in childWindows)
                {
                    ((ToggleHideableWindow)sideWindow).UpdatePosition(); //updates position of both button and window attached to button
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
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

        //Получение длины трека в формате мм:сс
        private string GetAudioLength(string filePath)
        {
            try
            {
                string extension = System.IO.Path.GetExtension(filePath);
                //Debug.WriteLine("Расширение: " + extension);
                //TODO: deal with this abysmal useless switch
                switch (extension)
                {
                    case ".mp3":
                        Mp3FileReader mp3Reader = new Mp3FileReader(filePath);
                        return (mp3Reader.TotalTime.ToString(@"mm\:ss"));
                    case ".wav":
                        WaveFileReader wavReader = new WaveFileReader(filePath);
                        return (wavReader.TotalTime.ToString(@"mm\:ss"));
                    default:
                        return "null";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
                return null;
            }
        }

        //Получение длины трека в формате timespan
        private TimeSpan GetAudioTimeSpanLength(string filePath)
        {
            try
            {
                string extension = System.IO.Path.GetExtension(filePath);
                //Debug.WriteLine("Расширение: " + extension);
                //TODO: deal with this horrible switch too
                switch (extension)
                {
                    case ".mp3":
                        Mp3FileReader mp3Reader = new Mp3FileReader(filePath);
                        return (mp3Reader.TotalTime);
                    case ".wav":
                        WaveFileReader wavReader = new WaveFileReader(filePath);
                        return (wavReader.TotalTime);
                    default:
                        return new TimeSpan(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
                return new TimeSpan(0);
            }
        }

        #region Управление содержимым

        //Обновление датагрида кнопкой
        private void contentControlRefreshGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RefreshDataGrid();
        }

        //Выбор рабочей директории
        private void contentControlSelectWorkingFolder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = workingFolderPath;
                fbd.Description = "Выберите папку для аудиозаписей";
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    settingsFile.DocumentElement["workingFolder"].InnerText = fbd.SelectedPath;
                    workingFolderPath = fbd.SelectedPath;
                    settingsFile.Save(pathToSettingsFile);
                    RefreshDataGrid();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Удаление файла
        private void contentControlDeleteTrack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (tracksDataGrid.SelectedItems.Count == 0)
                {
                    return;
                }
                string caption = IsMoreThanOneTrackSelected() ? "Удаление треков" : "Удаление трека";
                string message = IsMoreThanOneTrackSelected() ? "Вы уверены, что хотите удалить выбранные треки?" : "Вы уверены, что хотите удалить выбранный трек?";
                System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.YesNo;
                if (System.Windows.MessageBox.Show(message, caption, button) == MessageBoxResult.Yes)
                {
                    foreach(Track trackToDelete in tracksDataGrid.SelectedItems)
                    {
                        File.Delete(trackToDelete.filePath);
                        tracksList.Remove(trackToDelete);
                    }
                    tracksDataGrid.ItemsSource = null;
                    tracksDataGrid.ItemsSource = tracksList;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Проверка на кол-во выбранных треков
        private bool IsMoreThanOneTrackSelected()
        {
            if (tracksDataGrid.SelectedItems.Count > 1)
            {
                return true;
            }
            return false;
        }

        //Добавление файла
        private void contentControlAddTrack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Выбор файлов";
                ofd.Filter = "Audio files|*.mp3;*.wav";
                ofd.FilterIndex = 0;
                ofd.InitialDirectory = workingFolderPath;
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach(string file in ofd.FileNames)
                    {
                        try
                        {
                            File.Copy(file, workingFolderPath + @"\" + System.IO.Path.GetFileName(file));
                        }
                        catch (Exception ex) //continues through selected files if file cannot be copied
                        {
                            Debug.WriteLine("\nИСКЛЮЧЕНИЕ В ЦИКЛЕ: " + ex + "\n");
                            continue;
                        }
                    }
                    RefreshDataGrid();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        #endregion

        #region Управление воспроизведением

        //Timer runs every 100ms, updates playfinder slider value
        public void Timer()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    while(outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing) //if song is playing
                    {
                        Dispatcher.Invoke((Action)delegate()
                        {
                            playerControlSlider.Value = mainReader.CurrentTime.TotalSeconds; //Update slider value
                        });
                        Thread.Sleep(100); 
                    }
                    //when song is stopped for some reason
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        if (playerControlSlider.Value == playerControlSlider.Maximum) //check if it's ended
                        {
                            playerControlNext_MouseUp(null, null); //play next song
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("\n" + ex + "\n");
                }
            }).Start();
        }

        //Воспроизведение выбранной композиции
        private void playerControlPlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing) return; //do nothing if song is already playing
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused) //if song is paused - unpause and start timer again
                {
                    outputDevice.Play();
                    Timer();
                    return;
                }

                if (IsMoreThanOneTrackSelected()) return; //if user tries to play multiple tracks, do nothing

                outputDevice = new WaveOutEvent(); //set audio output
                mainReader = new AudioFileReader(((Track)tracksDataGrid.SelectedItem).filePath); //set reader as stream provider
                //visualizerWindow.CreateAggregator();
                equalizer = new SampleAggregator(mainReader, bands); //create new equalizer with mainReader as stream provider and bands as equalizer data
                outputDevice.Init(equalizer); //Initialize equalizer in audio output
                outputDevice.Play();
                Timer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Пауза
        private void playerControlPause_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    outputDevice.Pause();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Предыдущий трек
        private void playerControlPrev_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //check current playback option
                //names of source images are used for improved readability
                switch (playbackOptionsArray[selectedPlaybackOptionIndex]) 
                {
                    case "loop.png":
                        loopPrevTrack();
                        break;
                    case "repeat.png":
                        repeatOneTrack();
                        break;
                    case "one.png":
                        repeatOneTrackOnce();
                        break;
                    case "random.png":
                        randomTrack();
                        break;
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Следующий трек
        private void playerControlNext_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //check current playback option
                //names of source images are used for improved readability
                switch (playbackOptionsArray[selectedPlaybackOptionIndex])
                {
                    case "loop.png":
                        loopNextTrack();
                        break;
                    case "repeat.png":
                        repeatOneTrack();
                        break;
                    case "one.png":
                        repeatOneTrackOnce();
                        break;
                    case "random.png":
                        randomTrack();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Случайный трек
        public void randomTrack()
        {
            try
            {
                Random rnd = new Random();
                tracksDataGrid.SelectedIndex = rnd.Next(0, tracksDataGrid.Items.Count); //Select random number >= 0 and < tracks count and select corresponding track
                if (outputDevice != null) outputDevice.Stop(); //stop before playing next track
                outputDevice = null;
                mainReader = null;
                playerControlPlay_MouseUp(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Повторение одного трека один раз
        public void repeatOneTrackOnce()
        {
            try
            {
                playerControlSlider.Value = playerControlSlider.Minimum;
                if (outputDevice != null) outputDevice.Stop();
                outputDevice = null;
                mainReader = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Повторение одного трека
        private void repeatOneTrack()
        {
            try
            {
                playerControlSlider.Value = playerControlSlider.Minimum;
                if (outputDevice != null) outputDevice.Stop();
                outputDevice = null;
                mainReader = null;
                playerControlPlay_MouseUp(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Следующий трек в режиме цикла
        private void loopNextTrack()
        {
            try
            {
                if (tracksDataGrid.SelectedIndex + 1 >= tracksDataGrid.Items.Count)
                {
                    tracksDataGrid.SelectedIndex = 0;
                }
                else
                {
                    tracksDataGrid.SelectedIndex += 1;
                }
                if (outputDevice != null) outputDevice.Stop();
                outputDevice = null;
                mainReader = null;
                playerControlPlay_MouseUp(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Предыдущий трек в режиме цикла
        private void loopPrevTrack()
        {
            try
            {
                if (tracksDataGrid.SelectedIndex - 1 < 0)
                {
                    tracksDataGrid.SelectedIndex = tracksDataGrid.Items.Count - 1;
                }
                else
                {
                    tracksDataGrid.SelectedIndex -= 1;
                }
                if (outputDevice != null) outputDevice.Stop();
                outputDevice = null;
                mainReader = null;
                playerControlPlay_MouseUp(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Изменение значения слайдера
        private void playerControlSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (mainReader != null)
                {
                    mainReader.CurrentTime = TimeSpan.FromSeconds(((Slider)sender).Value); //Set current playback time to value on slider
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Выбор трека на датагриде
        private void tracksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                playerControlSlider.Value = playerControlSlider.Minimum;
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing) //if song is playing start playing selected song
                {
                    outputDevice.Stop();
                    outputDevice = null;
                    mainReader = null;
                    playerControlPlay_MouseUp(null, null);
                }
                else if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused) //if song is paused just reset reader and audio output
                {
                    outputDevice = null;
                    mainReader = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Изменение выбранного режима воспроизведения
        private void playerControlPlaybackMode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedPlaybackOptionIndex + 1 >= playbackOptionsArray.Length)
            {
                selectedPlaybackOptionIndex = 0;
            }
            else
            {
                selectedPlaybackOptionIndex += 1;
            }
            ((Image)sender).Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://application:,,,/AudioPlayerFrame;component/Icons/" + playbackOptionsArray[selectedPlaybackOptionIndex]));
        }

        #endregion

        //test button
        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainReader = new AudioFileReader(((Track)tracksDataGrid.SelectedItem).filePath);
                equalizer = new Equalizer(mainReader, bands);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(equalizer);
                outputDevice.Play();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }

    //Track class, stores info about track
    //TODO: move duration in string format and TimeSpan format calculation here from main class
    public class Track
    {
        public string name { get; set; }
        public string duration { get; set; }
        public string extension { get; set; }
        public string filePath { get; set; }
        public TimeSpan audioTimeSpanLength { get; set; }
    }

    

}
