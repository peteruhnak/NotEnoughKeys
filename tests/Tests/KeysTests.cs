namespace Tests;

public class KeysTests
{
    [TestCase(new VirtualKey[] { }, (Modifiers)0)]
    [TestCase(new[] { VirtualKey.Control }, Modifiers.Control)]
    [TestCase(new[] { VirtualKey.Alt }, Modifiers.Alt)]
    [TestCase(new[] { VirtualKey.Shift }, Modifiers.Shift)]
    [TestCase(new[] { VirtualKey.Control, VirtualKey.Alt, VirtualKey.Shift },
        Modifiers.Control | Modifiers.Alt | Modifiers.Shift)]
    public void KeysToModifiers(VirtualKey[] keys, Modifiers modifiers)
    {
        Assert.That(KeyMapper.KeysToModifiers(keys), Is.EqualTo(modifiers));
        Assert.That(KeyMapper.ModifiersToKeys(modifiers), Is.EqualTo(keys));
    }

    [TestCase(new[] { VirtualKey.V }, ExpectedResult = (Modifiers)0)]
    [TestCase(new[] { VirtualKey.V, VirtualKey.Shift }, ExpectedResult = Modifiers.Shift)]
    public Modifiers IgnoreUnknown(VirtualKey[] keys)
        => KeyMapper.KeysToModifiers(keys);

    // @formatter:off
    [TestCase(new[] { VirtualKey.Left, VirtualKey.Right, VirtualKey.Up, VirtualKey.Down, VirtualKey.Home, VirtualKey.End, VirtualKey.PageUp, VirtualKey.PageDown, VirtualKey.Insert, VirtualKey.Delete, VirtualKey.Apps, VirtualKey.Lwin, VirtualKey.Rwin }, true)]
    // @formatter:on
    [TestCase(new[] { VirtualKey.V, VirtualKey.X, VirtualKey.Shift }, false)]
    public void IsExtended(VirtualKey[] keys, bool expected)
    {
        foreach (VirtualKey k in keys)
            Assert.That(KeyMapper.IsExtended(k), Is.EqualTo(expected));
    }

    [TestCase("0", ExpectedResult = VirtualKey.K0)]
    [TestCase("9", ExpectedResult = VirtualKey.K9)]
    [TestCase("ctRL", ExpectedResult = VirtualKey.Control)]
    [TestCase("Semicolon", ExpectedResult = VirtualKey.Oem1)]
    [TestCase("Quote", ExpectedResult = VirtualKey.Oem7)]
    [TestCase("X", ExpectedResult = VirtualKey.X)]
    [TestCase("Shift", ExpectedResult = VirtualKey.Shift)]
    public VirtualKey FromString(string s)
        => KeyMapper.FromString(s);

    [TestCase("unknown")]
    [TestCase("")]
    public void FromStringFail(string s)
        => Assert.Throws<ArgumentException>(() => KeyMapper.FromString(s));
}