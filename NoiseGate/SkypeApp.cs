using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace NoiseGate
{
    public class SkypeApp
    {
        static AutomationElement GetSkypeWindow()
        {
            var SkyeMainWindowCondition = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.ClassNameProperty, "tSkMainForm"));
            return AutomationElement.RootElement.FindFirst(TreeScope.Children, SkyeMainWindowCondition);
        }

        static void UIAInvokeButton(string ButtonName)
        {
            var btn = GetSkypeWindow().FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, ButtonName));
            if (btn != null)
            {
                // UIA brings the window to the front, work around.
                SendKeys.SendWait("^`");

                // var MuteInvokeButon = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
                // MuteInvokeButon.Invoke();
            }
        }

        public static void Mute()
        {
            UIAInvokeButton("Mute");
        }

        public static void Unmute()
        {
            UIAInvokeButton("Unmute");
        }
    }
}
