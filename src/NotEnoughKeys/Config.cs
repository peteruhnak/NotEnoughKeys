using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NotEnoughKeys;

using VK = VirtualKey;
using System.Text.Json;

public static class ConfigLoader
{
    public static Config LoadFromFile(string configFile)
    {
        if (!File.Exists(configFile))
            throw new InvalidConfigException($"No such file {configFile}");
        return LoadFromString(File.ReadAllText(configFile));
    }

    public static Config LoadFromString(string configText)
    {
        var output = JsonSerializer.Deserialize<RawConfig>(configText);
        if (output == null)
            throw new InvalidConfigException($"Failed to parse config:\n{configText}");
        var hooks = new List<Binding>();
        if (output.Hooks is { } rawHooks)
            foreach (var (key, value) in rawHooks)
                hooks.Add(ParseBinding(key, value, isHotkey: false));
        var hotkeys = new List<Binding>();
        if (output.Hotkeys is { } rawHotkeys)
            foreach (var (key, value) in rawHotkeys)
                hotkeys.Add(ParseBinding(key, value, isHotkey: true));
        return new Config
        {
            Hooks = hooks,
            Hotkeys = hotkeys
        };
    }

    internal static Binding ParseBinding(string key, JsonElement value, bool isHotkey)
    {
        var binding = new Binding();
        var allKeys = ParseKeyCombination(key);
        if (isHotkey)
        {
            binding.Keys = allKeys.Where(k => !ModifierKeys.Contains(k)).ToArray();
            binding.Modifiers = KeyMapper.KeysToModifiers(allKeys.Where(k => ModifierKeys.Contains(k)).ToArray());
        }
        else
        {
            binding.Keys = allKeys;
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                binding.Send = ParseKeyCombination(value.GetString()!);
                break;
            case JsonValueKind.Object:
            {
                var rawBinding = JsonSerializer.Deserialize<RawBinding>(value.GetRawText());
                if (rawBinding == null)
                    throw new InvalidConfigException($"Failed to parse hook action: '{value.GetRawText()}'");
                if (rawBinding.Send is { } sendString)
                {
                    binding.Send = ParseKeyCombination(sendString);
                }
                else if (rawBinding.Raw is { } rawString)
                {
                    binding.Send = ParseKeyCombination(rawString.ToCharArray().Select(c => "" + c).ToList());
                }

                binding.Run = rawBinding.Run;
                binding.Special = rawBinding.Special;
                if (rawBinding.When is { } dict)
                {
                    binding.When = new WhenCondition();
                    if (dict.TryGetValue("exe", out var whenExe))
                        binding.When.Exe = whenExe;
                    if (dict.TryGetValue("title", out var whenTitle))
                        binding.When.Title = whenTitle;
                }

                break;
            }
            default:
                throw new InvalidConfigException($"Unexpected hook binding: '{value.GetRawText()}");
        }

        return binding;
    }

    internal static VirtualKey[] ParseKeyCombination(string keySpec)
    {
        return ParseKeyCombination(keySpec.Split("&").Select(s => s.Trim()).ToList());
    }

    internal static VK[] ParseKeyCombination(IEnumerable<string> keySpec)
    {
        return keySpec.Select(KeyMapper.FromString).ToArray();
    }

    internal static HashSet<VK> ModifierKeys { get; } = new()
    {
        VK.Control,
        VK.Lcontrol,
        VK.Rcontrol,
        VK.Alt,
        VK.Lmenu,
        VK.Rmenu,
        VK.Shift,
        VK.Lshift,
        VK.Rshift,
    };
}

internal class RawConfig
{
    [JsonPropertyName("hook")] public Dictionary<string, JsonElement>? Hooks { get; set; }
    [JsonPropertyName("hotkey")] public Dictionary<string, JsonElement>? Hotkeys { get; set; }
    [JsonPropertyName("special")] public IList<Dictionary<string, object>>? Special { get; set; }
}

public class RawBinding
{
    [JsonPropertyName("send")] public string? Send { get; set; }
    [JsonPropertyName("raw")] public string? Raw { get; set; }
    [JsonPropertyName("run")] public string? Run { get; set; }
    [JsonPropertyName("special")] public string? Special { get; set; }
    [JsonPropertyName("when")] public Dictionary<string, string>? When { get; set; }
}

public class Config
{
    public IList<Binding> Hooks { get; init; } = null!;
    public IList<Binding> Hotkeys { get; init; } = null!;
}

public class Binding
{
    public VK[] Keys { get; set; } = null!;
    public Modifiers? Modifiers { get; set; }
    public VK[]? Send { get; set; }
    public string? Run { get; set; }
    public string? Special { get; set; }
    public WhenCondition? When { get; set; }
}

public class WhenCondition
{
    public string? Exe { get; set; }
    public string? Title { get; set; }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class InvalidConfigException : SystemException
{
    public InvalidConfigException(string message) : base(message)
    {
    }

    public InvalidConfigException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}