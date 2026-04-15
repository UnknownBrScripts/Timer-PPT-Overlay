using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Timer_PPT
{
    internal static class Program
    {
        private const string MutexName = "Local\\TimerPPTOverlay.SingleInstance";
        private const string ShowEventName = "Local\\TimerPPTOverlay.Show";

        [STAThread]
        private static void Main()
        {
            Mutex mutex = null;
            try
            {
                bool createdNew;
                mutex = new Mutex(true, MutexName, out createdNew);
                if (!createdNew)
                {
                    TrySignalExistingInstance();
                    return;
                }

                ConfigureUnhandledExceptionHandling();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StandaloneAppContext(ShowEventName));
            }
            catch (Exception ex)
            {
                TryLogFatal(ex);
                TryShowFatal(ex);
            }
            finally
            {
                try { mutex?.ReleaseMutex(); } catch { }
                try { mutex?.Dispose(); } catch { }
            }
        }

        private static void TrySignalExistingInstance()
        {
            try
            {
                using (var showEvent = EventWaitHandle.OpenExisting(ShowEventName))
                {
                    showEvent.Set();
                }
            }
            catch
            {
            }
        }

        private static void ConfigureUnhandledExceptionHandling()
        {
            try
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                Application.ThreadException += (s, e) =>
                {
                    TryLogFatal(e.Exception);
                    TryShowFatal(e.Exception);
                };

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception ?? new Exception("Erro não tratado (sem ExceptionObject).");
                    TryLogFatal(ex);
                    TryShowFatal(ex);
                };
            }
            catch
            {
            }
        }

        private static string GetLogPath()
        {
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimerPPT");
            return Path.Combine(baseDir, "TimerPPTOverlay.log");
        }

        private static void TryLogFatal(Exception ex)
        {
            if (ex == null) return;

            try
            {
                var path = GetLogPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.AppendAllText(path,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\r\n\r\n");
            }
            catch
            {
            }
        }

        private static void TryShowFatal(Exception ex)
        {
            try
            {
                var path = GetLogPath();
                MessageBox.Show(
                    "O Timer PPT não conseguiu iniciar.\r\n\r\n" +
                    ex.Message + "\r\n\r\n" +
                    "Log: " + path,
                    "Timer PPT",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
            }
        }
    }

    internal sealed class StandaloneAppContext : ApplicationContext
    {
        private readonly TimerForm form;
        private readonly NotifyIcon tray;
        private readonly EventWaitHandle showEvent;
        private readonly System.Windows.Forms.Timer showRequestTimer;
        private int showRequestPending;
        private volatile bool exiting;

        public StandaloneAppContext(string showEventName)
        {
            form = new TimerForm();
            form.CloseMenuExits = true;
            form.FormClosed += (s, e) => ExitThread();

            showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, showEventName);
            var showListener = new Thread(() =>
            {
                while (!exiting)
                {
                    try
                    {
                        showEvent.WaitOne();
                        if (exiting) break;
                        Interlocked.Exchange(ref showRequestPending, 1);
                    }
                    catch
                    {
                    }
                }
            })
            { IsBackground = true };
            showListener.Start();

            showRequestTimer = new System.Windows.Forms.Timer { Interval = 250 };
            showRequestTimer.Tick += (s, e) =>
            {
                if (Interlocked.Exchange(ref showRequestPending, 0) == 1)
                {
                    ShowAndActivate();
                }
            };
            showRequestTimer.Start();

            tray = new NotifyIcon
            {
                Text = "Timer PPT",
                Icon = LoadTrayIcon(),
                Visible = true
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("Mostrar/Ocultar", null, (s, e) => ToggleVisibility()));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Sair", null, (s, e) => Exit()));
            tray.ContextMenuStrip = menu;
            tray.DoubleClick += (s, e) => ToggleVisibility();

            form.Show();
            form.Activate();
        }

        private static Icon LoadTrayIcon()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                foreach (var name in asm.GetManifestResourceNames())
                {
                    if (!name.EndsWith("logo.ico", StringComparison.OrdinalIgnoreCase)) continue;
                    using (var stream = asm.GetManifestResourceStream(name))
                    {
                        if (stream == null) continue;
                        return new Icon(stream);
                    }
                }
            }
            catch
            {
            }

            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            return SystemIcons.Application;
        }

        private void ToggleVisibility()
        {
            if (form.IsDisposed) return;

            if (form.Visible)
            {
                form.Hide();
            }
            else
            {
                form.Show();
                form.Activate();
            }
        }

        private void Exit()
        {
            if (!form.IsDisposed)
            {
                form.Close();
            }
            ExitThread();
        }

        private void ShowAndActivate()
        {
            if (form.IsDisposed) return;

            try
            {
                form.ApplySettings(TimerSettingsStore.Load());
            }
            catch
            {
            }

            if (!form.Visible)
            {
                try { form.Show(); } catch { }
            }

            try { form.BringToFront(); } catch { }
            try { form.Activate(); } catch { }
        }

        protected override void ExitThreadCore()
        {
            try
            {
                exiting = true;
                try { showEvent.Set(); } catch { }

                try
                {
                    showRequestTimer.Stop();
                    showRequestTimer.Dispose();
                }
                catch
                {
                }

                tray.Visible = false;
                tray.Dispose();
            }
            catch
            {
            }

            try
            {
                try { showEvent.Dispose(); } catch { }
                if (!form.IsDisposed) form.Dispose();
            }
            catch
            {
            }

            base.ExitThreadCore();
        }
    }
}
