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
        public List<Track> tracksList = new List<Track>();
        public List<Window> childWindows = new List<Window>();
        public List<Window> childWindowsLeft = new List<Window>();
        public List<Window> childWindowsRight = new List<Window>();
        public string workingFolderPath = @"defaultWorkingFolder";
        public string exePath = AppDomain.CurrentDomain.BaseDirectory;
        public string[] allowedExtensions = new string[] { ".wav", ".mp3" };
        public AudioFileReader mainReader;
        public WaveOutEvent outputDevice;
        public XmlDocument settingsFile = new XmlDocument();
        public string pathToSettingsFile = AppDomain.CurrentDomain.BaseDirectory + @"\Settings.xml";
        #endregion

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                settingsFile.Load(pathToSettingsFile);
                Debug.WriteLine("\n INITIALIZING \n");
                Debug.WriteLine("\nПуть к исполняемогу файлу: " + exePath + "\n");
                if (settingsFile.DocumentElement["firstLaunch"].InnerText == "true")
                {
                    
                    Debug.WriteLine("\nПЕРВЫЙ ЗАПУСК\n");
                    System.Windows.MessageBox.Show("Первый запуск");
                    settingsFile.DocumentElement["firstLaunch"].InnerText = "false";
                    settingsFile.DocumentElement["workingFolder"].InnerText = exePath + workingFolderPath;
                    System.IO.Directory.CreateDirectory(settingsFile.DocumentElement["workingFolder"].InnerText);
                }
                //workingFolderPath = exePath + workingFolderPath;
                //System.IO.Directory.CreateDirectory(workingFolderPath);
                workingFolderPath = settingsFile.DocumentElement["workingFolder"].InnerText;
                RefreshDataGrid();
                settingsFile.Save(pathToSettingsFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //После отрисовки содержимого окна
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("\n CONTENT RENDERED \n");
                CreateToggleHideableWindow(">", "Equalizer", "Эквалайзер");
                CreateToggleHideableWindow("<", "Equalizer", "Поиск аудио");
                CreateToggleHideableWindow(">", "Equalizer", "Редактор трека");
                CreateToggleHideableWindow("<", "Equalizer", "Окно 4");
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
                tracksDataGrid.ItemsSource = null;
                tracksList.Clear();
                foreach(string file in System.IO.Directory.GetFiles(workingFolderPath).Where(file => allowedExtensions.Any(file.ToLower().EndsWith)))
                {
                    //Debug.WriteLine("filename = " + file);
                    GetAudioLength(file);
                    var newTrack = new Track
                    {
                        name = System.IO.Path.GetFileName(file),
                        duration = GetAudioLength(file),
                        extension = System.IO.Path.GetExtension(file),
                        filePath = file,
                        audioTimeSpanLength = GetAudioTimeSpanLength(file)
                    };
                    //tracksDataGrid.Items.Add(newTrack);
                    tracksList.Add(newTrack);
                }
                tracksDataGrid.ItemsSource = tracksList;
                if (tracksDataGrid.Items.Count > 0)
                {
                    tracksDataGrid.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Создание кнопки доп окна
        private void CreateToggleHideableWindow(string arrow, string windowName, string toolTip)
        {
            try
            {
                Window newSideBarWindow = new ToggleHideableWindow(arrow, windowName, toolTip);
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
                Window_LocationChanged(null, null);
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
                foreach(Window w in childWindows)
                {
                    ((ToggleHideableWindow)w).UpdatePosition();
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
                //Debug.WriteLine("Реакция на нажатие кнопки удаления");
                if (tracksDataGrid.SelectedItems.Count == 0)
                {
                    //Debug.WriteLine("Не выбраны треки");
                    return;
                }
                //Debug.WriteLine("Треки выбраны");
                string caption = IsMoreThanOneTrackSelected() ? "Удаление треков" : "Удаление трека";
                string message = IsMoreThanOneTrackSelected() ? "Вы уверены, что хотите удалить выбранные треки?" : "Вы уверены, что хотите удалить выбранный трек?";
                System.Windows.MessageBoxButton button = System.Windows.MessageBoxButton.YesNo;
                if (System.Windows.MessageBox.Show(message, caption, button) == MessageBoxResult.Yes)
                {
                    //Debug.WriteLine("Начало удаления");
                    foreach(Track trackToDelete in tracksDataGrid.SelectedItems)
                    {
                        //Debug.WriteLine("Удаление " + trackToDelete.name);
                        File.Delete(trackToDelete.filePath);
                        //Debug.WriteLine("Файл удалён");
                        tracksList.Remove(trackToDelete);
                        //Debug.WriteLine("Элемент удалён");
                    }
                    tracksDataGrid.ItemsSource = null;
                    //Debug.WriteLine("Сорс = null");
                    tracksDataGrid.ItemsSource = tracksList;
                    //Debug.WriteLine("Сорс = ЛИСТ");
                    //RefreshDataGrid();
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
                //System.Windows.MessageBox.Show("Выбрано больше одного");
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
                        catch (Exception ex)
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

        public void Timer()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    while(outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Dispatcher.Invoke((Action)delegate()
                        {
                            playerControlSlider.Value = mainReader.CurrentTime.TotalSeconds;
                        });
                        Thread.Sleep(100);
                    }
                    //if (mainReader != null && outputDevice != null)
                    //{
                    //    Dispatcher.Invoke((Action)delegate ()
                    //    {
                    //        Debug.WriteLine("\nСтатус трека: " + outputDevice.PlaybackState +
                    //                "\nВремя: " + mainReader.CurrentTime.TotalSeconds +
                    //                "\nЗначение слайдера: " + playerControlSlider.Value +
                    //                "\nМаксимальное значение слайдера: " + playerControlSlider.Maximum);
                    //    });
                        
                    //}
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        if (playerControlSlider.Value == playerControlSlider.Maximum)
                        {
                            playerControlNext_MouseUp(null, null);
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
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing) return;
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    outputDevice.Play();
                    Timer();
                    return;
                }

                if (IsMoreThanOneTrackSelected()) return;

                outputDevice = new WaveOutEvent();
                mainReader = new AudioFileReader(((Track)tracksDataGrid.SelectedItem).filePath);
                outputDevice.Init(mainReader);
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

        //Следующий трек
        private void playerControlNext_MouseUp(object sender, MouseButtonEventArgs e)
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

        //Изменение значения слайдера
        private void playerControlSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (mainReader != null)
                {
                    mainReader.CurrentTime = TimeSpan.FromSeconds(((Slider)sender).Value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Изменение выбранного трека
        private void tracksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                playerControlSlider.Value = playerControlSlider.Minimum;
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    outputDevice.Stop();
                    outputDevice = null;
                    mainReader = null;
                    playerControlPlay_MouseUp(null, null);
                }
                else if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Paused)
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



        #endregion

        
    }

    public class Track
    {
        public string name { get; set; }
        public string duration { get; set; }
        public string extension { get; set; }
        public string filePath { get; set; }
        public TimeSpan audioTimeSpanLength { get; set; }
    }

    

}
