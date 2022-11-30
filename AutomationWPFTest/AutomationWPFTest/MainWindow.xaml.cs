using AutomationWPFTest.Hotkey;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AutomationWPFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RegisterHotKeys();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // Need to shutdown the hook. idk what happens if
            // you dont, but it might cause a memory leak.
            HotkeysManager.ShutdownSystemHook();
        }
        private void ChooseDirBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "EXE(*.exe)|*.exe",
                Multiselect = false,
                CheckPathExists = true,
                RestoreDirectory = true,
            };
            if (ofd.ShowDialog() == true)
            {
                DirTxt.Text = ofd.FileName;
            }
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DirTxt.Text))
            {
                ChooseDirBtn.IsEnabled = false;
                TestBtn.IsEnabled = false;
                StopBtn.IsEnabled = true;
                SuspendBtn.IsEnabled = true;
                TestWDSwift.Client.LoopAction = i =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoopCountTxt.Text = i.ToString();
                    });
                };

                TestWDSwift.Client.ErrorAction = error =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorTxt.Text += $"{error}\r\n";
                    });
                };

                TestWDSwift.Client.TestNavChanged(DirTxt.Text);
            }
            else
            {
                MessageBox.Show("请输入程序地址", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StopBtn.IsEnabled)
            {

                TestWDSwift.Client.StopTest();
                ChooseDirBtn.IsEnabled = true;
                TestBtn.IsEnabled = true;
                StopBtn.IsEnabled = false;
                SuspendBtn.IsEnabled = false;
            }
        }


        /// <summary>
        /// 注册全局热键
        /// </summary>
        private void RegisterHotKeys()
        {
            // need to setup the global hook. this can go in
            // App.xaml.cs's constructor if you want
            HotkeysManager.SetupSystemHook();

            // You can create a globalhotkey object and pass it like so
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.F4, () =>
            {
                StopBtn_Click(null, null);
            }));

            // or do it like this. both end up doing the same thing, but this is probably simpler.
            HotkeysManager.AddHotkey(ModifierKeys.Shift, Key.F4, () =>
            {
                SuspendBtn_Click(null, null);
            });

        }

        private void SuspendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuspendBtn.Content.ToString() == "暂停")
            {
                SuspendBtn.Content = "继续";
                StopBtn.IsEnabled = false;
                TestWDSwift.Client.IsSuspend = true;
            }
            else
            {
                SuspendBtn.Content = "暂停";
                StopBtn.IsEnabled = true;
                TestWDSwift.Client.IsSuspend = false;
            }
        }
    }
}
