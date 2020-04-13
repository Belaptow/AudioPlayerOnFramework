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
using System.Diagnostics;
using NAudio;
using NAudio.Wave;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net;
using System.Web;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for SearchAudio.xaml
    /// </summary>
    public partial class SearchAudio : Window
    {
        public static bool playing = false;
        //string siteSearch = "https://zaycev.net/search.html?query_search=";
        //string site = "https://zaycev.net";
        string siteSearch = "https://wwf.mp3-tut.info/search?query=";
        Dictionary<string, MouseButtonEventHandler> eventHandlers = new Dictionary<string, MouseButtonEventHandler>();
        string site = "https://wwf.mp3-tut.info";
        HtmlWeb webClient = new HtmlWeb();
        HtmlDocument document = new HtmlDocument();
        public MediaFoundationReader webReader;
        List<SearchResultTrack> tracksList = new List<SearchResultTrack>();
        SearchResultTrack currentTrack;
        public SolidColorBrush whiteBrush = Brushes.White;
        public SolidColorBrush darkGrayBrush = Brushes.DarkGray;
        bool click = false;

        public SearchAudio()
        {
            InitializeComponent();
            eventHandlers.Add("zaycev.net", searchCommenceZaycev_MouseUp);
            eventHandlers.Add("mp3-tut.info", searchCommencemp3Tut_MouseUp);
            eventHandlers.Add("youtube.com", searchCommencefreemusicdownloads_MouseUp);
            sitesChoice.ItemsSource = eventHandlers;
            sitesChoice.SelectedIndex = 2;
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

        private void searchCommenceZaycev_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (searchInput.Text == null || searchInput.Text == "") return;

                noResults.Visibility = Visibility.Hidden;
                tracksList.Clear();
                searchResults.ItemsSource = null;

                site = "https://zaycev.net";
                siteSearch = "https://zaycev.net/search.html?query_search=";

                string query = searchInput.Text.Replace(' ', '+');
                document = webClient.Load(siteSearch + query);

                if (document.DocumentNode.SelectNodes("//div[@class='musicset-track-list__items']/div") == null)
                {
                    noResults.Visibility = Visibility.Visible;
                    noResults.Text = "По запросу " + searchInput.Text + " ничего не найдено";
                    return;
                }

                searchProgress.Visibility = Visibility.Visible;
                searchProgress.Value = 0;
                searchProgress.Maximum = document.DocumentNode.SelectNodes("//div[@class='musicset-track-list__items']/div").Count;

                new Task(() =>
                {
                    foreach (HtmlNode node in document.DocumentNode.SelectNodes("//div[@class='musicset-track-list__items']/div"))
                    {
                        if (node.HasClass("track-is-banned")) continue;

                        string fullName = WebUtility.HtmlDecode(node.SelectSingleNode(".//div[@class='musicset-track__fullname']")
                                                                    .GetAttributeValue("title", ""));

                        string duration = node.SelectSingleNode(".//div[@class='musicset-track__duration']").InnerText;

                        string downloadUrl = "";
                        HtmlNode linkNode = node.SelectSingleNode(".//div[@class='musicset-track__download track-geo']")
                                                .SelectSingleNode(".//a");
                        if (linkNode != null) downloadUrl = site + linkNode.GetAttributeValue("href", "");

                        string json = GET(site + node.GetAttributeValue("data-url", ""), "");
                        Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        string urlToPlay = jsonDict["url"];

                        Dispatcher.Invoke((Action)delegate ()
                        {
                            tracksList.Add(new SearchResultTrack(fullName, duration, urlToPlay, downloadUrl, darkGrayBrush));
                            searchProgress.Value++;
                        });
                    }
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        searchResults.ItemsSource = tracksList;
                        searchProgress.Visibility = Visibility.Hidden;
                    });
                }).Start();
            }
            catch (WebException ex)
            {
                if (ex.GetBaseException().GetType() == typeof(System.Security.Authentication.AuthenticationException)) MessageBox.Show("Ошибка сертификата сайта.\nПроверьте работоспособность сайта " + site);
                Debug.WriteLine("\nВеб Исключение" + ex + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void searchCommencemp3Tut_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (searchInput.Text == null || searchInput.Text == "") return;

                siteSearch = "https://wwf.mp3-tut.info/search?query=";
                site = "https://wwf.mp3-tut.info";

                noResults.Visibility = Visibility.Hidden;
                tracksList.Clear();
                searchResults.ItemsSource = null;

                string query = searchInput.Text;

                document = webClient.Load(siteSearch + query);

                if (document.DocumentNode.SelectNodes("//div[@class='audio-list-entry-inner']") == null)
                {
                    noResults.Visibility = Visibility.Visible;
                    noResults.Text = "По запросу " + searchInput.Text + " ничего не найдено";
                    return;
                }

                searchProgress.Visibility = Visibility.Visible;
                searchProgress.Value = 0;
                searchProgress.Maximum = document.DocumentNode.SelectNodes("//div[@class='audio-list-entry-inner']").Count;

                new Task(() =>
                {
                    foreach (HtmlNode node in document.DocumentNode.SelectNodes("//div[@class='audio-list-entry-inner']"))
                    {
                        string name = WebUtility.HtmlDecode(node.SelectSingleNode(".//button[@class='play-button']")
                                                                .GetAttributeValue("data-title", ""));

                        if (name == "" || name == null) continue;

                        string duration = node.SelectSingleNode(".//div[@class='audio-duration']").InnerText;

                        string url = node.SelectSingleNode(".//button[@class='play-button']").GetAttributeValue("data-audiofile", "");
                        if (url == "" || url == null) continue;

                        string downloadUrl = url + @"&download=1";

                        Dispatcher.Invoke((Action)delegate ()
                        {
                            tracksList.Add(new SearchResultTrack(name, duration, url, downloadUrl, darkGrayBrush));
                            searchProgress.Value++;
                        });
                    }
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        searchResults.ItemsSource = tracksList;
                        searchProgress.Visibility = Visibility.Hidden;
                    });
                }).Start();
            }
            catch (WebException ex)
            {
                if (ex.GetBaseException().GetType() == typeof(System.Security.Authentication.AuthenticationException)) MessageBox.Show("Ошибка сертификата сайта.\nПроверьте работоспособность сайта " + site);
                Debug.WriteLine("\nВеб Исключение" + ex + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void searchCommencefreemusicdownloads_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (searchInput.Text == null || searchInput.Text == "") return;
                Debug.WriteLine("searchCommencefreemusicdownloads_MouseUp");

                siteSearch = "https://online.freemusicdownloads.world/results?search_query=";
                site = "https://online.freemusicdownloads.world";
                

                noResults.Visibility = Visibility.Hidden;
                tracksList.Clear();
                searchResults.ItemsSource = null;

                string query = searchInput.Text;

                document = webClient.Load(siteSearch + query);

                if (document.DocumentNode.SelectNodes("//div[@class='panel-body']") == null)
                {
                    noResults.Visibility = Visibility.Visible;
                    noResults.Text = "По запросу " + searchInput.Text + " ничего не найдено";
                    return;
                }

                searchProgress.Visibility = Visibility.Visible;
                searchProgress.Value = 0;
                searchProgress.Maximum = document.DocumentNode.SelectNodes("//div[@class='panel-body']").Count;

                new Task(() =>
                {
                    foreach (HtmlNode node in document.DocumentNode.SelectNodes("//div[@class='panel-body']"))
                    {
                        HtmlNode formNode = node.SelectSingleNode(".//form[@class='result-form']");

                        if (formNode == null) continue;

                        string name = WebUtility.HtmlDecode(formNode.SelectSingleNode(".//input[@name='title']")
                                                                    .GetAttributeValue("value", ""));
                        if (name == "" || name == null) continue;
                        Debug.WriteLine(name);

                        string duration = node.SelectSingleNode(".//small[@class='text-muted']").InnerText.Replace("\n", "");

                        string videoId = formNode.SelectSingleNode(".//input[@name='video_id']")
                                                 .GetAttributeValue("value", "");

                        HtmlDocument tempDoc = webClient.Load("https://www.320youtube.com/watch?v=" + videoId);
                        if (tempDoc.DocumentNode.SelectSingleNode("//div[@id='download']/a") == null)
                        {
                            Debug.WriteLine("Видео ИД = " + videoId);
                            Debug.WriteLine(tempDoc.Text);
                            continue;
                        }
                        string url = tempDoc.DocumentNode.SelectSingleNode("//div[@id='download']/a").GetAttributeValue("href", "");

                        string downloadUrl = url;

                        Dispatcher.Invoke((Action)delegate ()
                        {
                            tracksList.Add(new SearchResultTrack(name, duration, url, downloadUrl, darkGrayBrush));
                            searchProgress.Value++;
                        });
                    }
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        searchResults.ItemsSource = tracksList;
                        searchProgress.Visibility = Visibility.Hidden;
                    });
                }).Start();
            }
            catch (WebException ex)
            {
                if (ex.GetBaseException().GetType() == typeof(System.Security.Authentication.AuthenticationException)) MessageBox.Show("Ошибка сертификата сайта.\nПроверьте работоспособность сайта " + site);
                Debug.WriteLine("\nВеб Исключение" + ex + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void searchCommence_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                eventHandlers[((KeyValuePair<string, MouseButtonEventHandler>)sitesChoice.SelectedItem).Key](null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        public void AddValueToProgressBar(double value)
        {
            Dispatcher.Invoke((Action)delegate ()
            {
                searchProgress.Value += value;
            });
        }

        public string GET(string Url, string Data)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url + "?" + Data);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        //Так как невозможно привязать событие к методу привязанного класса, реализовано через глобальный обработчик
        //Отправитель события преобразовывается в элемент фреймворка
        //Из него получается контекст данных, преобразовывается в нужный класс, вызывается метод класса
        private void PlayButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                
                FrameworkElement fe = sender as FrameworkElement;
                if (currentTrack != null) currentTrack.brush = darkGrayBrush;
                currentTrack = ((SearchResultTrack)fe.DataContext);
                StartTrack(((SearchResultTrack)fe.DataContext).url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void StartTrack(string url)
        {
            try
            {
                if (MainWindow.outputDevice.PlaybackState == PlaybackState.Playing || MainWindow.outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    MainWindow.outputDevice.PlaybackStopped -= OutputDevice_PlaybackStopped;
                    MainWindow.outputDevice.Stop();
                }
                if (currentTrack != null) currentTrack.brush = whiteBrush; //white
                searchResults.ItemsSource = null;
                searchResults.ItemsSource = tracksList;
                webReader = new MediaFoundationReader(url);
                MainWindow.equalizer = new SampleAggregator(webReader.ToSampleProvider(), MainWindow.bands);
                MainWindow.equalizer.NotificationCount = webReader.WaveFormat.SampleRate / 100;
                MainWindow.equalizer.PerformFFT = true;
                MainWindow.equalizer.FftCalculated += (s, a) => MainWindow.visualizerWindow.FftCalculatedFired(s, a);
                MainWindow.outputDevice.Init(MainWindow.equalizer);
                MainWindow.outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
                MainWindow.outputDevice.Play();
                playing = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            currentTrack.brush = darkGrayBrush; //darkgray
            searchResults.ItemsSource = null;
            searchResults.ItemsSource = tracksList;
            playing = false;
        }

        private void StopButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (playing)
                {
                    MainWindow.outputDevice.Stop();
                    playing = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartTrack("https://downloadlagu-gratis.net/views/Gt0DwtaxCnQ");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void searchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) eventHandlers[((KeyValuePair<string, MouseButtonEventHandler>)sitesChoice.SelectedItem).Key](null, null);
        }

        private void SaveButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            SaveTrack(((SearchResultTrack)fe.DataContext).downloadUrl, ((SearchResultTrack)fe.DataContext).name);
        }

        public void SaveTrack(string url, string name)
        {
            try
            {
                Debug.WriteLine(url);
                Uri uri = new Uri(url);
                searchProgress.Visibility = Visibility.Visible;
                WebClient client = new WebClient();
                searchProgress.Maximum = 100;
                searchProgress.Value = 0;
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                Debug.WriteLine(MainWindow.workingFolderPath);
                client.DownloadFileAsync(uri, MainWindow.workingFolderPath + @"\" + name + ".mp3");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
                searchProgress.Visibility = Visibility.Hidden;
            }
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate ()
            {
                searchProgress.Visibility = Visibility.Hidden;
            });
            //MessageBox.Show("Загрузка завершена");
            ((MainWindow)this.Owner).RefreshDataGrid();
            //throw new NotImplementedException();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Dispatcher.Invoke((Action)delegate ()
            {
                searchProgress.Value = percentage;
            });
            //throw new NotImplementedException();
        }

        private void sitesChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            eventHandlers[((KeyValuePair<string, MouseButtonEventHandler>)sitesChoice.SelectedItem).Key](null, null);
        }
    }

    public class SearchResultTrack
    {
        public SolidColorBrush brush { get; set; }
        public string name { get; set; }
        public string duration { get; set; }
        public string url { get; set; }
        public string downloadUrl { get; set; }
        public bool canDownload { get; set; }
        public string toolTip { get; set; }
        public Visibility visibility { get; set; }
        public SearchResultTrack(string name, string duration, string url, string downloadUrl, SolidColorBrush brush)
        {
            this.name = name;
            this.duration = duration;
            this.url = url;
            this.downloadUrl = downloadUrl;
            this.canDownload = downloadUrl == "" ? false : true;
            this.toolTip = this.canDownload ? "Загрузка" : "Загрузка недоступна";
            this.visibility = this.canDownload ? Visibility.Visible : Visibility.Hidden;
            this.brush = brush;
        }
    }
}
