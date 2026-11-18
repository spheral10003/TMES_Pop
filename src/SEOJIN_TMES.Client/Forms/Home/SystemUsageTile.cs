using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Home
{
    /// <summary>
    /// 홈 화면 하드웨어 사용량 타일 — 왼쪽 색 막대 + [이름 | 게이지(%)] 줄들(CPU · Memory · Disk C · Disk D).
    /// 바탕 · 테두리 · 막대는 다른 정보 타일(<c>HomeScreen.InfoTile</c>)과 같은 스타일이고, 높이도 같다(<see cref="Height"/>).
    /// 줄이 넷이라 제목 줄은 두지 않는다 — 이름(CPU · Memory · Disk)만으로 무엇인지 보인다.
    ///
    /// 이름은 다국어를 타지 않고 영어로 고정한다(사용자 지정, 2026-11-18) — 하드웨어 용어라 언어마다 바꿀 필요가 없다.
    /// 디스크 수는 PC마다 달라(최대 <see cref="SystemUsageMonitor.MaxDrives"/>개) 줄 수에 맞춰 세로 가운데에 다시 놓는다.
    /// </summary>
    internal sealed class SystemUsageTile
    {
        private const string CpuText = "CPU";
        private const string MemoryText = "Memory";
        private const string DiskTextFormat = "Disk {0}";

        /// <summary>다른 정보 타일과 같은 높이 — 정보 타일 줄의 윗선 · 아랫선이 함께 맞는다.</summary>
        public const int Height = 84;
        private const int Width = 240;
        private const int RowHeight = 17;
        private const int RowGap = 2;
        private const int NameLeft = 16;
        private const int NameWidth = 52;

        private readonly Color _accent;
        private readonly UsageRow _cpu;
        private readonly UsageRow _memory;
        private readonly List<UsageRow> _drives = new List<UsageRow>();

        public PanelControl Panel { get; }

        public SystemUsageTile(Color accent)
        {
            _accent = accent;

            Panel = new PanelControl();
            Panel.Size = new Size(Width, Height);

            var bar = new PanelControl();
            bar.Dock = DockStyle.Left;
            bar.Width = 5;
            bar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            bar.Appearance.BackColor = accent;
            bar.Appearance.Options.UseBackColor = true;
            Panel.Controls.Add(bar);

            _cpu = AddRow(CpuText);
            _memory = AddRow(MemoryText);
            PlaceRows();
        }

        /// <summary>읽은 값을 게이지에 넣는다. 디스크 구성이 바뀌었으면(처음 · 디스크 추가) 줄을 다시 만든다.</summary>
        public void SetUsage(SystemUsageSnapshot snapshot)
        {
            _cpu.Gauge.Percent = snapshot.CpuPercent;
            _memory.Gauge.Percent = snapshot.MemoryPercent;

            if (!SameDrives(snapshot.Drives))
                RebuildDrives(snapshot.Drives);

            for (var i = 0; i < _drives.Count; i++)
                _drives[i].Gauge.Percent = snapshot.Drives[i].Percent;
        }

        public void ApplyTheme()
        {
            AppTheme.CardPanel(Panel);

            foreach (var row in AllRows())
                ApplyRowTheme(row);
        }

        private bool SameDrives(IReadOnlyList<DriveUsage> drives)
        {
            if (drives.Count != _drives.Count)
                return false;

            for (var i = 0; i < drives.Count; i++)
                if (!string.Equals(_drives[i].Letter, drives[i].Letter, StringComparison.OrdinalIgnoreCase))
                    return false;

            return true;
        }

        private void RebuildDrives(IReadOnlyList<DriveUsage> drives)
        {
            Panel.SuspendLayout();
            foreach (var row in _drives)
            {
                row.Name.Dispose();
                row.Gauge.Dispose();
            }
            _drives.Clear();

            foreach (var drive in drives)
            {
                var row = AddRow(string.Format(CultureInfo.InvariantCulture, DiskTextFormat, drive.Letter));
                row.Letter = drive.Letter;
                ApplyRowTheme(row);
                _drives.Add(row);
            }

            PlaceRows();
            Panel.ResumeLayout();
        }

        private UsageRow AddRow(string text)
        {
            var name = new LabelControl();
            name.AutoSizeMode = LabelAutoSizeMode.None;
            name.AutoEllipsis = true;
            name.Size = new Size(NameWidth, RowHeight);
            name.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            name.Text = text;

            var gauge = new UsageGauge();
            gauge.Size = new Size(Width - NameLeft - NameWidth - 6 - 12, RowHeight);
            gauge.Fill = _accent;

            Panel.Controls.Add(name);
            Panel.Controls.Add(gauge);
            return new UsageRow(name, gauge);
        }

        /// <summary>줄들을 타일 세로 가운데에 놓는다. 디스크가 0~2개라 줄 수가 2~4로 달라진다.</summary>
        private void PlaceRows()
        {
            var rows = AllRows().ToList();
            var total = rows.Count * RowHeight + (rows.Count - 1) * RowGap;
            var top = (Height - total) / 2;

            foreach (var row in rows)
            {
                row.Name.Location = new Point(NameLeft, top);
                row.Gauge.Location = new Point(NameLeft + NameWidth + 6, top);
                top += RowHeight + RowGap;
            }
        }

        private static void ApplyRowTheme(UsageRow row)
        {
            AppTheme.Caption(row.Name);
            row.Gauge.Track = AppTheme.GaugeTrack;
            row.Gauge.TrackText = AppTheme.TextPrimary;
            row.Gauge.FillText = AppTheme.GaugeFillText;
            row.Gauge.Font = AppTheme.SmallFont;
            row.Gauge.Invalidate();
        }

        private IEnumerable<UsageRow> AllRows() => new[] { _cpu, _memory }.Concat(_drives);

        private sealed class UsageRow
        {
            public LabelControl Name { get; }
            public UsageGauge Gauge { get; }

            /// <summary>디스크 줄이면 드라이브 문자("C"). CPU · Memory 줄은 null.</summary>
            public string Letter { get; set; }

            public UsageRow(LabelControl name, UsageGauge gauge)
            {
                Name = name;
                Gauge = gauge;
            }
        }
    }

    /// <summary>
    /// 가로 막대 게이지 + 가운데 % 글자. 직접 그리는 이유: DevExpress ProgressBarControl은 스킨이 막대 색을 칠해
    /// 타일 색(막대 색)과 맞출 수 없다(PanelControl 테두리 함정과 같은 이유 — 프로젝트 CLAUDE.md 5번).
    /// 글자가 채움과 트랙에 걸치면 채움 위는 흰 글자, 트랙 위는 본문 글자색으로 나눠 그려 어디서든 읽히게 한다.
    /// </summary>
    internal sealed class UsageGauge : Control
    {
        private float _percent;

        public Color Track { get; set; } = Color.Gainsboro;
        public Color Fill { get; set; } = Color.SeaGreen;
        public Color TrackText { get; set; } = Color.Black;
        public Color FillText { get; set; } = Color.White;

        public UsageGauge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>0~100. 범위를 벗어난 값은 잘라서 그린다. 정수 %가 같으면 다시 그리지 않는다(500ms마다 깜빡이지 않게).</summary>
        public float Percent
        {
            get => _percent;
            set
            {
                var clamped = Math.Max(0f, Math.Min(100f, value));
                var changed = Math.Round(clamped) != Math.Round(_percent);
                _percent = clamped;
                if (changed)
                    Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = ClientRectangle;

            using (var track = new SolidBrush(Track))
                g.FillRectangle(track, bounds);

            var fillWidth = (int)Math.Round(bounds.Width * _percent / 100f);
            var fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
            using (var fill = new SolidBrush(Fill))
                g.FillRectangle(fill, fillRect);

            var text = ((int)Math.Round(_percent)).ToString(CultureInfo.InvariantCulture) + "%";
            // PreserveGraphicsClipping이 없으면 TextRenderer가 Graphics의 잘라내기 영역을 무시한다.
            const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                                          TextFormatFlags.SingleLine | TextFormatFlags.NoPadding |
                                          TextFormatFlags.PreserveGraphicsClipping;

            // 같은 글자를 두 번 그리되 영역을 나눈다 — 겹쳐 그리면 흰 글자 가장자리에 어두운 글자가 번져 보인다.
            var trackRect = new Rectangle(fillRect.Right, bounds.Y, bounds.Width - fillWidth, bounds.Height);
            DrawClipped(g, text, bounds, trackRect, TrackText, flags);
            DrawClipped(g, text, bounds, fillRect, FillText, flags);
        }

        private void DrawClipped(Graphics g, string text, Rectangle bounds, Rectangle clip, Color color, TextFormatFlags flags)
        {
            if (clip.Width <= 0)
                return;

            g.SetClip(clip);
            TextRenderer.DrawText(g, text, Font, bounds, color, flags);
            g.ResetClip();
        }
    }
}
