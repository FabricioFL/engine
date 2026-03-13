using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Engine.Logging;

namespace Engine.Network.Udp;

public sealed class UdpClient : IDisposable
{
    private readonly ILogger _logger;
    private System.Net.Sockets.UdpClient? _udp;
    private IPEndPoint? _serverEndpoint;
    private readonly ConcurrentQueue<byte[]> _incomingMessages = new();
    private CancellationTokenSource? _cts;

    public UdpClient(ILogger logger)
    {
        _logger = logger;
    }

    public void Connect(string host, int port)
    {
        _serverEndpoint = new IPEndPoint(IPAddress.Parse(host), port);
        _udp = new System.Net.Sockets.UdpClient();
        _udp.Connect(_serverEndpoint);
        _cts = new CancellationTokenSource();
        _ = ReceiveAsync(_cts.Token);
        _logger.Info("UdpClient", $"Connected to {host}:{port}");
    }

    private async Task ReceiveAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udp!.ReceiveAsync(ct);
                _incomingMessages.Enqueue(result.Buffer);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.Warning("UdpClient", $"Receive error: {ex.Message}");
            }
        }
    }

    public bool TryDequeue(out byte[] data)
    {
        return _incomingMessages.TryDequeue(out data!);
    }

    public void Send(byte[] data)
    {
        _udp?.Send(data, data.Length);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _udp?.Dispose();
        _cts?.Dispose();
    }
}
