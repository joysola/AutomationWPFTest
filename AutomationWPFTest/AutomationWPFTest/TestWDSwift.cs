using FlaUI.Core;
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
using System.Windows.Automation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AutomationWPFTest
{
    internal class TestWDSwift
    {
        public static TestWDSwift Client { get; } = new TestWDSwift();
        /// <summary>
        /// 进程路径
        /// </summary>
        private string _processPath;
        private UIA3Automation automation;
        /// <summary>
        /// 垂直导航栏
        /// </summary>
        private ListBox verticalListBox;
        /// <summary>
        /// 水平导航栏
        /// </summary>
        private ListBox horizontalListBox;
        public Action<int> LoopAction { get; set; }
        public Action<string> ErrorAction { get; set; }


        private CancellationTokenSource _tokenSource;
        public void TestNavChanged(string path)
        {
            _processPath = path;
            Task.Run(async () =>
            {
                CheckStartProcess(path);
                automation = new UIA3Automation()
;

                var app = FlaUI.Core.Application.Launch(path);


                var loginWindow = app.GetMainWindow(automation);
                Console.WriteLine(loginWindow.Title);

                await Task.Delay(200);
                var loginBtn = loginWindow.FindFirstDescendant(x => x.ByText("立 即 登 录")).AsButton();
                loginBtn.Click(true);
                await Task.Delay(2500);

                await MainTask(app);
            });

        }


        public void StopTest()
        {
            automation.Dispose();
            automation = null;
            _tokenSource?.Cancel();
        }

        /// <summary>
        /// 启动前检查，是否存在进程，存在则kill
        /// </summary>
        /// <param name="path"></param>
        public void CheckStartProcess(string path)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            Process[] pros = Process.GetProcessesByName(fileName);
            for (int i = 0; i < pros.Length; i++)
            {
                if (pros[i].Id != Process.GetCurrentProcess().Id)
                {
                    pros[i].Kill();
                }
            }
        }
        /// <summary>
        /// 主任务
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private async Task MainTask(FlaUI.Core.Application app)
        {
            var window = app.GetMainWindow(automation);
            var verticalToolBar = window.FindFirstDescendant(x => x.ByClassName("VerticalToolBar"));
            var horizontalNavigation = window.FindFirstDescendant(x => x.ByClassName("HorizontalNavigation"));

             verticalListBox = verticalToolBar.FindFirstDescendant(x => x.ByAutomationId("lbMenuControl")).AsListBox();
             horizontalListBox = horizontalNavigation.FindFirstDescendant(x => x.ByAutomationId("lbMenuControl")).AsListBox();

            try
            {
                _tokenSource = new CancellationTokenSource();
                await Task.Run(async () =>
                {
                    await LoopTask();
                }, _tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
            catch (Exception ex) // 其他报错，继续执行
            {
                ErrorAction?.Invoke($"{DateTime.Now:yyyyMMddHHmmss}出错，问题：{ex.Message}、详细：{ex.InnerException?.Message}");
                await Task.Delay(500);
                await Retry();
            }
        }
        /// <summary>
        /// 循环任务
        /// </summary>
        /// <param name="verticalListBox"></param>
        /// <param name="horizontalListBox"></param>
        /// <returns></returns>
        private async Task LoopTask()
        {
            int i = 0;
            while (true)
            {
                LoopAction?.Invoke(i);
                foreach (var verticalItem in verticalListBox.Items)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested(); // cancel
                    verticalItem.WaitUntilClickable();
                    verticalItem.Click(true);
                    await Task.Delay(100);
                    foreach (var horizontalItem in horizontalListBox.Items)
                    {
                        horizontalItem.WaitUntilClickable();
                        _tokenSource.Token.ThrowIfCancellationRequested(); // cancel

                        horizontalItem.Click(true);
                        await Task.Delay(100);
                    }
                }
                LoopAction?.Invoke(i++);
            }
        }
        /// <summary>
        /// 重试
        /// </summary>
        /// <returns></returns>
        private async Task Retry()
        {
            if (!string.IsNullOrEmpty(_processPath))
            {
                automation?.Dispose();
                automation = null;
                automation = new UIA3Automation();
                var app = FlaUI.Core.Application.Attach(_processPath); // 再次附加到进程
                await MainTask(app);
            }
        }
    }
}

