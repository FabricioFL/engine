using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Engine.Logging;

namespace Engine.Network.Tcp;

public sealed class TcpClient : IDisposable
{
    private readonly ILogger _logger;
    private System.Net.Sockets.TcpClient? _client;
    private NetworkStream? _stream;
    private readonly ConcurrentQueue<byte[]> _incomingMessages = new();
    private CancellationTokenSource? _cts;

    public bool IsConnected => _client?.Connected ?? false;

    public TcpClient(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(string host, int port)
    {
        _client = new System.Net.Sockets.TcpClient();
        await _client.ConnectAsync(host, port);
        _stream = _client.GetStream();
        _cts = new CancellationTokenSource();
        _ = ReceiveAsync(_cts.Token);
        _logger.Info("TcpClient", $"Connected to {host}:{port}");
    }

    private async Task ReceiveAsync(CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (!ct.IsCancellationRequested && _client!.Connected)
            {
                int read = await _stream!.ReadAsync(buffer.AsMemory(0, 4096), ct);
                if (read == 0) break;

                var data = new byte[read];
                Buffer.BlockCopy(buffer, 0, data, 0, read);
                _incomingMessages.Enqueue(data);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Warning("TcpClient", $"Receive error: {ex.Message}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public bool TryDequeue(out byte[] data)
    {
        return _incomingMessages.TryDequeue(out data!);
    }

    public void Send(ReadOnlySpan<byte> data)
    {
        _stream?.Write(data);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _client?.Dispose();
        _cts?.Dispose();
    }
}
