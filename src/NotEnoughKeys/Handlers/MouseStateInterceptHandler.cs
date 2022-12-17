using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using NotEnoughKeys.Registry;
using NotEnoughKeys.Tiling;

namespace NotEnoughKeys.Handlers;

internal class MouseStateInterceptHandler : IMouseLowLevelHookHandler
{
    public bool LeftMouseDown { get; private set; }
    public bool RightMouseDown { get; private set; }

    public bool Handle(WPARAM wParam, MSLLHOOKSTRUCT lParam)
    {
        var flags = (MSLLHOOKSTRUCT_FLAGS)lParam.flags;
        if ((flags & MSLLHOOKSTRUCT_FLAGS.LLMHF_INJECTED) != 0 ||
            (flags & MSLLHOOKSTRUCT_FLAGS.LLMHF_LOWER_IL_INJECTED) != 0) return false;

        switch (wParam.Value)
        {
            case PInvoke.WM_LBUTTONDOWN:
                LeftMouseDown = true;
                return true;
            case PInvoke.WM_LBUTTONUP:
                LeftMouseDown = false;
                return true;
            case PInvoke.WM_RBUTTONDOWN:
                RightMouseDown = true;
                return true;
            case PInvoke.WM_RBUTTONUP:
                RightMouseDown = false;
                return true;
            default:
                return false;
        }
    }
}