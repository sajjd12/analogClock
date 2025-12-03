using System;
using System.Drawing;
using System.Windows.Forms;

namespace computerGraphics
{
    // لوحة تدعم الرسم المزدوج لمنع الرمشة
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // عدم استدعاء السلوك الافتراضي لتقليل المسح الكامل
        }
    }

    public class Form1 : Form
    {
        private DoubleBufferedPanel clockPanel;
        private Timer timer1;

        private int centerX, centerY;
        private int radius;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Analog Clock - Greek Numerals & Frame";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            

            // تفعيل DoubleBuffer للفورم أيضاً
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // إنشاء الـ panel المزدوج
            clockPanel = new DoubleBufferedPanel();
            clockPanel.Dock = DockStyle.Fill;
            // خلفية فاتحة كما طلبت (غير داكنة)
            clockPanel.BackColor = Color.Ivory;
            clockPanel.Paint += ClockPanel_Paint;
            this.Controls.Add(clockPanel);

            // إنشاء المؤقت لحركة سلسة
            timer1 = new Timer();
            timer1.Interval = 50; // حركة سلسة لعقرب الثواني
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            this.Resize += Form1_Resize;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            clockPanel.Invalidate();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            clockPanel.Invalidate();
        }

        // دالة مساعدة لحساب إحداثيات نهاية العقرب
        private Point GetHandPoint(int length, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0;
            int x = centerX + (int)(length * Math.Sin(angleRadians));
            int y = centerY - (int)(length * Math.Cos(angleRadians));
            return new Point(x, y);
        }

        private void ClockPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // حسابات مركز ونصف قطر
            centerX = clockPanel.Width / 2;
            centerY = clockPanel.Height / 2;
            radius = (int)(Math.Min(clockPanel.Width, clockPanel.Height) * 0.45);

            // أطوال العقارب
            int secHandLen = (int)(radius * 0.85);
            int minHandLen = (int)(radius * 0.65);
            int hrHandLen = (int)(radius * 0.50); // أجعل عقرب الساعات أطول قليلاً ليكون واضحاً

            DateTime now = DateTime.Now;

            double preciseSeconds = now.Second + now.Millisecond / 1000.0;
            double secAngle = preciseSeconds * 6.0;

            double preciseMinutes = now.Minute + preciseSeconds / 60.0;
            double minAngle = preciseMinutes * 6.0;

            double preciseHours = (now.Hour % 12) + preciseMinutes / 60.0;
            double hrAngle = preciseHours * 30.0;
            
            // رسم الخلفية الداخلية (وجه الساعة) بخفة ظل لإبراز الإطار
            using (Brush faceBrush = new SolidBrush(Color.FromArgb(255, 250, 245)))
            {
                g.FillEllipse(faceBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // إطار الساعة الخارجي (ثخين وواضح)
            using (Pen framePen = new Pen(Color.DarkSlateGray, Math.Max(4, radius / 20)))
            {
                framePen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                g.DrawEllipse(framePen, centerX - radius, centerY - radius, radius * 2, radius * 2);

                // ظل بسيط خارجي (رسم دائرة أخف قليلاً خارج الإطار)
                using (Pen outerGlow = new Pen(Color.FromArgb(120, Color.LightGray), Math.Max(2, radius / 60)))
                {
                    g.DrawEllipse(outerGlow, centerX - radius - 2, centerY - radius - 2, (radius * 2) + 4, (radius * 2) + 4);
                }
            }

            // درجات الساعة الصغيرة
            using (Pen tickPen = new Pen(Color.DimGray, 1))
            {
                for (int i = 0; i < 60; i++)
                {
                    double angle = i * 6 * Math.PI / 180.0;
                    int outer = (int)(radius * 0.98);
                    int inner = (i % 5 == 0) ? (int)(radius * 0.86) : (int)(radius * 0.94);
                    int x1 = centerX + (int)(outer * Math.Sin(angle));
                    int y1 = centerY - (int)(outer * Math.Cos(angle));
                    int x2 = centerX + (int)(inner * Math.Sin(angle));
                    int y2 = centerY - (int)(inner * Math.Cos(angle));
                    g.DrawLine(tickPen, x1, y1, x2, y2);
                }
            }

            // الأرقام اليونانية (أحرف يونانية كبيرة من Α إلى Μ لتمثيل 1..12)
            string[] greekNumerals = new string[]
            {
                "", "I", "II", "III", "IV", "V", "VI",
        "VII", "VIII", "IX", "X", "XI", "XII"
            };

            using (Font numeralFont = new Font("Times New Roman", (float)(radius * 0.10), FontStyle.Bold))
            using (Brush numeralBrush = new SolidBrush(Color.Black))
            {
                int numeralRadius = (int)(radius * 0.75);
                for (int hour = 1; hour <= 12; hour++)
                {
                    float angleDegrees = hour * 30f;
                    double angleRadians = angleDegrees * Math.PI / 180.0;

                    int numX = centerX + (int)(numeralRadius * Math.Sin(angleRadians));
                    int numY = centerY - (int)(numeralRadius * Math.Cos(angleRadians));

                    string numeral = greekNumerals[hour];
                    SizeF stringSize = g.MeasureString(numeral, numeralFont);

                    g.DrawString(numeral, numeralFont, numeralBrush,
                        numX - (stringSize.Width / 2),
                        numY - (stringSize.Height / 2));
                }
            }

            // رسم العقارب — نرسم الساعات أولاً (أوسع) ثم الدقائق ثم الثواني لتقليل القطع البصري
            using (Pen hrPen = new Pen(Color.Black, Math.Max(6, radius / 18)))
            {
                hrPen.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                g.DrawLine(hrPen, centerX, centerY, GetHandPoint(hrHandLen, hrAngle).X, GetHandPoint(hrHandLen, hrAngle).Y);
            }

            using (Pen minPen = new Pen(Color.Black, Math.Max(4, radius / 28)))
            {
                minPen.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                g.DrawLine(minPen, centerX, centerY, GetHandPoint(minHandLen, minAngle).X, GetHandPoint(minHandLen, minAngle).Y);
            }

            using (Pen secPen = new Pen(Color.Crimson, Math.Max(2, radius / 120)))
            {
                secPen.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
                g.DrawLine(secPen, centerX, centerY, GetHandPoint(secHandLen, secAngle).X, GetHandPoint(secHandLen, secAngle).Y);

                
            }
            using (Pen secPen = new Pen(Color.Crimson, Math.Max(2, radius / 120)))
            {
                secPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                Point tail = GetHandPoint((int)(secHandLen * -0.15), secAngle);
                g.DrawLine(secPen, centerX, centerY, tail.X, tail.Y);
            }
                // نقطة مركزية بارزة
                int centerDotSize = Math.Max(8, radius / 24);
            using (Brush centerBrush = new SolidBrush(Color.Black))
            {
                g.FillEllipse(centerBrush, centerX - centerDotSize / 2, centerY - centerDotSize / 2, centerDotSize, centerDotSize);
            }
        }

        // تنظيف المؤقت عند الإغلاق
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (timer1 != null)
            {
                timer1.Stop();
                timer1.Tick -= Timer1_Tick;
                timer1.Dispose();
                timer1 = null;
            }
            base.OnFormClosing(e);
        }
    }
}
