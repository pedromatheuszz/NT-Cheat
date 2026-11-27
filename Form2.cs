using EXTERNAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NT_Cheat
{
    public partial class Form2 : Form
    {
        private Aimem memory = new Aimem();

        private bool isHeadKeyPressed = false;
        private bool isLegitKeyPressed = false;
        private Keys headKey = Keys.F1;
        private Keys legitKey = Keys.F2;
        private bool isWaitingForKey = false;
        private int keySelectionMode = 0;

        private Thread keyThread;
        private volatile bool isRunning = true;

        // Thread-safe: lock protege acesso às listas e dicionários de endereços
        private readonly object headLock = new object();
        private readonly object legitLock = new object();

        private Dictionary<long, int> HeadOriginalValues = new Dictionary<long, int>();
        private List<long> HeadFoundAddresses = new List<long>();
        private volatile bool HeadActive = false;
        private volatile bool HeadEnabled = false;
        private int HeadMode = 0;

        private Dictionary<long, int> LegitOriginalValues = new Dictionary<long, int>();
        private List<long> LegitFoundAddresses = new List<long>();
        private volatile bool LegitActive = false;
        private volatile bool LegitEnabled = false;
        private int LegitMode = 0;

        // Process monitoring
        private int processCheckCounter = 0;
        private const int PROCESS_CHECK_INTERVAL = 200; // a cada 200 iterações (~1s)

        // ========== EXTERNAL BONES ==========
        private const int OFF_HEAD = 0xB4;
        private const int OFF_CHEST = 0xB8;
        private const int OFF_NECK = 0xBC;
        private const int OFF_ROOT = 0xC8;
        private const int OFF_LEFT_ANKLE = 0xD0;
        private const int OFF_RIGHT_ANKLE = 0xD4;
        private const int OFF_LEFT_FOOT = 0xD8;
        private const int OFF_RIGHT_FOOT = 0xDC;
        private const int OFF_LEFT_SHOULDER = 0xE8;
        private const int OFF_RIGHT_SHOULDER = 0xEC;
        private const int OFF_RIGHT_HAND = 0xF0;
        private const int OFF_LEFT_HAND = 0xF4;
        private const int OFF_RIGHT_ELBOW = 0xF8;
        private const int OFF_LEFT_ELBOW = 0xFC;
        private const int OFF_COLLIDER = 0x100;
        private const int OFF_PATCH_COLLIDER = -0x394;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const string Pattern =
            "FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";

        private static readonly string[] TargetProcesses = new[]
        {
            "HD-Player", "HD-Player.exe",
            "Bluestacks", "Bluestacks.exe",
            "BstkSVC", "BstkSVC.exe",
            "AndroidProcess", "AndroidProcess.exe",
            "dnplayer", "dnplayer.exe",
            "LdVBoxHeadless",
            "MEmu", "MEmu.exe",
            "Nox", "Nox.exe", "NoxVMHandle",
            "aow_exe", "GameLoop", "AndroidEmulator"
        };

        public Form2()
        {
            InitializeComponent();

            // Close button (canto superior direito)
            var closeBtn = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 140, 140),
                BackColor = Color.Transparent,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            closeBtn.Location = new Point(this.ClientSize.Width - closeBtn.PreferredWidth - 10, 4);
            closeBtn.MouseEnter += (s, e) => closeBtn.ForeColor = Color.White;
            closeBtn.MouseLeave += (s, e) => closeBtn.ForeColor = Color.FromArgb(140, 140, 140);
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
            closeBtn.BringToFront();

            // Minimize button
            var minBtn = new Label
            {
                Text = "─",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 140, 140),
                BackColor = Color.Transparent,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            minBtn.Location = new Point(closeBtn.Left - minBtn.PreferredWidth - 6, 4);
            minBtn.MouseEnter += (s, e) => minBtn.ForeColor = Color.White;
            minBtn.MouseLeave += (s, e) => minBtn.ForeColor = Color.FromArgb(140, 140, 140);
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            this.Controls.Add(minBtn);
            minBtn.BringToFront();

            if (guna2ComboBox1 != null)
            {
                guna2ComboBox1.Items.Clear();
                guna2ComboBox1.Items.Add("Hold");
                guna2ComboBox1.Items.Add("Toggle");
                guna2ComboBox1.SelectedIndex = 0;
                guna2ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                guna2ComboBox1.SelectedIndexChanged += Guna2ComboBox1_SelectedIndexChanged;
            }

            if (guna2ComboBox2 != null)
            {
                guna2ComboBox2.Items.Clear();
                guna2ComboBox2.Items.Add("Hold");
                guna2ComboBox2.Items.Add("Toggle");
                guna2ComboBox2.SelectedIndex = 0;
                guna2ComboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                guna2ComboBox2.SelectedIndexChanged += Guna2ComboBox2_SelectedIndexChanged;
            }

            if (guna2Button1 != null)
            {
                guna2Button1.Text = $"Head: {headKey}";
                guna2Button1.Click += HeadKeyButton_Click;
            }
            if (guna2Button2 != null)
            {
                guna2Button2.Text = $"Legit: {legitKey}";
                guna2Button2.Click += LegitKeyButton_Click;
            }

            // Conectar checkboxes
            if (lczxy7CustomCheckBox11 != null)
            {
                lczxy7CustomCheckBox11.CheckedChanged += HeadCheckBox_Changed;
            }
            if (lczxy7CustomCheckBox12 != null)
            {
                lczxy7CustomCheckBox12.CheckedChanged += LegitCheckBox_Changed;
            }

            keyThread = new Thread(KeyListenerLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            keyThread.Start();

            FormClosing += Form2_FormClosing;

            status.Text = "Aguardando...";
            status.ForeColor = Color.White;
        }

        // ========== CHECKBOX HANDLERS ==========
        private void HeadCheckBox_Changed(object sender, EventArgs e)
        {
            bool isChecked = lczxy7CustomCheckBox11.Checked;
            HeadEnabled = isChecked;
            if (isChecked)
            {
                ActivateHead();
            }
            else
            {
                DeactivateHead();
            }
        }

        private void LegitCheckBox_Changed(object sender, EventArgs e)
        {
            bool isChecked = lczxy7CustomCheckBox12.Checked;
            LegitEnabled = isChecked;
            if (isChecked)
            {
                ActivateLegit();
            }
            else
            {
                DeactivateLegit();
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            lock (headLock) { if (HeadActive) RestoreUnsafe(HeadOriginalValues); }
            lock (legitLock) { if (LegitActive) RestoreUnsafe(LegitOriginalValues); }
            memory.Release();
        }

        private void HeadKeyButton_Click(object sender, EventArgs e)
        {
            if (isWaitingForKey) return;
            isWaitingForKey = true;
            keySelectionMode = 1;
            if (guna2Button1 != null) guna2Button1.Text = "Pressione uma tecla...";
            status.Text = "Aguardando tecla para Head...";
            status.ForeColor = Color.Yellow;
        }

        private void LegitKeyButton_Click(object sender, EventArgs e)
        {
            if (isWaitingForKey) return;
            isWaitingForKey = true;
            keySelectionMode = 2;
            if (guna2Button2 != null) guna2Button2.Text = "Pressione uma tecla...";
            status.Text = "Aguardando tecla para Legit...";
            status.ForeColor = Color.Yellow;
        }

        private void Guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HeadMode = guna2ComboBox1.SelectedIndex;
            if (HeadActive) DeactivateHead();
            status.Text = $"Head Mode: {guna2ComboBox1.SelectedItem}";
            status.ForeColor = Color.Yellow;
        }

        private void Guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LegitMode = guna2ComboBox2.SelectedIndex;
            if (LegitActive) DeactivateLegit();
            status.Text = $"Legit Mode: {guna2ComboBox2.SelectedItem}";
            status.ForeColor = Color.Yellow;
        }

        private void CaptureKey()
        {
            if (!isWaitingForKey) return;

            for (int i = 8; i <= 255; i++)
            {
                Keys key = (Keys)i;
                if (key == Keys.None || key == Keys.LButton || key == Keys.RButton ||
                    key == Keys.MButton || key == Keys.LShiftKey || key == Keys.RShiftKey ||
                    key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.LMenu ||
                    key == Keys.RMenu || key == Keys.LWin || key == Keys.RWin)
                    continue;

                if ((GetAsyncKeyState(i) & 0x8000) != 0)
                {
                    if (keySelectionMode == 1)
                    {
                        headKey = key;
                        RunOnUi(() => { if (guna2Button1 != null) guna2Button1.Text = $"Head: {headKey}"; });
                    }
                    else if (keySelectionMode == 2)
                    {
                        legitKey = key;
                        RunOnUi(() => { if (guna2Button2 != null) guna2Button2.Text = $"Legit: {legitKey}"; });
                    }

                    isWaitingForKey = false;
                    RunOnUi(() =>
                    {
                        status.Text = $"Tecla selecionada: {key}";
                        status.ForeColor = Color.Lime;
                    });
                    break;
                }
            }
        }

        private void RunOnUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired) BeginInvoke(action);
                else action();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void KeyListenerLoop()
        {
            while (isRunning)
            {
                try
                {
                    // Monitoramento de processo — verifica se o emulador ainda está vivo
                    processCheckCounter++;
                    if (processCheckCounter >= PROCESS_CHECK_INTERVAL)
                    {
                        processCheckCounter = 0;
                        if (memory.processId > 0 && !memory.IsProcessAlive())
                        {
                            // Processo morreu — invalidar tudo
                            RunOnUi(() => InvalidateOnProcessLost());
                        }
                    }

                    if (isWaitingForKey)
                    {
                        CaptureKey();
                        Thread.Sleep(50);
                        continue;
                    }

                    bool headPressed = (GetAsyncKeyState((int)headKey) & 0x8000) != 0;
                    bool legitPressed = (GetAsyncKeyState((int)legitKey) & 0x8000) != 0;

                    // Head keybind
                    if (HeadEnabled)
                    {
                        if (HeadMode == 0)
                        {
                            if (headPressed && !isHeadKeyPressed)
                            {
                                isHeadKeyPressed = true;
                                RunOnUi(ActivateHead);
                            }
                            else if (!headPressed && isHeadKeyPressed)
                            {
                                isHeadKeyPressed = false;
                                RunOnUi(DeactivateHead);
                            }
                        }
                        else
                        {
                            if (headPressed && !isHeadKeyPressed)
                            {
                                isHeadKeyPressed = true;
                                RunOnUi(ToggleHead);
                            }
                            else if (!headPressed && isHeadKeyPressed)
                            {
                                isHeadKeyPressed = false;
                            }
                        }
                    }

                    // Legit keybind
                    if (LegitEnabled)
                    {
                        if (LegitMode == 0)
                        {
                            if (legitPressed && !isLegitKeyPressed)
                            {
                                isLegitKeyPressed = true;
                                RunOnUi(ActivateLegit);
                            }
                            else if (!legitPressed && isLegitKeyPressed)
                            {
                                isLegitKeyPressed = false;
                                RunOnUi(DeactivateLegit);
                            }
                        }
                        else
                        {
                            if (legitPressed && !isLegitKeyPressed)
                            {
                                isLegitKeyPressed = true;
                                RunOnUi(ToggleLegit);
                            }
                            else if (!legitPressed && isLegitKeyPressed)
                            {
                                isLegitKeyPressed = false;
                            }
                        }
                    }

                    // Force loop — write direto sem string parsing
                    if (HeadActive)
                    {
                        lock (headLock)
                        {
                            if (HeadFoundAddresses.Count > 0)
                                ForceHeadFast();
                        }
                    }
                    if (LegitActive)
                    {
                        lock (legitLock)
                        {
                            if (LegitFoundAddresses.Count > 0)
                                ForceLegitFast();
                        }
                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro: {ex.Message}");
                }
            }
        }

        // ========== PROCESS LOST ==========
        private void InvalidateOnProcessLost()
        {
            lock (headLock)
            {
                HeadFoundAddresses.Clear();
                HeadOriginalValues.Clear();
                HeadActive = false;
            }
            lock (legitLock)
            {
                LegitFoundAddresses.Clear();
                LegitOriginalValues.Clear();
                LegitActive = false;
            }
            memory.Release();

            status.Text = "Processo perdido. Reabra o emulador + jogo.";
            status.ForeColor = Color.Red;
            if (guna2Button1 != null) guna2Button1.BackColor = Color.Transparent;
            if (guna2Button2 != null) guna2Button2.BackColor = Color.Transparent;
        }

        private bool AttachProcess()
        {
            // Libera handle anterior se existir
            memory.Release();

            if (memory.SetProcess(TargetProcesses))
            {
                status.Text = "Processo anexado!";
                status.ForeColor = Color.Lime;
                return true;
            }

            foreach (var name in TargetProcesses)
            {
                string clean = name.Replace(".exe", "");
                var procs = Process.GetProcessesByName(clean);
                if (procs.Length > 0 && memory.SetProcess(new[] { clean, name }))
                {
                    status.Text = $"Anexado: {procs[0].ProcessName} (PID {procs[0].Id})";
                    status.ForeColor = Color.Lime;
                    return true;
                }
            }

            var candidates = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        string n = p.ProcessName.ToLowerInvariant();
                        return n.Contains("player") || n.Contains("blue") ||
                               n.Contains("nox") || n.Contains("memu") ||
                               n.Contains("ld") || n.Contains("android") ||
                               n.Contains("emulator") || n.Contains("aow");
                    }
                    catch { return false; }
                })
                .Select(p => p.ProcessName)
                .Distinct()
                .ToArray();

            if (candidates.Length > 0 && memory.SetProcess(candidates))
            {
                status.Text = $"Anexado (fallback): {candidates[0]}";
                status.ForeColor = Color.Lime;
                return true;
            }

            status.Text = "Processo não encontrado! Abra o emulador + jogo.";
            status.ForeColor = Color.Red;
            return false;
        }

        private async System.Threading.Tasks.Task<bool> EnsureAddresses(List<long> cache, object cacheLock)
        {
            lock (cacheLock)
            {
                if (cache.Count > 0) return true;
            }

            var results = await memory.AoBScan(Pattern);
            if (!results.Any())
            {
                status.Text = "Nenhum padrão encontrado!";
                status.ForeColor = Color.Red;
                return false;
            }

            lock (cacheLock)
            {
                cache.Clear();
                cache.AddRange(results);
            }
            status.Text = $"Padrão OK — {results.Count()} addr";
            status.ForeColor = Color.Lime;
            return true;
        }

        private void Backup(List<long> addresses, Dictionary<long, int> originals, params int[] offsets)
        {
            foreach (long baseAddr in addresses)
            {
                foreach (int off in offsets)
                {
                    long addr = baseAddr + off;
                    if (!originals.ContainsKey(addr))
                        originals[addr] = memory.ReadInt(addr);
                }
            }
        }

        // ========== FAST WRITE — sem string parsing ==========
        private void ForceHeadFast()
        {
            foreach (long baseAddr in HeadFoundAddresses)
            {
                int headValue = memory.ReadInt(baseAddr + OFF_HEAD);
                memory.WriteInt(baseAddr + OFF_CHEST, headValue);
                memory.WriteInt(baseAddr + OFF_NECK, headValue);
                memory.WriteInt(baseAddr + OFF_HEAD, headValue);
            }
        }

        private void ForceLegitFast()
        {
            foreach (long baseAddr in LegitFoundAddresses)
            {
                int neck = memory.ReadInt(baseAddr + OFF_NECK);
                memory.WriteInt(baseAddr + OFF_CHEST, neck);
            }
        }

        private void RestoreUnsafe(Dictionary<long, int> originals)
        {
            foreach (var kvp in originals)
                memory.WriteInt(kvp.Key, kvp.Value);
            originals.Clear();
        }

        private async void ActivateHead()
        {
            if (HeadActive) return;
            if (!AttachProcess()) return;
            if (!await EnsureAddresses(HeadFoundAddresses, headLock)) return;

            lock (headLock)
            {
                Backup(HeadFoundAddresses, HeadOriginalValues, OFF_HEAD, OFF_NECK, OFF_CHEST);
            }

            HeadActive = true;
            status.Text = $"Head LOCK [{headKey}] {(HeadMode == 0 ? "(Hold)" : "(Toggle)")}";
            status.ForeColor = Color.Lime;
            if (guna2Button1 != null) guna2Button1.BackColor = Color.Lime;
        }

        private void DeactivateHead()
        {
            if (!HeadActive) return;
            lock (headLock) { RestoreUnsafe(HeadOriginalValues); }
            HeadActive = false;
            status.Text = $"Head OFF [{headKey}]";
            status.ForeColor = Color.White;
            if (guna2Button1 != null) guna2Button1.BackColor = Color.Transparent;
        }

        private void ToggleHead()
        {
            if (!HeadActive) ActivateHead();
            else DeactivateHead();
        }

        private async void ActivateLegit()
        {
            if (LegitActive) return;
            if (!AttachProcess()) return;
            if (!await EnsureAddresses(LegitFoundAddresses, legitLock)) return;

            lock (legitLock)
            {
                Backup(LegitFoundAddresses, LegitOriginalValues, OFF_CHEST);
            }
            LegitActive = true;
            status.Text = $"Legit ON [{legitKey}] {(LegitMode == 0 ? "(Hold)" : "(Toggle)")}";
            status.ForeColor = Color.Lime;
            if (guna2Button2 != null) guna2Button2.BackColor = Color.Lime;
        }

        private void DeactivateLegit()
        {
            if (!LegitActive) return;
            lock (legitLock) { RestoreUnsafe(LegitOriginalValues); }
            LegitActive = false;
            status.Text = $"Legit OFF [{legitKey}]";
            status.ForeColor = Color.White;
            if (guna2Button2 != null) guna2Button2.BackColor = Color.Transparent;
        }

        private void ToggleLegit()
        {
            if (!LegitActive) ActivateLegit();
            else DeactivateLegit();
        }
    }
}
