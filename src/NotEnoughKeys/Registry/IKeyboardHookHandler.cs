using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys.Registry;

internal interface IKeyboardLowLevelHookHandler
{
    bool HandleKey(WPARAM wParam, KBDLLHOOKSTRUCT lParam);
}