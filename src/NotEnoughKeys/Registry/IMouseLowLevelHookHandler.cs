using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace NotEnoughKeys.Registry;

internal interface IMouseLowLevelHookHandler
{
    public bool Handle(WPARAM wParam, MSLLHOOKSTRUCT lParam);
}

[Flags]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum MSLLHOOKSTRUCT_FLAGS : uint
{
    LLMHF_INJECTED = 0x00000001,
    LLMHF_LOWER_IL_INJECTED = 0x00000002,
}