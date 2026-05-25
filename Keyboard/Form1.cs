using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace Keyboard
{
    public partial class Form1 : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Dictionary<string, string> keyboardLanguages = new Dictionary<string, string>();
        private readonly List<LanguageOption> languageOptions = new List<LanguageOption>();

        private const int WM_INPUT = 0x00FF;
        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

        private const int RID_INPUT = 0x10000003;
        private const int RIM_TYPEKEYBOARD = 1;
        private const int RIDI_DEVICENAME = 0x20000007;

        private const int RIDEV_INPUTSINK = 0x00000100;
        private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        private const ushort HID_USAGE_GENERIC_KEYBOARD = 0x06;

        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardLanguageSwitcher");

        private static readonly string SettingsPath =
            Path.Combine(SettingsDirectory, "settings.json");

        public Form1()
        {
            InitializeComponent();

            Text = "Keyboard Language Switcher";
            Width = 1000;
            Height = 540;
            StartPosition = FormStartPosition.CenterScreen;

            LoadSystemLanguages();
            LoadSettings();

            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var deviceColumn = new DataGridViewTextBoxColumn
            {
                HeaderText = "Keyboard",
                Name = "Device",
                ReadOnly = true
            };

            var languageColumn = new DataGridViewComboBoxColumn
            {
                HeaderText = "Language",
                Name = "Language",
                DataSource = languageOptions,
                DisplayMember = nameof(LanguageOption.Display),
                ValueMember = nameof(LanguageOption.Key)
            };

            grid.Columns.Add(deviceColumn);
            grid.Columns.Add(languageColumn);

            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += Grid_CurrentCellDirtyStateChanged;

            var contactLabel = new Label
            {
                Text = "Contact: einzige.cz@gmail.com     https://www.linkedin.com/in/konstantin-liubchich/",
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            Controls.Add(grid);
            Controls.Add(contactLabel);

            Load += Form1_Load;
        }
        private void Form1_Load(object? sender, EventArgs e)
        {
            RegisterRawKeyboardInput();
            LoadKeyboardList();
        }

        private void LoadSystemLanguages()
        {
            languageOptions.Clear();

            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                string key = GetLanguageKey(language);

                languageOptions.Add(new LanguageOption
                {
                    Key = key,
                    Display = $"{language.LayoutName} ({language.Culture.Name})"
                });
            }
        }

        private static string GetLanguageKey(InputLanguage language)
        {
            return $"{language.Culture.Name}|{language.LayoutName}";
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath))
                return;

            try
            {
                string json = File.ReadAllText(SettingsPath);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings?.KeyboardLanguages == null)
                    return;

                keyboardLanguages.Clear();

                foreach (var pair in settings.KeyboardLanguages)
                {
                    keyboardLanguages[pair.Key] = pair.Value;
                }
            }
            catch
            {
                // Если файл настроек поврежден, просто запускаемся с настройками по умолчанию.
            }
        }

        private void SaveSettings()
        {
            Directory.CreateDirectory(SettingsDirectory);

            var settings = new AppSettings
            {
                KeyboardLanguages = keyboardLanguages
            };

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }

        private void Grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (grid.IsCurrentCellDirty)
            {
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string device = grid.Rows[e.RowIndex].Cells["Device"].Value?.ToString() ?? "";
            string languageKey = grid.Rows[e.RowIndex].Cells["Language"].Value?.ToString() ?? "";

            if (!string.IsNullOrWhiteSpace(device) && !string.IsNullOrWhiteSpace(languageKey))
            {
                keyboardLanguages[device] = languageKey;
                SaveSettings();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT)
            {
                string? deviceName = GetRawInputDeviceName(m.LParam);

                if (deviceName != null && keyboardLanguages.TryGetValue(deviceName, out string languageKey))
                {
                    SwitchForegroundWindowLanguage(languageKey);
                }
            }

            base.WndProc(ref m);
        }

        private void LoadKeyboardList()
        {
            uint deviceCount = 0;
            GetRawInputDeviceList(null, ref deviceCount, (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>());

            var devices = new RAWINPUTDEVICELIST[deviceCount];

            if (GetRawInputDeviceList(devices, ref deviceCount, (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>()) == uint.MaxValue)
                return;

            string defaultLanguageKey = languageOptions.FirstOrDefault()?.Key ?? "";

            foreach (var device in devices)
            {
                if (device.dwType != RIM_TYPEKEYBOARD)
                    continue;

                string? name = GetDeviceName(device.hDevice);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (grid.Rows.Cast<DataGridViewRow>().Any(r => r.Cells["Device"].Value?.ToString() == name))
                    continue;

                if (!keyboardLanguages.TryGetValue(name, out string? selectedLanguage))
                {
                    selectedLanguage = defaultLanguageKey;
                    keyboardLanguages[name] = selectedLanguage;
                }

                if (!languageOptions.Any(l => l.Key == selectedLanguage))
                {
                    selectedLanguage = defaultLanguageKey;
                    keyboardLanguages[name] = selectedLanguage;
                }

                grid.Rows.Add(name, selectedLanguage);
            }

            SaveSettings();
        }

        private void RegisterRawKeyboardInput()
        {
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];

            rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[0].usUsage = HID_USAGE_GENERIC_KEYBOARD;
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].hwndTarget = Handle;

            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
            {
                MessageBox.Show("Failed to register Raw Input.");
            }
        }

        private string? GetRawInputDeviceName(IntPtr rawInputHandle)
        {
            uint size = 0;
            GetRawInputData(rawInputHandle, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf<RAWINPUTHEADER>());

            IntPtr buffer = Marshal.AllocHGlobal((int)size);

            try
            {
                if (GetRawInputData(rawInputHandle, RID_INPUT, buffer, ref size, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) != size)
                    return null;

                RAWINPUT raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

                if (raw.header.dwType != RIM_TYPEKEYBOARD)
                    return null;

                return GetDeviceName(raw.header.hDevice);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private string? GetDeviceName(IntPtr deviceHandle)
        {
            uint size = 0;
            GetRawInputDeviceInfo(deviceHandle, RIDI_DEVICENAME, IntPtr.Zero, ref size);

            if (size == 0)
                return null;

            IntPtr buffer = Marshal.AllocHGlobal((int)size * 2);

            try
            {
                if (GetRawInputDeviceInfo(deviceHandle, RIDI_DEVICENAME, buffer, ref size) == uint.MaxValue)
                    return null;

                return Marshal.PtrToStringAnsi(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void SwitchForegroundWindowLanguage(string languageKey)
        {
            InputLanguage? language = InputLanguage.InstalledInputLanguages
                .Cast<InputLanguage>()
                .FirstOrDefault(l => GetLanguageKey(l) == languageKey);

            if (language == null)
                return;

            IntPtr foregroundWindow = GetForegroundWindow();

            if (foregroundWindow != IntPtr.Zero)
            {
                PostMessage(foregroundWindow, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, language.Handle);
            }
        }

        private class LanguageOption
        {
            public string Key { get; set; } = "";
            public string Display { get; set; } = "";
        }

        private class AppSettings
        {
            public Dictionary<string, string> KeyboardLanguages { get; set; } = new Dictionary<string, string>();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize
        );

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceList(
            [Out] RAWINPUTDEVICELIST[]? pRawInputDeviceList,
            ref uint puiNumDevices,
            uint cbSize
        );

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceInfo(
            IntPtr hDevice,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize
        );

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public int dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWKEYBOARD keyboard;
        }
    }
}