
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AttendanceClientTest
{
    public partial class Form1 : Form
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        public Form1()
        {
            InitializeComponent();

            btnConnect.Click += btnConnect_Click;
            btnDisconnect.Click += btnDisconnect_Click;
            btnCheckIn.Click += btnCheckIn_Click;

            btnDisconnect.Enabled = false;
            btnCheckIn.Enabled = false;

            FormClosing += Form1_FormClosing;

            Log("Client is not connected.");
        }

        private void Log(string message)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(message)));
                return;
            }

            txtLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"
            );
        }

        // Kết nối đến Server
        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            btnConnect.Enabled = false;

            TcpClient client = new TcpClient();
            _client = client;

            try
            {
                string serverIP = txtServerIP.Text.Trim();

                if (string.IsNullOrWhiteSpace(serverIP))
                    throw new Exception("Please enter Server IP.");

                Log($"Connecting to {serverIP}:8888...");

                await client.ConnectAsync(serverIP, 8888);

                if (!ReferenceEquals(_client, client))
                    return;

                _stream = client.GetStream();

                _reader = new StreamReader(
                    _stream, Encoding.UTF8, false, 1024, true);

                _writer = new StreamWriter(
                    _stream, new UTF8Encoding(false), 1024, true)
                {
                    AutoFlush = true
                };

                Log("Connected to server successfully.");

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnCheckIn.Enabled = true;

                // Bắt đầu lắng nghe Server liên tục
                _ = ReceiveMessagesAsync(client, _reader);
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(_client, client))
                {
                    Log($"Connection failed: {ex.Message}");
                    CloseConnection();
                }
            }
        }

        // Luôn lắng nghe phản hồi từ Server
        private async Task ReceiveMessagesAsync(
            TcpClient client,
            StreamReader reader)
        {
            try
            {
                while (ReferenceEquals(_client, client))
                {
                    string? response = await reader.ReadLineAsync();

                    // Server đã đóng kết nối
                    if (response == null)
                    {
                        HandleServerDisconnected(client);
                        return;
                    }

                    Log($"Server response: {response}");
                }
            }
            catch (IOException)
            {
                HandleServerDisconnected(client);
            }
            catch (SocketException)
            {
                HandleServerDisconnected(client);
            }
            catch (ObjectDisposedException)
            {
                // Client đã chủ động đóng kết nối
            }
        }

        // Xử lý khi Server ngắt kết nối
        private void HandleServerDisconnected(TcpClient client)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(
                    () => HandleServerDisconnected(client)));
                return;
            }

            // Tránh đóng nhầm kết nối mới
            if (!ReferenceEquals(_client, client))
                return;

            Log("Server disconnected or connection lost.");

            CloseConnection();

            Log("Client is ready to reconnect.");
        }

        // Gửi lệnh CHECKIN
        private async void btnCheckIn_Click(object? sender, EventArgs e)
        {
            if (_client == null || _writer == null)
            {
                Log("Please connect to the server first.");
                return;
            }

            TcpClient client = _client;
            StreamWriter writer = _writer;

            btnCheckIn.Enabled = false;

            try
            {
                await writer.WriteLineAsync("CHECKIN");

                Log("Sent: CHECKIN");

                // Phản hồi được nhận bởi ReceiveMessagesAsync()
            }
            catch (IOException ex)
            {
                Log($"Communication error: {ex.Message}");
                HandleServerDisconnected(client);
            }
            catch (SocketException ex)
            {
                Log($"Socket error: {ex.Message}");
                HandleServerDisconnected(client);
            }
            catch (ObjectDisposedException)
            {
                HandleServerDisconnected(client);
            }
            finally
            {
                if (ReferenceEquals(_client, client))
                    btnCheckIn.Enabled = true;
            }
        }

        // Ngắt kết nối thủ công
        private void btnDisconnect_Click(object? sender, EventArgs e)
        {
            CloseConnection();
            Log("Disconnected from server.");
        }

        // Giải phóng kết nối
        private void CloseConnection()
        {
            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }

            _reader = null;
            _writer = null;
            _stream = null;
            _client = null;

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnCheckIn.Enabled = false;
        }

        private void Form1_FormClosing(
            object? sender,
            FormClosingEventArgs e)
        {
            CloseConnection();
        }
    }
}
