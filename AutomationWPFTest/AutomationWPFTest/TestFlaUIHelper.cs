using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationWPFTest
{
    /// <summary>
    /// FlaUI测试帮助类
    /// </summary>
    internal class TestFlaUIHelper
    {
        /// <summary>
        /// 获取元素直接
        /// </summary>
        /// <param name="automationElement"></param>
        /// <returns></returns>
        public static List<AutomationElement> GetAutomationElementDirectChildren(AutomationElement automationElement)
        {
            var result = new List<AutomationElement>();
            var treeWalker = automationElement.Automation.TreeWalkerFactory.GetRawViewWalker();
            var firstChild = treeWalker.GetFirstChild(automationElement);
            var child = firstChild;
            do
            {
                if (child != null)
                {
                    result.Add(child);
                }
                child = treeWalker.GetNextSibling(child);
            } while (child != null);

            return result;
        }
        /// <summary>
        /// 循环时间相关
        /// </summary>
        /// <param name="loopStartTime">循环开始时间</param>
        /// <returns></returns>
        public static string GetLoopTimeInfo(DateTime loopStartTime)
        {
            var timeSpan = DateTime.Now - loopStartTime;
            return $"{timeSpan.Hours}小时{timeSpan.Minutes}分钟{timeSpan.Seconds}秒";
        }
    }
}
