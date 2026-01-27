using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Photo8
{
    public partial class PDFToSheet_PdfUI : Form
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly List<string> _selectedFiles = new List<string>();
        private string _token = null;

        public PDFToSheet_PdfUI()
        {
            InitializeComponent();

            _http.Timeout = TimeSpan.FromMinutes(30);

            // ✅ Load config (ưu tiên LastServerUrl, nếu trống thì DefaultServerUrl)
            LoadConfigToUi();

            SetUiState();
            AppendLine("Ready.");
        }

        private void LoadConfigToUi()
        {
            try
            {
                var defaultUrl = Properties.Settings.Default.DefaultServerUrl;
                var lastUrl = Properties.Settings.Default.LastServerUrl;

                pdf_txtServerUrl.Text = string.IsNullOrWhiteSpace(lastUrl) ? defaultUrl : lastUrl;
                pdf_txtUsername.Text = Properties.Settings.Default.LastUsername;
            }
            catch
            {
                // Nếu Settings chưa tạo đúng, vẫn cho app chạy
                if (string.IsNullOrWhiteSpace(pdf_txtServerUrl.Text))
                    pdf_txtServerUrl.Text = "http://127.0.0.1:8000";
            }
        }

        private void SaveUserSettings()
        {
            try
            {
                Properties.Settings.Default.LastServerUrl = pdf_txtServerUrl.Text.Trim();
                Properties.Settings.Default.LastUsername = pdf_txtUsername.Text.Trim();
                Properties.Settings.Default.Save();
            }
            catch
            {
                // ignore nếu Settings chưa setup
            }
        }

        private void SetUiState()
        {
            bool loggedIn = !string.IsNullOrEmpty(_token);
            pdf_btnChoosePdfs.Enabled = loggedIn;
            pdf_btnUploadRun.Enabled = loggedIn;

            if (!loggedIn)
                pdf_lblStatus.Text = "Status: Not logged in";
        }

        private void SetBusy(bool busy, string statusText = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetBusy(busy, statusText)));
                return;
            }

            pdf_txtServerUrl.Enabled = !busy;
            pdf_txtUsername.Enabled = !busy;
            pdf_txtPassword.Enabled = !busy;
            pdf_btnLogin.Enabled = !busy;

            pdf_btnChoosePdfs.Enabled = !busy && !string.IsNullOrEmpty(_token);
            pdf_btnUploadRun.Enabled = !busy && !string.IsNullOrEmpty(_token);
            pdf_btnClearLog.Enabled = !busy;
            pdf_btnCopyLog.Enabled = !busy;

            pdf_progressBar1.Visible = busy;

            if (!string.IsNullOrEmpty(statusText))
                pdf_lblStatus.Text = statusText;
        }

        private void AppendLine(string s)
        {
            if (pdf_txtLog.InvokeRequired)
            {
                pdf_txtLog.BeginInvoke(new Action(() => AppendLine(s)));
                return;
            }

            pdf_txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");
        }

        private async void pdf_btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var server = pdf_txtServerUrl.Text.Trim().TrimEnd('/');
                var user = pdf_txtUsername.Text.Trim();
                var pass = pdf_txtPassword.Text;

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Please enter server url, username, password.");
                    return;
                }

                // ✅ Save config mỗi lần login
                SaveUserSettings();

                SetBusy(true, "Status: Logging in...");
                AppendLine("Login started...");

                _token = await Login(server, user, pass);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                AppendLine("Login success.");
                SetBusy(false, "Status: Logged in");
                SetUiState();
            }
            catch (Exception ex)
            {
                _token = null;
                AppendLine("Login failed: " + ex.Message);
                SetBusy(false, "Status: Not logged in");
                SetUiState();
                MessageBox.Show("Login failed: " + ex.Message);
            }
        }

        private async Task<string> Login(string server, string username, string password)
        {
            var url = $"{server}/api/v1/auth/login";
            var payload = new { username = username, password = password };
            var json = JsonSerializer.Serialize(payload);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var resp = await _http.PostAsync(url, content);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"HTTP {(int)resp.StatusCode}: {body}");

                using (var doc = JsonDocument.Parse(body))
                    return doc.RootElement.GetProperty("access_token").GetString();
            }
        }

        private void pdf_btnChoosePdfs_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "PDF files (*.pdf)|*.pdf";
                dlg.Multiselect = true;
                dlg.Title = "Select PDF files to upload";

                if (dlg.ShowDialog() != DialogResult.OK) return;

                _selectedFiles.Clear();
                _selectedFiles.AddRange(dlg.FileNames);

                pdf_lstFiles.Items.Clear();
                foreach (var f in _selectedFiles) pdf_lstFiles.Items.Add(f);

                AppendLine($"Selected {_selectedFiles.Count} file(s).");
                pdf_lblStatus.Text = $"Status: Selected {_selectedFiles.Count} file(s)";
            }
        }

        private async void pdf_btnUploadRun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("Please login first.");
                return;
            }

            if (_selectedFiles.Count == 0)
            {
                MessageBox.Show("Please choose PDFs first.");
                return;
            }

            pdf_txtLog.Clear();
            AppendLine("Upload started...");

            try
            {
                var server = pdf_txtServerUrl.Text.Trim().TrimEnd('/');

                SetBusy(true, "Status: Uploading / Running...");

                var jobId = await CreateJobUploadFiles(server, _selectedFiles);
                AppendLine($"Created job: {jobId}");

                AppendLine("Done");
                SetBusy(false, "Status: Done");
            }
            catch (Exception ex)
            {
                AppendLine("Error: " + ex.Message);
                SetBusy(false, "Status: Error");
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                SetUiState();
            }
        }

        private async Task<string> CreateJobUploadFiles(string server, List<string> files)
        {
            var url = $"{server}/api/v1/jobs";
            using (var form = new MultipartFormDataContent())
            {
                foreach (var path in files)
                {
                    var fileName = Path.GetFileName(path);
                    var bytes = File.ReadAllBytes(path);

                    var part = new ByteArrayContent(bytes);
                    part.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                    form.Add(part, "files", fileName);
                }

                var resp = await _http.PostAsync(url, form);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"Upload failed HTTP {(int)resp.StatusCode}: {body}");

                using (var doc = JsonDocument.Parse(body))
                    return doc.RootElement.GetProperty("job_id").GetString();
            }
        }

        private void pdf_btnClearLog_Click(object sender, EventArgs e)
        {
            pdf_txtLog.Clear();
        }

        private void pdf_btnCopyLog_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pdf_txtLog.Text))
                Clipboard.SetText(pdf_txtLog.Text);
        }

        // ✅ Wrapper handlers nếu Designer của bạn vẫn gọi tên cũ (btnChoosePdfs_Click...)
        // Nếu bạn đã sửa Designer để gọi pdf_btn... thì có thể xóa block này.
        private void btnLogin_Click(object sender, EventArgs e) => pdf_btnLogin_Click(sender, e);
        private void btnChoosePdfs_Click(object sender, EventArgs e) => pdf_btnChoosePdfs_Click(sender, e);
        private void btnUploadRun_Click(object sender, EventArgs e) => pdf_btnUploadRun_Click(sender, e);
        private void btnClearLog_Click(object sender, EventArgs e) => pdf_btnClearLog_Click(sender, e);
        private void btnCopyLog_Click(object sender, EventArgs e) => pdf_btnCopyLog_Click(sender, e);
    }
}