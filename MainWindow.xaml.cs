using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Thickness normalThickness;
        Thickness maxThickness;

        public MainWindow()
        {
            InitializeComponent();

            normalThickness = this.Chrome.ResizeBorderThickness;
            maxThickness = new Thickness(0);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void SampleWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BaseGrid.Style = (Style)(this.Resources["MaximumStyle"]);
                this.MaximizeButton.Content = "2";

                // 最大化したらそもそもウィンドウのリサイズはないですし、
                // トップをドラッグして最大化の解除もできません。
                // そのため最大化したら0を代入しておきます。
                this.Chrome.ResizeBorderThickness = maxThickness;

                // 最大化したらキャプションの大きさがthicknessのぶん少なくなるため、
                // 無理やり足しています。
                this.Chrome.CaptionHeight += normalThickness.Top;
            }
            else
            {
                this.BaseGrid.Style = (Style)(this.Resources["NormalStyle"]);
                this.MaximizeButton.Content = "1";
                this.Chrome.ResizeBorderThickness = normalThickness;
                this.Chrome.CaptionHeight -= normalThickness.Top;
            }
        }
    }
}
