using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaleFarm.WinApi
{
    public static partial class User32
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScan(char ch);

        //private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        //private const uint KEYEVENTF_KEYUP = 0x0002;
        //private const byte ENTER = 0x0D;

        private const byte KEYUP = 0x2;
        private const byte KEYDOWN = 0x0;
        private const byte VK_SHIFT = 0x10;

        public static async Task SendText(string text)
        {
            // call this function when you want to simulate the key press .    
            // presses the key  
            foreach (var key in text)
            {
                byte Vcode = (byte)VkKeyScan(key);

                if (Regex.IsMatch(key.ToString(), @"[!_]"))
                {
                    keybd_event(VK_SHIFT, 0, KEYDOWN, 0);
                    keybd_event(Vcode, 0, KEYDOWN, 0);
                    await Task.Delay(150);
                    keybd_event(Vcode, 0, KEYUP, 0);
                    keybd_event(VK_SHIFT, 0, KEYUP, 0);
                }
                else if (char.IsUpper(key))
                {
                    keybd_event(VK_SHIFT, 0, KEYDOWN, 0);
                    keybd_event(Vcode, 0, KEYDOWN, 0);
                    await Task.Delay(150);
                    keybd_event(Vcode, 0, KEYUP, 0);
                    keybd_event(VK_SHIFT, 0, KEYUP, 0);
                }
                else
                {
                    keybd_event(Vcode, 0, KEYDOWN, 0);
                    await Task.Delay(150);
                    keybd_event(Vcode, 0, KEYUP, 0);
                }

                //keybd_event((byte)VkKeyScan(c), 0, KEYEVENTF_EXTENDEDKEY, 0); //Key down
                //await Task.Delay(150);
                //keybd_event((byte)VkKeyScan(c), 0, KEYEVENTF_KEYUP, 0); //Key up
            }
            await Task.Delay(500);

            //keybd_event(ENTER, 0x45, KEYEVENTF_EXTENDEDKEY, 0); //Key down
            //await Task.Delay(150);
            //keybd_event(ENTER, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0); //Key up
        }
    }
}
