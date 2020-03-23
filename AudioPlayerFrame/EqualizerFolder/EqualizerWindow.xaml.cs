using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for Equalizer.xaml
    /// </summary>
    public partial class EqualizerWindow : Window
    {
        public EqualizerWindow()
        {
            InitializeComponent();
            foreach(UIElement element in equalizerStackPanel.Children)
            {
                ((Slider)element).Value = double.Parse(MainWindow.settingsFile.DocumentElement["Band" + ((Slider)element).Name.Split('_')[1]].InnerText);
            }
        }

        private void BandValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                MainWindow.bands[int.Parse(((Slider)sender).Name.Split('_')[1]) - 1].Gain = Convert.ToSingle(((Slider)sender).Value);
                if (MainWindow.equalizer != null) MainWindow.equalizer.Update();
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

        private void resetEqualizer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach(UIElement element in equalizerStackPanel.Children)
                {
                    ((Slider)element).Value = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }

        private void saveEqualizer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach(UIElement element in equalizerStackPanel.Children)
                {
                    MainWindow.settingsFile.DocumentElement["Band" + ((Slider)element).Name.Split('_')[1]].InnerText = ((Slider)element).Value.ToString();
                }
                MainWindow.settingsFile.Save(MainWindow.pathToSettingsFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }
}
