using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AutomationWPFTest
{
    internal class TestWDSwift
    {
        public static TestWDSwift Client { get; } = new TestWDSwift();

        public Action<int> LoopAction { get; set; }

        private CancellationTokenSource _tokenSource;
        public void TestNavChanged(string path)
        {

            Task.Run(async () =>
            {
                CheckStartProcess(path);

                using var automation = new UIA3Automation();

                var app = FlaUI.Core.Application.Launch(path);

                var loginWindow = app.GetMainWindow(automation);
                Console.WriteLine(loginWindow.Title);

                await Task.Delay(200);
                var loginBtn = loginWindow.FindFirstDescendant(x => x.ByText("立 即 登 录")).AsButton();
                loginBtn.Click(true);
                await Task.Delay(2500);

                var window = app.GetMainWindow(automation);
                Console.WriteLine(loginWindow.Title);
                var verticalToolBar = window.FindFirstDescendant(x => x.ByClassName("VerticalToolBar"));
                var horizontalNavigation = window.FindFirstDescendant(x => x.ByClassName("HorizontalNavigation"));

                var verticalListBox = verticalToolBar.FindFirstDescendant(x => x.ByAutomationId("lbMenuControl")).AsListBox();
                var horizontalListBox = horizontalNavigation.FindFirstDescendant(x => x.ByAutomationId("lbMenuControl")).AsListBox();

                try
                {
                    _tokenSource = new CancellationTokenSource();
                    await Task.Run(async () =>
                    {
                        int i = 0;
                        while (true)
                        {
                            LoopAction?.Invoke(i);
                            foreach (var verticalItem in verticalListBox.Items)
                            {
                                _tokenSource.Token.ThrowIfCancellationRequested(); // cancel

                                verticalItem.Click(true);
                                await Task.Delay(500);
                                foreach (var horizontalItem in horizontalListBox.Items)
                                {
                                    _tokenSource.Token.ThrowIfCancellationRequested(); // cancel

                                    horizontalItem.Click(true);
                                    await Task.Delay(500);
                                }
                            }
                            LoopAction?.Invoke(i++);
                        }
                    }, _tokenSource.Token);
                }
                catch (Exception ex)
                {
                    // do nothing
                    MessageBox.Show($"出错了，问题：{ex.Message}、详细：{ex.InnerException?.Message}");
                }
            });

        }


        public void StopTest()
        {
            _tokenSource?.Cancel();
        }
        /// <summary>
        /// 启动前检查，是否存在进程，存在则kill
        /// </summary>
        /// <param name="path"></param>
        public void CheckStartProcess(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            Process[] pros = Process.GetProcessesByName(fileName);
            for (int i = 0; i < pros.Length; i++)
            {
                if (pros[i].Id != Process.GetCurrentProcess().Id)
                {
                    pros[i].Kill();
                }
            }
        }

    }
}

