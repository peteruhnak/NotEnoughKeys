using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Tests;

using VK = VirtualKey;

public class ConfigLoaderTests
{
    [TestCase("control", VK.Control)]
    [TestCase("ctrl", VK.Control)]
    [TestCase("ctRL", VK.Control)]
    [TestCase("0", VK.K0)]
    [TestCase("9", VK.K9)]
    public void ParseSingle(string spec, VK expected)
    {
        Assert.That(ConfigLoader.ParseKeyCombination(spec), Is.EqualTo(new[] { expected }));
    }

    [TestCase("ctrl & a", new[] { VK.Control, VK.A })]
    [TestCase("CapsLock & space", new[] { VK.CapsLock, VK.Space })]
    public void ParseMultiple(string spec, VK[] expected)
    {
        Assert.That(ConfigLoader.ParseKeyCombination(spec), Is.EqualTo(expected));
    }

    [Test]
    public void ScanCode()
    {
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_LSHIFT, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_LSHIFT, PInvoke.MAPVK_VK_TO_VSC_EX));
        
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_RSHIFT, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_RSHIFT, PInvoke.MAPVK_VK_TO_VSC_EX));
        
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_LEFT, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_LEFT, PInvoke.MAPVK_VK_TO_VSC_EX));
        
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_RIGHT, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_RIGHT, PInvoke.MAPVK_VK_TO_VSC_EX));
        
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_HOME, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_HOME, PInvoke.MAPVK_VK_TO_VSC_EX));

        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_END, PInvoke.MAPVK_VK_TO_VSC));
        Console.WriteLine(PInvoke.MapVirtualKey((uint)VIRTUAL_KEY.VK_END, PInvoke.MAPVK_VK_TO_VSC_EX));
    }
    

    [Test]
    public void ParseConfig()
    {
        var config = ConfigLoader.LoadFromString(ExampleConfig);
        Assert.Multiple(() =>
        {
            Assert.That(config.Hooks, Has.Count.EqualTo(3));
            Assert.That(config.Hotkeys, Has.Count.EqualTo(1));

            Assert.That(config.Hooks[0].Keys, Is.EqualTo(new[] { VK.CapsLock, VK.D }));
            Assert.That(config.Hooks[0].Modifiers, Is.Null);
            Assert.That(config.Hooks[0].Send, Is.EqualTo(new[] { VK.Delete }));
            Assert.That(config.Hooks[0].Run, Is.Null);
            Assert.That(config.Hooks[0].When, Is.Null);

            Assert.That(config.Hooks[1].Keys, Is.EqualTo(new[] { VK.Control, VK.U }));
            Assert.That(config.Hooks[1].Modifiers, Is.Null);
            Assert.That(config.Hooks[1].Send, Is.EqualTo(new[] { VK.H, VK.M, VK.M }));
            Assert.That(config.Hooks[1].Run, Is.Null);
            Assert.That(config.Hooks[1].When, Is.Not.Null);
            Assert.That(config.Hooks[1].When!.Exe, Is.EqualTo("notepad.exe"));
            Assert.That(config.Hooks[1].When!.Title, Is.EqualTo("Notepad"));
            
            Assert.That(config.Hooks[2].Keys, Is.EqualTo(new[] { VK.CapsLock, VK.Q }));
            Assert.That(config.Hooks[2].Modifiers, Is.Null);
            Assert.That(config.Hooks[2].Send, Is.Null);
            Assert.That(config.Hooks[2].Run, Is.Null);
            Assert.That(config.Hooks[2].Special, Is.EqualTo("MoveWindow"));

            Assert.That(config.Hotkeys[0].Keys, Is.EqualTo(new[] { VK.T }));
            Assert.That(config.Hotkeys[0].Modifiers, Is.EqualTo(Modifiers.Control | Modifiers.Alt));
            Assert.That(config.Hotkeys[0].Send, Is.Null);
            Assert.That(config.Hotkeys[0].Run, Is.EqualTo("wt"));
            Assert.That(config.Hotkeys[0].When, Is.Null);
        });
    }

    private string ExampleConfig => @"
        {
            ""hook"": {
                ""CapsLock & d"": ""delete"",
                ""Ctrl & u"": {
                    ""raw"": ""hmm"",
                    ""when"": {
                        ""exe"": ""notepad.exe"",
                        ""title"": ""Notepad""
                    }
                },
                ""CapsLock & q"": {
                    ""special"": ""MoveWindow""
                }
            },
            ""hotkey"": {
                ""Ctrl & Alt & t"": {
                    ""run"": ""wt""
                }
            }
        }";
}