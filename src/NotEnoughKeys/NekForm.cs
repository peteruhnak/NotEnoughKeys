using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace NotEnoughKeys;

public partial class NekForm : Form
{
    private readonly KeyboardManager _keyboardManager = new();
    private readonly NotifyIcon _trayIcon;
    private readonly TextBox _debugLogBox;

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public NekForm(string configFile)
    {
        InitializeComponent();

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Width = 800,
            Height = 600
        };
        Controls.Add(panel);

        panel.Controls.Add(new Label { Text = $"Config file: {configFile}" });
        var toggleDebugButton = new Button { Text = "Toggle Debug" };
        panel.Controls.Add(toggleDebugButton);
        var clearLogButton = new Button { Text = "Clear" };
        panel.Controls.Add(clearLogButton);
        _debugLogBox = new TextBox
        {
            Width = 750,
            Height = 650,
            ScrollBars = ScrollBars.Both,
            Multiline = true,
        };
        panel.Controls.Add(_debugLogBox);

        GlobalLog.OnMessage += GlobalLogOnOnMessage;

        toggleDebugButton.Click += (_, _) => GlobalLog.ToggleDebug();
        clearLogButton.Click += (_, _) => _debugLogBox.Text = "";

        Config config = ConfigLoader.LoadFromFile(configFile);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show/Hide", null, TrayMenu_ShowHide);
        contextMenu.Items.Add("Exit", null, TrayMenu_Exit);

        _trayIcon = new NotifyIcon
        {
            Text = "NEK",
            Icon = new Icon(SystemIcons.Application, 40, 40),
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        Text = "NotEnoughKeys";
        _keyboardManager.Initialize(config);
    }

    private void GlobalLogOnOnMessage(object? sender, LogEventArgs e)
    {
        LogMessage log = e.LogMessage;
        _debugLogBox.Text += $"[{log.Timestamp}] {log.Level} {log.Message}\r\n";
        if (log.Exception is { } ex)
            _debugLogBox.Text += ex.ToString();
    }

    private void Dispose2()
    {
        GlobalLog.OnMessage -= GlobalLogOnOnMessage;
        _trayIcon.Visible = false;
        _keyboardManager.Dispose();
    }

    private void TrayMenu_ShowHide(object? sender, EventArgs e)
    {
        SetVisibleCore(!Visible);
    }

    private void TrayMenu_Exit(object? sender, EventArgs e)
    {
        Dispose2();
        Application.Exit();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Dispose2();
        base.OnClosing(e);
    }
}