using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Witsml.Common;
using WitsmlODViewer.Services;
using WitsmlODViewer.Witsml141;

namespace WitsmlODViewer.WinForms
{
    public class MainForm : Form
    {
        private const string DefaultBasePath = @"C:\Users\maxim\CABL y Cubits Dropbox\Oil & Gas";
        private const string DefaultConnectionString = "Server=.\\SQLExpress;Database=WitsmlData;Trusted_Connection=True;TrustServerCertificate=True;";

        private ComboBox _comboType = null!;
        private TextBox _txtBasePath = null!;
        private Button _btnBrowse = null!;
        private Button _btnCount = null!;
        private Button _btnProcess = null!;
        private Label _lblCount = null!;
        private ProgressBar _progressBar = null!;
        private TextBox _txtLog = null!;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "WITSML 1.4.1 — Procesador";
            Size = new Size(700, 520);
            MinimumSize = new Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 6
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++)
                mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var row = 0;

            // Tipo de objeto
            mainPanel.Controls.Add(new Label { Text = "Tipo de objeto:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            _comboType = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            var types = new[] { "wells", "wellbores", "trajectories", "trajectoryStations", "mudLogs", "geologyIntervals", "lithologies", "logs", "rigs", "tubulars", "tubularComponents", "wbGeometrys", "wbGeometrySections", "bhaRuns", "messages", "attachments", "formationMarkers" };
            foreach (var key in types)
                _comboType.Items.Add(new ComboItem(key, WitsmlFileCounter.ObjectTypeDisplayNames[key]));
            _comboType.SelectedIndex = 0;
            mainPanel.Controls.Add(_comboType, 1, row);
            row++;

            // Carpeta base
            mainPanel.Controls.Add(new Label { Text = "Carpeta base:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            var pathPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            _txtBasePath = new TextBox
            {
                Text = DefaultBasePath,
                Width = 400,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true
            };
            _btnBrowse = new Button { Text = "Examinar...", AutoSize = true };
            _btnBrowse.Click += BtnBrowse_Click;
            pathPanel.Controls.Add(_txtBasePath);
            pathPanel.Controls.Add(_btnBrowse);
            mainPanel.Controls.Add(pathPanel, 1, row);
            row++;

            // Contar archivos
            mainPanel.Controls.Add(new Label { Text = "Archivos:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            var countPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            _btnCount = new Button { Text = "Contar", AutoSize = true };
            _btnCount.Click += BtnCount_Click;
            _lblCount = new Label { Text = "—", AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            countPanel.Controls.Add(_btnCount);
            countPanel.Controls.Add(new Label { Text = "  " });
            countPanel.Controls.Add(_lblCount);
            mainPanel.Controls.Add(countPanel, 1, row);
            row++;

            // Procesar
            mainPanel.Controls.Add(new Label { Text = "Acción:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            _btnProcess = new Button { Text = "Cargar en BD (1.4.1)", AutoSize = true };
            _btnProcess.Click += BtnProcess_Click;
            mainPanel.Controls.Add(_btnProcess, 1, row);
            row++;

            // Barra de progreso
            mainPanel.Controls.Add(new Label { Text = "Progreso:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            _progressBar = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Continuous };
            mainPanel.Controls.Add(_progressBar, 1, row);
            row++;

            // Log
            mainPanel.Controls.Add(new Label { Text = "Log:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            _txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray
            };
            mainPanel.Controls.Add(_txtLog, 1, row);

            Controls.Add(mainPanel);

            Log("Aplicación iniciada. Selecciona un tipo de objeto y haz clic en 'Contar'.");
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Seleccionar carpeta base (Oil & Gas)",
                SelectedPath = _txtBasePath.Text,
                UseDescriptionForTitle = true
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                _txtBasePath.Text = dlg.SelectedPath;
        }

        private async void BtnCount_Click(object? sender, EventArgs e)
        {
            var basePath = _txtBasePath.Text;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                MessageBox.Show("La carpeta no existe. Verifica la ruta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = (ComboItem)_comboType.SelectedItem!;
            _btnCount.Enabled = false;
            _lblCount.Text = "Contando...";

            try
            {
                var counter = new WitsmlFileCounter(basePath);
                var count141 = await Task.Run(() => counter.CountWitsml141ByContent(item.Key), CancellationToken.None);
                _lblCount.Text = count141 > 0 ? $"{count141:N0} (1.4.1)" : "0";
                Log($"Tipo: {item.Display} → {count141:N0} archivo(s) WITSML 1.4.1 (2.x excluidos).");
            }
            catch (Exception ex)
            {
                _lblCount.Text = "—";
                Log($"Error al contar: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnCount.Enabled = true;
            }
        }

        private async void BtnProcess_Click(object? sender, EventArgs e)
        {
            var basePath = _txtBasePath.Text;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                MessageBox.Show("La carpeta no existe. Verifica la ruta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = (ComboItem)_comboType.SelectedItem!;
            var connStr = Environment.GetEnvironmentVariable("WITSML_CONNECTION_STRING") ?? DefaultConnectionString;

            _btnProcess.Enabled = false;
            _btnCount.Enabled = false;
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 30;
            Log($"Procesando {item.Display}...");

            try
            {
                var counter = new WitsmlFileCounter(basePath);
                var files = await Task.Run(() => counter.GetFilesForTypeByContent(item.Key).ToList(), CancellationToken.None);
                var total = files.Count;

                if (total == 0)
                {
                    Log("No hay archivos de este tipo para procesar.");
                    return;
                }

                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.Maximum = total;
                _progressBar.Value = 0;

                var processor = new Witsml141Processor();
                var processed = 0;
                var errors = 0;
                var skipped21 = 0;

                foreach (var file in files)
                {
                    try
                    {
                        var xmlContent = await File.ReadAllTextAsync(file);
                        if (WitsmlXmlVersionDetector.IsWitsml21(xmlContent))
                        {
                            skipped21++;
                            Log($"  ⊘ {Path.GetFileName(file)}: WITSML 2.x omitido (usar producto ETP/2.1).");
                            continue;
                        }

                        using var repo = new Witsml141Repository(connStr);
                        await processor.ProcessWitsmlFileAsync(file, repo);
                        processed++;
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        Log($"  ✗ {Path.GetFileName(file)}: {ex.Message}");
                    }

                    _progressBar.Value = processed + errors;
                }

                Log($"✓ Completado: {processed} archivo(s) procesado(s), {skipped21} omitido(s) (2.x), {errors} error(es).");
                MessageBox.Show($"{total} archivo(s) encontrado(s).\nProcesados (1.4.1): {processed}\nOmitidos (2.x): {skipped21}\nErrores: {errors}", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnProcess.Enabled = true;
                _btnCount.Enabled = true;
                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.MarqueeAnimationSpeed = 0;
                _progressBar.Value = 0;
            }
        }

        private void Log(string message)
        {
            if (_txtLog.InvokeRequired)
            {
                _txtLog.Invoke(() => Log(message));
                return;
            }
            _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private record ComboItem(string Key, string Display)
        {
            public override string ToString() => Display;
        }
    }
}
