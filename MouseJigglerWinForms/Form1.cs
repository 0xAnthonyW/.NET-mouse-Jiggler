using System.Runtime.InteropServices;

namespace MouseJigglerWinForms
{
    public partial class Form1 : Form
    {
        // Import necessary user32.dll functions
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Constants for mouse event flags
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // Constants for hotkey
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;
        private const uint MOD_NONE = 0x0000;
        private const uint VK_ESCAPE = 0x1B;

        // Struct to store the cursor position
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private bool stopJiggler = false;
        private Thread jigglerThread;

        public Form1()
        {
            InitializeComponent();

            // Register the hotkey
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_NONE, VK_ESCAPE);

            // Start the mouse jiggler in a separate thread
            jigglerThread = new Thread(MouseJiggler);
            jigglerThread.IsBackground = true;
            jigglerThread.Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            stopJiggler = true;
            btnStop.Enabled = false;
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            Environment.Exit(0);
        }

        private void MouseJiggler()
        {
            Random random = new Random();

            while (!stopJiggler)
            {
                // Get the current cursor position
                GetCursorPos(out POINT currentPos);

                // Move the mouse cursor a few millimeters to the left and right
                int newX = currentPos.X + random.Next(-5, 6); // Random move between -5 and 5
                int newY = currentPos.Y + random.Next(-5, 6); // Random move between -5 and 5

                // Set the new cursor position
                SetCursorPos(newX, newY);

                // Randomly decide whether to click
                if (random.NextDouble() < 0.5) // 50% chance to click
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, newX, newY, 0, UIntPtr.Zero);
                    mouse_event(MOUSEEVENTF_LEFTUP, newX, newY, 0, UIntPtr.Zero);
                }

                // Wait for a short period before the next movement
                Thread.Sleep(1000); // 1 second delay
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                BtnStop_Click(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }
    }
}
