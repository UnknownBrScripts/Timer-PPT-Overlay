using System;
using System.Drawing;
using System.Windows.Forms;

namespace Timer_PPT
{
    public class TimerSettingsForm : Form
    {
        private readonly NumericUpDown numMin;
        private readonly NumericUpDown numSec;
        private readonly NumericUpDown numFont;
        private readonly Button btnColor;
        private readonly Panel pnlColorPreview;
        private readonly CheckBox chkRememberPos;
        private readonly CheckBox chkSoundOnFinish;

        private Color selectedColor;

        public TimerSettingsForm()
        {
            Text = "Configurações do Timer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 260);
            Padding = new Padding(12);

            var s = TimerSettingsStore.Load();
            selectedColor = Color.FromArgb(s.FontColorArgb);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblTime = new Label { AutoSize = true, Text = "Tempo padrão:", Anchor = AnchorStyles.Left };
            numMin = new NumericUpDown { Width = 70, Minimum = 0, Maximum = 99, Value = s.DefaultSeconds / 60 };
            numSec = new NumericUpDown { Width = 70, Minimum = 0, Maximum = 59, Value = s.DefaultSeconds % 60 };

            var pnlTime = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill
            };
            pnlTime.Controls.Add(numMin);
            pnlTime.Controls.Add(new Label { AutoSize = true, Text = ":", Padding = new Padding(6, 6, 6, 0) });
            pnlTime.Controls.Add(numSec);

            var lblFont = new Label { AutoSize = true, Text = "Tamanho da fonte:", Anchor = AnchorStyles.Left };
            numFont = new NumericUpDown { Width = 90, Minimum = 16, Maximum = 180, Value = s.FontSize };

            var lblColor = new Label { AutoSize = true, Text = "Cor do timer:", Anchor = AnchorStyles.Left };
            pnlColorPreview = new Panel
            {
                Width = 34,
                Height = 22,
                BackColor = selectedColor,
                Margin = new Padding(0, 3, 10, 0)
            };
            btnColor = new Button { AutoSize = true, Height = 28, Text = "Selecionar..." };
            btnColor.Click += (sender, e) =>
            {
                using (var cd = new ColorDialog())
                {
                    cd.Color = selectedColor;
                    if (cd.ShowDialog(this) == DialogResult.OK)
                    {
                        selectedColor = cd.Color;
                        pnlColorPreview.BackColor = selectedColor;
                    }
                }
            };

            var pnlColor = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill
            };
            pnlColor.Controls.Add(pnlColorPreview);
            pnlColor.Controls.Add(btnColor);

            chkSoundOnFinish = new CheckBox { AutoSize = true, Text = "Aviso sonoro ao terminar", Checked = s.SoundOnFinish };
            chkRememberPos = new CheckBox { AutoSize = true, Text = "Lembrar posição do timer", Checked = s.RememberPosition };

            var btnOk = new Button { Width = 90, Text = "OK", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Width = 90, Text = "Cancelar", DialogResult = DialogResult.Cancel };
            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false
            };
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnOk);

            table.Controls.Add(lblTime, 0, 0);
            table.Controls.Add(pnlTime, 1, 0);

            table.Controls.Add(lblFont, 0, 1);
            table.Controls.Add(numFont, 1, 1);

            table.Controls.Add(lblColor, 0, 2);
            table.Controls.Add(pnlColor, 1, 2);

            table.Controls.Add(chkSoundOnFinish, 0, 3);
            table.SetColumnSpan(chkSoundOnFinish, 2);

            table.Controls.Add(chkRememberPos, 0, 4);
            table.SetColumnSpan(chkRememberPos, 2);

            table.Controls.Add(pnlButtons, 0, 5);
            table.SetColumnSpan(pnlButtons, 2);

            Controls.Add(table);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            btnOk.Click += (sender, e) => Save();
        }

        private void Save()
        {
            var s = TimerSettingsStore.Load();
            s.DefaultSeconds = (int)numMin.Value * 60 + (int)numSec.Value;
            s.FontSize = (int)numFont.Value;
            s.FontColorArgb = selectedColor.ToArgb();
            s.SoundOnFinish = chkSoundOnFinish.Checked;
            s.RememberPosition = chkRememberPos.Checked;
            TimerSettingsStore.Save(s);
        }
    }
}

