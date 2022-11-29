using FlaUI.Core.AutomationElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomationWPFTest
{
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
    }
}
