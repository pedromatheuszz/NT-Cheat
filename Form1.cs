using KeyAuth;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NT_Cheat
{
    public partial class Form1 : Form
    {

        // Configuração das credenciais do painel KeyAuth
        public static api KeyAuthApp = new api(
            name: "d4rkklz",
            ownerid: "ntQ0mh8ip7",
            secret: "fee189a728162641dfd31939b413ea587922bc4f6f191dcc18dbca413b928687",
            version: "1.0"
        );

        // Variáveis do loading
        private LoadingAnimation loadingAnimation;
        private Panel loadingOverlay;

        public Form1()
        {
            InitializeComponent();

            KeyAuthApp.init();
            if (!KeyAuthApp.response.success)
            {
                MessageBox.Show("Erro de conexão: " + KeyAuthApp.response.message);
                Environment.Exit(0);
            }

            // Configuração do loading overlay
            loadingOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(140, 15, 15, 18),
                Visible = false
            };

            loadingAnimation = new LoadingAnimation
            {
                Size = new Size(60, 60),
                BackColor = Color.Transparent
            };

            loadingOverlay.Controls.Add(loadingAnimation);
            this.Controls.Add(loadingOverlay);

            loadingOverlay.Resize += (s, e) => CenterLoadingAnimation();
            CenterLoadingAnimation();
        }

        // Método para centralizar o loading
        private void CenterLoadingAnimation()
        {
            if (loadingAnimation != null && loadingOverlay != null)
            {
                loadingAnimation.Location = new Point(
                    (loadingOverlay.Width - loadingAnimation.Width) / 2,
                    (loadingOverlay.Height - loadingAnimation.Height) / 2
                );
            }
        }

        private async void ShowLoadingAndLogin()
        {
            // Mostra o loading
            loadingOverlay.Visible = true;
            loadingOverlay.BringToFront();
            loadingAnimation.StartAnimation();

            // Pequeno delay para o loading aparecer
            await Task.Delay(500);

            await RealizarLoginAsync();
        }

        private async Task RealizarLoginAsync()
        {
            try
            {
                string user = Username.Text.Trim();
                string pass = Pass.Text.Trim();

                // Login em background thread para não congelar a UI
                bool success = false;
                string message = "";

                await Task.Run(() =>
                {
                    KeyAuthApp.login(user, pass);
                    success = KeyAuthApp.response.success;
                    message = KeyAuthApp.response.message;
                });

                if (success)
                {
                    status.Text = "Login efetuado!";

                    // Para o loading e esconde o overlay
                    loadingAnimation.StopAnimation();
                    loadingOverlay.Visible = false;

                    Form2 main = new Form2();
                    main.FormClosed += (s, e) => this.Close();
                    main.Show();
                    this.Hide();
                }
                else
                {
                    status.Text = message;

                    // Para o loading e esconde o overlay em caso de erro
                    loadingAnimation.StopAnimation();
                    loadingOverlay.Visible = false;
                }
            }
            catch (Exception ex)
            {
                status.Text = "Erro de autenticação: " + ex.Message;

                // Para o loading e esconde o overlay em caso de exceção
                loadingAnimation.StopAnimation();
                loadingOverlay.Visible = false;
            }
        }

        public class LoadingAnimation : Control
        {
            private Timer animationTimer;
            private float angle = 0;

            public LoadingAnimation()
            {
                this.Size = new Size(60, 60);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.Opaque, false);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.DoubleBuffered = true;
                this.BackColor = Color.Transparent;

                animationTimer = new Timer();
                animationTimer.Interval = 15;
                animationTimer.Tick += (s, e) =>
                {
                    angle += 10;
                    if (angle >= 360) angle = 0;
                    this.Invalidate();
                };
            }

            public void StartAnimation()
            {
                animationTimer.Start();
            }

            public void StopAnimation()
            {
                animationTimer.Stop();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int strokeWidth = 4;
                int padding = strokeWidth / 2 + 2;
                Rectangle rect = new Rectangle(padding, padding, this.Width - (padding * 2), this.Height - (padding * 2));

                // Círculo de fundo (transparente)
                using (Pen bgPen = new Pen(Color.FromArgb(40, 255, 255, 255), strokeWidth))
                {
                    e.Graphics.DrawEllipse(bgPen, rect);
                }

                // Arco animado (branco)
                using (Pen mainPen = new Pen(Color.FromArgb(255, 255, 255), strokeWidth))
                {
                    mainPen.StartCap = LineCap.Round;
                    mainPen.EndCap = LineCap.Round;
                    e.Graphics.DrawArc(mainPen, rect, angle, 100);
                }
            }

            // Permite transparência real
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                    return cp;
                }
            }
        }

        private void lczxy7AnimatedButton11_Click_1(object sender, EventArgs e)
        {
            ShowLoadingAndLogin();
        }
    }
}
