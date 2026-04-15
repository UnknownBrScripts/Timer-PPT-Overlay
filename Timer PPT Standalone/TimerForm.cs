using System;
using System.Drawing;
using System.Media;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Timer_PPT
{
    public class TimerForm : Form
    {
        private readonly TableLayoutPanel layout;
        private readonly TimerTextControl lblTime;
        private readonly Timer timer;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem miStartPause;
        private readonly ToolStripMenuItem miClose;

        private int initialTimeSeconds;
        private int timeRemainingSeconds;
        private bool isRunning;
        private int normalColorArgb;

        private bool dragging;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private bool movedBeyondClickThreshold;
        private bool soundOnFinish;
        private bool finishSoundPlayed;
        private int lastLeftClickTick;
        private Point lastLeftClickScreenPos;
        private bool thisClickIsDouble;

        public bool CloseMenuExits { get; set; }

        public TimerForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            BackColor = Color.Magenta;
            TransparencyKey = Color.Magenta;

            layout = new TableLayoutPanel
            {
                AutoSize = true,
                BackColor = Color.Magenta,
                Padding = new Padding(18),
                Margin = new Padding(0),
                ColumnCount = 1,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblTime = new TimerTextControl
            {
                Cursor = Cursors.SizeAll,
                BackColor = Color.Magenta,
                Margin = new Padding(0),
                Anchor = AnchorStyles.None,
                AutoSize = true
            };

            lblTime.MouseDown += LblTime_MouseDown;
            lblTime.MouseMove += LblTime_MouseMove;
            lblTime.MouseUp += LblTime_MouseUp;
            layout.Controls.Add(lblTime, 0, 0);
            Controls.Add(layout);

            timer = new Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;

            menu = new ContextMenuStrip();

            miStartPause = new ToolStripMenuItem("Iniciar");
            miStartPause.Click += (s, e) => ToggleTimer();
            menu.Items.Add(miStartPause);

            menu.Items.Add(new ToolStripMenuItem("Reset", null, (s, e) => Reset()));
            menu.Items.Add(new ToolStripMenuItem("Configurar...", null, (s, e) => OpenSettings()));
            menu.Items.Add(new ToolStripSeparator());
            miClose = new ToolStripMenuItem("Ocultar");
            miClose.Click += (s, e) =>
            {
                if (CloseMenuExits) Close();
                else Hide();
            };
            menu.Items.Add(miClose);

            lblTime.ContextMenuStrip = menu;
            ContextMenuStrip = menu;
            MouseDown += Main_MouseDown;
            MouseUp += Main_MouseUp;
            layout.MouseDown += Main_MouseDown;
            layout.MouseUp += Main_MouseUp;
            ApplySettings(TimerSettingsStore.Load());
        }

        public void ApplySettings(TimerSettings settings)
        {
            if (settings == null) settings = TimerSettingsStore.Load();

            initialTimeSeconds = settings.DefaultSeconds;
            normalColorArgb = settings.FontColorArgb;
            soundOnFinish = settings.SoundOnFinish;

            if (!isRunning)
            {
                timeRemainingSeconds = settings.DefaultSeconds;
                finishSoundPlayed = false;
            }

            var safeFontSize = settings.FontSize;
            if (safeFontSize < 16) safeFontSize = 16;
            if (safeFontSize > 180) safeFontSize = 180;

            lblTime.Font = new Font("Segoe UI", safeFontSize, FontStyle.Bold);

            UpdateDisplay();

            var desired = settings.RememberPosition ? new Point(settings.PosX, settings.PosY) : GetDefaultLocation();
            Location = EnsureVisibleOnAnyScreen(desired);
        }

        private Point GetDefaultLocation()
        {
            var work = Screen.PrimaryScreen != null ? Screen.PrimaryScreen.WorkingArea : new Rectangle(0, 0, 1920, 1080);
            var x = work.Right - Width - 30;
            var y = work.Top + 30;
            return ClampToWorkingArea(new Point(x, y), work);
        }

        private Point EnsureVisibleOnAnyScreen(Point desired)
        {
            var rect = new Rectangle(desired, Size);
            foreach (var screen in Screen.AllScreens)
            {
                if (rect.IntersectsWith(screen.WorkingArea))
                {
                    return ClampToWorkingArea(desired, screen.WorkingArea);
                }
            }

            return GetDefaultLocation();
        }

        private Point ClampToWorkingArea(Point p, Rectangle work)
        {
            var maxX = work.Right - Width;
            var maxY = work.Bottom - Height;

            if (maxX < work.Left) maxX = work.Left;
            if (maxY < work.Top) maxY = work.Top;

            var x = Math.Max(work.Left, Math.Min(p.X, maxX));
            var y = Math.Max(work.Top, Math.Min(p.Y, maxY));
            return new Point(x, y);
        }

        public void ToggleTimer()
        {
            if (isRunning)
            {
                Pause();
            }
            else
            {
                Start();
            }
        }

        public void Start()
        {
            finishSoundPlayed = false;
            isRunning = true;
            timer.Start();
            UpdateMenu();
        }

        public void Pause()
        {
            isRunning = false;
            timer.Stop();
            UpdateMenu();
        }

        public void Reset()
        {
            Pause();
            timeRemainingSeconds = initialTimeSeconds;
            finishSoundPlayed = false;
            UpdateDisplay();
        }

        public bool IsRunning => isRunning;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;

            var before = timeRemainingSeconds;
            timeRemainingSeconds--;
            UpdateDisplay();
            if (before > 0 && timeRemainingSeconds == 0) TryPlayFinishSound();
        }

        private void TryPlayFinishSound()
        {
            if (!soundOnFinish) return;
            if (finishSoundPlayed) return;
            finishSoundPlayed = true;
            try { SystemSounds.Exclamation.Play(); } catch { }
        }

        private void OpenSettings()
        {
            using (var dlg = new TimerSettingsForm())
            {
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    ApplySettings(TimerSettingsStore.Load());
                }
            }
        }

        private void UpdateMenu()
        {
            miStartPause.Text = isRunning ? "Pausar" : "Iniciar";
            miClose.Text = CloseMenuExits ? "Fechar" : "Ocultar";
        }

        private void UpdateDisplay()
        {
            var sign = timeRemainingSeconds < 0;
            var absSeconds = Math.Abs(timeRemainingSeconds);
            var m = absSeconds / 60;
            var s = absSeconds % 60;
            lblTime.Text = (sign ? "-" : "") + $"{m:D2}:{s:D2}";
            UpdateColors();
            UpdateMenu();
            ResizeToFit();
        }

        private void UpdateColors()
        {
            var color = timeRemainingSeconds <= 0 ? Color.Red : Color.FromArgb(normalColorArgb);
            lblTime.ForeColor = color;
        }

        private void ResizeToFit()
        {
            var outerPadding = 16;
            layout.Location = new Point(outerPadding, outerPadding);

            ClientSize = new Size(layout.Width + outerPadding * 2, layout.Height + outerPadding * 2);
        }

        private void BeginClickSequence(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var now = Environment.TickCount;
            var pos = Cursor.Position;

            var withinTime = now - lastLeftClickTick <= SystemInformation.DoubleClickTime;
            var withinArea = Math.Abs(pos.X - lastLeftClickScreenPos.X) <= SystemInformation.DoubleClickSize.Width &&
                             Math.Abs(pos.Y - lastLeftClickScreenPos.Y) <= SystemInformation.DoubleClickSize.Height;

            thisClickIsDouble = withinTime && withinArea;
            lastLeftClickTick = now;
            lastLeftClickScreenPos = pos;
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            BeginClickSequence(e);
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (thisClickIsDouble) Reset();
            else ToggleTimer();
        }

        private void LblTime_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            if (e.Button != MouseButtons.Left) return;

            BeginClickSequence(e);
            dragging = true;
            movedBeyondClickThreshold = false;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }

        private void LblTime_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;

            var dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            if (!movedBeyondClickThreshold && (Math.Abs(dif.X) > 4 || Math.Abs(dif.Y) > 4))
            {
                movedBeyondClickThreshold = true;
            }
            Location = Point.Add(dragFormPoint, new Size(dif));
        }

        private void LblTime_MouseUp(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            dragging = false;

            var s = TimerSettingsStore.Load();
            if (s.RememberPosition)
            {
                s.PosX = Location.X;
                s.PosY = Location.Y;
                TimerSettingsStore.Save(s);
            }

            if (e.Button == MouseButtons.Left && !movedBeyondClickThreshold)
            {
                if (thisClickIsDouble) Reset();
                else ToggleTimer();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEACTIVATE = 0x21;
            const int MA_ACTIVATE = 1;

            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_ACTIVATE;
                return;
            }

            base.WndProc(ref m);
        }

        private sealed class TimerTextControl : Control
        {
            public TimerTextControl()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.UserPaint, true);
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                Invalidate();
                if (AutoSize) Size = GetPreferredSize(Size.Empty);
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                Invalidate();
                if (AutoSize) Size = GetPreferredSize(Size.Empty);
            }

            protected override void OnForeColorChanged(EventArgs e)
            {
                base.OnForeColorChanged(e);
                Invalidate();
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                var text = string.IsNullOrEmpty(Text) ? "00:00" : Text;
                var s = TextRenderer.MeasureText(text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                return new Size(s.Width + 10, s.Height + 10);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.Clear(Color.Magenta);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                e.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                var text = Text ?? string.Empty;
                if (text.Length == 0) return;

                using (var shadowBrush = new SolidBrush(Color.Black))
                using (var outlineBrush = new SolidBrush(Color.Black))
                using (var fillBrush = new SolidBrush(ForeColor))
                {
                    var origin = new PointF(4f, 2f);
                    e.Graphics.DrawString(text, Font, shadowBrush, origin.X + 2f, origin.Y + 2f);

                    var offsets = new[]
                    {
                        new PointF(-1f, 0f), new PointF(1f, 0f), new PointF(0f, -1f), new PointF(0f, 1f),
                        new PointF(-1f, -1f), new PointF(-1f, 1f), new PointF(1f, -1f), new PointF(1f, 1f)
                    };

                    foreach (var o in offsets)
                    {
                        e.Graphics.DrawString(text, Font, outlineBrush, origin.X + o.X, origin.Y + o.Y);
                    }

                    e.Graphics.DrawString(text, Font, fillBrush, origin);
                }
            }
        }
    }
}
