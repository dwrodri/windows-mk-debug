using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MouseKeyboardTracker
{
    public partial class MainForm : Form
    {
        private Label mousePositionLabel;
        private TextBox keystrokeTextBox;
        private Button clearButton;
        private CheckBox trackingCheckBox;
        
        private GlobalMouseHook mouseHook;
        private GlobalKeyboardHook keyboardHook;
        private StringBuilder keystrokeLog = new StringBuilder();

        public MainForm()
        {
            InitializeComponent();
            InitializeHooks();
        }

        private void InitializeComponent()
        {
            this.Text = "Mouse & Keyboard Tracker";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);

            // Mouse position label
            mousePositionLabel = new Label
            {
                Text = "Mouse Position: (0, 0)",
                Location = new Point(10, 10),
                Size = new Size(300, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            this.Controls.Add(mousePositionLabel);

            // Tracking checkbox
            trackingCheckBox = new CheckBox
            {
                Text = "Enable Tracking",
                Location = new Point(10, 50),
                Size = new Size(150, 30),
                Checked = true
            };
            trackingCheckBox.CheckedChanged += TrackingCheckBox_CheckedChanged;
            this.Controls.Add(trackingCheckBox);

            // Clear button
            clearButton = new Button
            {
                Text = "Clear Log",
                Location = new Point(170, 50),
                Size = new Size(100, 30)
            };
            clearButton.Click += ClearButton_Click;
            this.Controls.Add(clearButton);

            // Keystroke log textbox
            Label keystrokeLabel = new Label
            {
                Text = "Keystroke Log:",
                Location = new Point(10, 90),
                Size = new Size(100, 20)
            };
            this.Controls.Add(keystrokeLabel);

            keystrokeTextBox = new TextBox
            {
                Location = new Point(10, 115),
                Size = new Size(560, 320),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(keystrokeTextBox);
        }

        private void InitializeHooks()
        {
            mouseHook = new GlobalMouseHook();
            keyboardHook = new GlobalKeyboardHook();

            mouseHook.MouseMoved += MouseHook_MouseMoved;
            keyboardHook.KeyDown += KeyboardHook_KeyDown;
            keyboardHook.KeyUp += KeyboardHook_KeyUp;

            if (trackingCheckBox.Checked)
            {
                mouseHook.Install();
                keyboardHook.Install();
            }
        }

        private void TrackingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (trackingCheckBox.Checked)
            {
                mouseHook.Install();
                keyboardHook.Install();
            }
            else
            {
                mouseHook.Uninstall();
                keyboardHook.Uninstall();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            keystrokeLog.Clear();
            keystrokeTextBox.Text = "";
        }

        private void MouseHook_MouseMoved(object sender, Point location)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => 
                {
                    mousePositionLabel.Text = $"Mouse Position: ({location.X}, {location.Y})";
                }));
            }
            else
            {
                mousePositionLabel.Text = $"Mouse Position: ({location.X}, {location.Y})";
            }
        }

        private void KeyboardHook_KeyDown(object sender, GlobalKeyboardHook.KeyboardHookEventArgs e)
        {
            LogKeystroke($"DOWN: {GetKeyString(e.VirtualKeyCode, e.Modifiers)}");
        }

        private void KeyboardHook_KeyUp(object sender, GlobalKeyboardHook.KeyboardHookEventArgs e)
        {
            LogKeystroke($"UP: {GetKeyString(e.VirtualKeyCode, e.Modifiers)}");
        }

        private string GetKeyString(int virtualKeyCode, GlobalKeyboardHook.ModifierKeys modifiers)
        {
            StringBuilder keyString = new StringBuilder();

            // Add modifiers
            if ((modifiers & GlobalKeyboardHook.ModifierKeys.Control) != 0)
                keyString.Append("Ctrl+");
            if ((modifiers & GlobalKeyboardHook.ModifierKeys.Alt) != 0)
                keyString.Append("Alt+");
            if ((modifiers & GlobalKeyboardHook.ModifierKeys.Shift) != 0)
                keyString.Append("Shift+");
            if ((modifiers & GlobalKeyboardHook.ModifierKeys.Win) != 0)
                keyString.Append("Win+");

            // Convert virtual key code to key name
            Keys key = (Keys)virtualKeyCode;
            keyString.Append(key.ToString());

            return keyString.ToString();
        }

        private void LogKeystroke(string keystroke)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {keystroke}\r\n";

            keystrokeLog.Append(logEntry);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => 
                {
                    keystrokeTextBox.Text = keystrokeLog.ToString();
                    keystrokeTextBox.SelectionStart = keystrokeTextBox.Text.Length;
                    keystrokeTextBox.ScrollToCaret();
                }));
            }
            else
            {
                keystrokeTextBox.Text = keystrokeLog.ToString();
                keystrokeTextBox.SelectionStart = keystrokeTextBox.Text.Length;
                keystrokeTextBox.ScrollToCaret();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            mouseHook?.Uninstall();
            keyboardHook?.Uninstall();
            base.OnFormClosed(e);
        }
    }
}
