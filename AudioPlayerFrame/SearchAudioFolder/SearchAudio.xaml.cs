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

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for SearchAudio.xaml
    /// </summary>
    public partial class SearchAudio : Window
    {
        List<SearchResultTrack> tracksList = new List<SearchResultTrack>();
        public SearchAudio()
        {
            InitializeComponent();
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

        private void searchCommence_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                tracksList.Add(new SearchResultTrack("real folks blues", "05:22", 0));
                tracksList.Add(new SearchResultTrack("knock a little harder", "04:32", 1));
                tracksList.Add(new SearchResultTrack("shine a little light", "02:56", 2));
                tracksList.Add(new SearchResultTrack("don't bother none", "06:41", 3));
                searchResults.ItemsSource = tracksList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        //Так как невозможно привязать событие к методу привязанного класса, реализовано через глобальный обработчик
        //Отправитель события преобразовывается в элемент фреймворка
        //Из него получается контекст данных, преобразовывается в нужный класс, вызывается метод класса
        private void PlayButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                ((SearchResultTrack)fe.DataContext).playMouseUp();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void StopButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }

    public class SearchResultTrack
    {
        public string name { get; set; }
        public string duration { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public SearchResultTrack(string name, string duration, int id)
        {
            this.name = name;
            this.duration = duration;
            this.id = id;
        }

        public void playMouseUp()
        {
            try
            {
                Debug.WriteLine("Нажат плей на " + this.name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }
}
