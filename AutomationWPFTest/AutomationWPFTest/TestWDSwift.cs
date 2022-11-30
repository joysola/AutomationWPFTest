using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AutomationWPFTest
{
    internal class TestWDSwift
    {
        #region properties
        public static TestWDSwift Client { get; } = new TestWDSwift();
        /// <summary>
        /// 进程路径
        /// </summary>
        private string _processPath;
        /// <summary>
        /// 自动化测试类
        /// </summary>
        private UIA3Automation automation;
        /// <summary>
        /// 垂直导航栏
        /// </summary>
        private ListBox verticalListBox;
        /// <summary>
        /// 水平导航栏
        /// </summary>
        private ListBox horizontalListBox;
        /// <summary>
        /// 主窗体
        /// </summary>
        private Window mainWindow;
        /// <summary>
        /// 循环次数
        /// </summary>
        private int _loopCount;
        /// <summary>
        /// 循环开始时间
        /// </summary>
        private DateTime _loopStartTime;

        private bool _isSuspend;

        private CancellationTokenSource _tokenSource;
        public Action<int> LoopAction { get; set; }
        public Action<string> ErrorAction { get; set; }
        /// <summary>
        /// 是否暂停
        /// </summary>
        internal bool IsSuspend
        {
            get => _isSuspend;
            set
            {
                _isSuspend = value;
                if (_isSuspend)
                {
                    ErrorAction?.Invoke($"暂停测试，循环时间：{TestFlaUIHelper.GetLoopTimeInfo(_loopStartTime)} ，本次循环到{_loopCount} 次。");
                }
            }
        }


        #endregion properties

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
                loginBtn.WaitUntilClickable();
                loginBtn.Click(true);
                //await Task.Delay(10000);

                await MainTask(app);
            });

        }

        /// <summary>
        /// 停止测试
        /// </summary>
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
            while (window.Name != "MainWindow") // 等待主窗体完成
            {
                await Task.Delay(100);
                window = app.GetMainWindow(automation);
            }
            mainWindow = window;
            mainWindow.WaitUntilClickable();

            var verticalToolBar = mainWindow.FindFirstDescendant(x => x.ByClassName("VerticalToolBar"));
            var horizontalNavigation = mainWindow.FindFirstDescendant(x => x.ByClassName("HorizontalNavigation"));

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
                ErrorAction?.Invoke($"终止测试，循环时间：{TestFlaUIHelper.GetLoopTimeInfo(_loopStartTime)} ，本次循环到{_loopCount} 次。");
            }
            catch (Exception ex) // 其他报错，继续执行
            {
                ErrorAction?.Invoke($"出现错误，循环时间：{TestFlaUIHelper.GetLoopTimeInfo(_loopStartTime)} ，本次循环到{_loopCount} 次。");
                ErrorAction?.Invoke($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}出错，问题：{ex.Message}、详细：{ex.InnerException?.Message}");
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
            _loopStartTime = DateTime.Now;
            //int i = 0;
            while (true)
            {
                LoopAction?.Invoke(_loopCount);
                foreach (var verticalItem in verticalListBox.Items)
                {
                    await InterruptTask(); // cancel
                    verticalItem.WaitUntilClickable();
                    verticalItem.Click(true);
                    await Task.Delay(100);
                    foreach (var horizontalItem in horizontalListBox.Items)
                    {
                        horizontalItem.WaitUntilClickable();
                        await InterruptTask(); // cancel

                        horizontalItem.Click(true);
                        await SpecialProcessTask(verticalItem, horizontalItem, mainWindow); // 特殊处理
                        await Task.Delay(100);
                    }
                }
                LoopAction?.Invoke(_loopCount++);
            }
        }
        /// <summary>
        /// 特殊处理
        /// </summary>
        /// <param name="verticalItem"></param>
        /// <param name="horizontalItem"></param>
        /// <returns></returns>
        private async Task SpecialProcessTask(ListBoxItem verticalItem, ListBoxItem horizontalItem, Window window)
        {
            var verticalItemChildren = TestFlaUIHelper.GetAutomationElementDirectChildren(verticalItem);
            var horizontalItemItemChildren = TestFlaUIHelper.GetAutomationElementDirectChildren(horizontalItem);
            var verticalItemText = verticalItemChildren?.FirstOrDefault(x => x.ControlType == FlaUI.Core.Definitions.ControlType.Text)?.Name;
            var horizontalItemText = horizontalItemItemChildren?.FirstOrDefault(x => x.ControlType == FlaUI.Core.Definitions.ControlType.Text)?.Name;

            if (!string.IsNullOrEmpty(verticalItemText) && !string.IsNullOrEmpty(horizontalItemText))
            {
                switch (verticalItemText)
                {
                    case "玻片管理":
                        switch (horizontalItemText)
                        {
                            case "玻片设置":
                                await TestModule.Client.Test_SlideManagement(window);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
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

        /// <summary>
        /// 中断操作
        /// </summary>
        /// <returns></returns>
        internal async Task InterruptTask()
        {
            StopTask();
            await SuspendTask();
        }

        /// <summary>
        /// 停止测试
        /// </summary>
        private void StopTask() => _tokenSource.Token.ThrowIfCancellationRequested(); // cancel
        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public async Task SuspendTask()
        {
            while (IsSuspend)
            {
                await Task.Delay(2000);
            }
        }
    }
}

