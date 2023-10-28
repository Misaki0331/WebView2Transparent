using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                const int WS_EX_LAYERED = 0x00080000;

                System.Windows.Forms.CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | WS_EX_LAYERED;

                return cp;
            }
        }

        // UpdateLayeredWindow�֘A��API��`
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DeleteObject(IntPtr hobject);

        public const byte AC_SRC_OVER = 0;
        public const byte AC_SRC_ALPHA = 1;
        public const int ULW_ALPHA = 2;

        // UpdateLayeredWindow�Ŏg��BLENDFUNCTION�\���̂̒�`
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        // UpdateLayeredWindow���g�����߂̒�`
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            [System.Runtime.InteropServices.In()]
            ref Point pptDst,
            [System.Runtime.InteropServices.In()]
            ref Size psize,
            IntPtr hdcSrc,
            [System.Runtime.InteropServices.In()]
            ref Point pptSrc,
            int crKey,
            [System.Runtime.InteropServices.In()]
            ref BLENDFUNCTION pblend,
            int dwFlags);


        // ���C���[�h�E�B���h�E��ݒ肷��
        public void SetLayeredWindow(Bitmap srcBitmap)
        {
            // �X�N���[����Graphics�ƁAhdc���擾
            Graphics g_sc = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc_sc = g_sc.GetHdc();

            // BITMAP��Graphics�ƁAhdc���擾
            Graphics g_bmp = Graphics.FromImage(srcBitmap);
            IntPtr hdc_bmp = g_bmp.GetHdc();

            // BITMAP��hdc�ŁA�T�[�t�F�C�X��BITMAP��I������
            // ���̂Ƃ��w�i�𖳐F�����ɂ��Ă���
            IntPtr oldhbmp = SelectObject(hdc_bmp, srcBitmap.GetHbitmap(Color.FromArgb(0)));

            // BLENDFUNCTION ��������
            BLENDFUNCTION blend = new BLENDFUNCTION();
            blend.BlendOp = AC_SRC_OVER;
            blend.BlendFlags = 0;
            blend.SourceConstantAlpha = 255;
            blend.AlphaFormat = AC_SRC_ALPHA;

            // �E�B���h�E�ʒu�̐ݒ�
            Point pos = new Point(this.Left, this.Top);

            // �T�[�t�F�[�X�T�C�Y�̐ݒ�
            Size surfaceSize = new Size(this.Width, this.Height);

            // �T�[�t�F�[�X�ʒu�̐ݒ�
            Point surfacePos = new Point(0, 0);

            // ���C���[�h�E�B���h�E�̐ݒ�
            UpdateLayeredWindow(
                this.Handle, hdc_sc, ref pos, ref surfaceSize,
                hdc_bmp, ref surfacePos, 0, ref blend, ULW_ALPHA);

            // ��n��
            DeleteObject(SelectObject(hdc_bmp, oldhbmp));
            g_sc.ReleaseHdc(hdc_sc);
            g_sc.Dispose();
            g_bmp.ReleaseHdc(hdc_bmp);
            g_bmp.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            webView21.DefaultBackgroundColor = Color.Transparent;
            TopMost = true;
        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
            webView21.CoreWebView2.WebResourceResponseReceived += WebResourceResponseReceived;
            webView21.CoreWebView2.WebResourceRequested += WebResourceRequested;
            webView21.CoreWebView2.FrameCreated += CoreWebView2_FrameCreated;
            webView21.Source = new("https://cdpn.io/mortenjust/fullpage/BaLrjzm?anon=true&view=");

             }

        private void WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            Trace.WriteLine($"---- {e.Request.Uri}");
        }

        private void CoreWebView2_FrameCreated(object? sender, CoreWebView2FrameCreatedEventArgs e)
        {
            Trace.WriteLine($"FrameID:{e.Frame.FrameId} {e.Frame.Name}");
        }

        private void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
           
        }
        private void WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {

        }
    }
}