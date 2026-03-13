using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Engine.Logging;

namespace Engine.Network.Udp;

public sealed class UdpServer : IDisposable
{
    private readonly ILogger _logger;
    private System.Net.Sockets.UdpClient? _udp;
    private readonly ConcurrentQueue<(IPEndPoint sender, byte[] data)> _incomingMessages = new();
    private CancellationTokenSource? _cts;

    public UdpServer(ILogger logger)
    {
        _logger = logger;
    }

    public void Start(int port)
    {
        _udp = new System.Net.Sockets.UdpClient(port);
        _cts = new CancellationTokenSource();
        _ = ReceiveAsync(_cts.Token);
        _logger.Info("UdpServer", $"Listening on port {port}");
    }

    private async Task ReceiveAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udp!.ReceiveAsync(ct);
                _incomingMessages.Enqueue((result.RemoteEndPoint, result.Buffer));
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.Error("UdpServer", $"Receive error: {ex.Message}");
            }
        }
    }

    public bool TryDequeue(out IPEndPoint? sender, out byte[] data)
    {
        if (_incomingMessages.TryDequeue(out var msg))
        {
            sender = msg.sender;
            data = msg.data;
            return true;
        }
        sender = null;
        data = Array.Empty<byte>();
        return false;
    }

    public void Send(byte[] data, IPEndPoint target)
    {
        _udp?.Send(data, data.Length, target);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _udp?.Dispose();
        _cts?.Dispose();
    }
}
