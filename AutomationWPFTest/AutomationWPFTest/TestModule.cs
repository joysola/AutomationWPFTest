using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationWPFTest
{
    /// <summary>
    /// 测试具体的部分模块
    /// </summary>
    internal class TestModule
    {
        public static TestModule Client { get; } = new TestModule();
        /// <summary>
        /// 测试玻片管理
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        internal async Task Test_SlideManagement(Window window)
        {
            var slideManagement = window.FindFirstDescendant(x => x.ByClassName("SlideManagement"));
            var dataGrid = slideManagement?.FindFirstDescendant(x => x.ByControlType(FlaUI.Core.Definitions.ControlType.DataGrid))?.AsDataGridView();
            foreach (var row in dataGrid.Rows)
            {
                await TestWDSwift.Client.InterruptTask(); // cancel
                foreach (var cell in row.Cells)
                {
                    await TestWDSwift.Client.InterruptTask(); // cancel
                    var btns = TestFlaUIHelper.GetAutomationElementDirectChildren(cell);
                    var deleteBtn = btns?.FirstOrDefault(x => x.ControlType == FlaUI.Core.Definitions.ControlType.Button && x.Name == "编辑");
                    if (deleteBtn != null)
                    {
                        deleteBtn.WaitUntilClickable();
                        deleteBtn.Click(true);
                        Window editWin = null;
                        while (editWin == null)
                        {
                            await Task.Delay(500);
                            editWin = window.ModalWindows?.FirstOrDefault(x => x.Name == "编辑病例").AsWindow();
                        }
                        var templateCtls = TestFlaUIHelper.GetAutomationElementDirectChildren(editWin); // 获取弹窗的内部控件集合
                        var winCloseBtn = templateCtls?.FirstOrDefault(x => x.AutomationId == "btnClose")?.AsButton(); // 获取弹窗的关闭按钮
                        await TestWDSwift.Client.InterruptTask(); // cancel
                        editWin.WaitUntilClickable();
                        //await Task.Delay(200);
                        winCloseBtn?.Click(true);
                        // 等待编辑窗口关闭完全
                        while (!editWin.IsOffscreen || window.IsOffscreen)
                        {
                            await Task.Delay(100);
                        }
                        //editWin.Close();
                        break;
                    }
                }
            }
        }
    }
}
