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
        }

        private void BandValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                //Debug.WriteLine("ТЕКУЩИЙ ИНДЕКС: " + (int.Parse(((Slider)sender).Name.Split('_')[1]) - 1));
                MainWindow.bands[int.Parse(((Slider)sender).Name.Split('_')[1]) - 1].Gain = Convert.ToSingle(((Slider)sender).Value);
                if (MainWindow.equalizer != null) MainWindow.equalizer.Update();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n" + ex + "\n");
            }
        }
    }
}
