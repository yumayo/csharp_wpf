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
        public MainWindow()
        {
            InitializeComponent();
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
            }
            else
            {
                this.BaseGrid.Style = (Style)(this.Resources["NormalStyle"]);
                this.MaximizeButton.Content = "1";
            }
        }

        private Win32.HitTestResult? GetBorderHitTest(Point point)
        {
            if (this.WindowState != WindowState.Normal)
            {
                return null;
            }
            var top = (point.Y <= -5);
            var bottom = (point.Y >= this.Height - -5);
            var left = (point.X <= -5);
            var right = (point.X >= this.Width - -5);
            if (top == true)
            {
                if (left == true)
                {
                    return Win32.HitTestResult.HTTOPLEFT;
                }
                if (right == true)
                {
                    return Win32.HitTestResult.HTTOPRIGHT;
                }
                return Win32.HitTestResult.HTTOP;
            }
            if (bottom == true)
            {
                if (left == true)
                {
                    return Win32.HitTestResult.HTBOTTOMLEFT;
                }
                if (right == true)
                {
                    return Win32.HitTestResult.HTBOTTOMRIGHT;
                }
                return Win32.HitTestResult.HTBOTTOM;
            }
            if (left == true)
            {
                return Win32.HitTestResult.HTLEFT;
            }
            if (right == true)
            {
                return Win32.HitTestResult.HTRIGHT;
            }
            return null;
        }

        private Win32.HitTestResult? GetChromeHitTest(Point point)
        {
            var result = VisualTreeHelper.HitTest(this, point);
            if (result != null)
            {
                var button = result.VisualHit.FindVisualAncestor<Button>();
                if (button == null)
                {
                    return Win32.HitTestResult.HTCAPTION;
                }
            }
            return null;
        }

        private IntPtr? OnNcHitTest(IntPtr handle, IntPtr wParam, IntPtr lParam)
        {
            var screenPoint = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
            var clientPoint = this.PointFromScreen(screenPoint);
            var borderHitTest = this.GetBorderHitTest(clientPoint);
            if (borderHitTest != null)
            {
                return (IntPtr)borderHitTest;
            }
            var chromeHitTest = this.GetChromeHitTest(clientPoint);
            if (chromeHitTest != null)
            {
                return (IntPtr)chromeHitTest;
            }
            return null;
        }

        private IntPtr WindowProc(IntPtr handle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)Win32.WindowMessages.WM_GETMINMAXINFO)
            {
                var result = this.OnGetMinMaxInfo(handle, wParam, lParam);
                if (result != null)
                {
                    handled = true;
                    return result.Value;
                }
            }

            if (msg == (int)Win32.WindowMessages.WM_NCHITTEST)
            {
                var result = this.OnNcHitTest(handle, wParam, lParam);
                if (result != null)
                {
                    handled = true;
                    return result.Value;
                }
            }
            return IntPtr.Zero;
        }

        private IntPtr? OnGetMinMaxInfo(IntPtr handle, IntPtr wParam, IntPtr lParam)
        {
            var monitor = MonitorControl.Interop.Win32.User32.MonitorFromWindow(handle, MonitorControl.Interop.Win32.User32.MonitorFromWindowFlags.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero)
            {
                return null;
            }
            var monitorInfo = new MonitorControl.Interop.Win32.User32.MONITORINFOEX();
            if (MonitorControl.Interop.Win32.User32.GetMonitorInfo(monitor, monitorInfo) != true)
            {
                return null;
            }
            var workingRectangle = monitorInfo.rcWork;
            var monitorRectangle = monitorInfo.rcMonitor;
            var minmax = (MonitorControl.Interop.Win32.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MonitorControl.Interop.Win32.MINMAXINFO));
            minmax.MaxPosition.X = Math.Abs(workingRectangle.left - monitorRectangle.left);
            minmax.MaxPosition.Y = Math.Abs(workingRectangle.top - monitorRectangle.top);
            minmax.MaxSize.X = Math.Abs(workingRectangle.right - monitorRectangle.left);
            minmax.MaxSize.Y = Math.Abs(workingRectangle.bottom - monitorRectangle.top);
            Marshal.StructureToPtr(minmax, lParam, true);
            return IntPtr.Zero;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }
            HwndSource.FromHwnd(handle).AddHook(this.WindowProc);
        }
    }
}
