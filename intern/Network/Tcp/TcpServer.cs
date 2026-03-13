using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Engine.Logging;

namespace Engine.Network.Tcp;

public sealed class TcpServer : IDisposable
{
    private readonly ILogger _logger;
    private TcpListener? _listener;
    private readonly ConcurrentDictionary<int, TcpConnection> _connections = new();
    private readonly ConcurrentQueue<(int connectionId, byte[] data)> _incomingMessages = new();
    private CancellationTokenSource? _cts;
    private int _nextConnectionId;

    public event Action<int>? OnClientConnected;
    public event Action<int>? OnClientDisconnected;

    public TcpServer(ILogger logger)
    {
        _logger = logger;
    }

    public void Start(int port)
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _ = AcceptClientsAsync(_cts.Token);
        _logger.Info("TcpServer", $"Listening on port {port}");
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(ct);
                int id = Interlocked.Increment(ref _nextConnectionId);
                var conn = new TcpConnection(id, client, _incomingMessages, _logger);
                _connections[id] = conn;
                conn.StartReceiving(ct);
                OnClientConnected?.Invoke(id);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.Error("TcpServer", $"Accept error: {ex.Message}");
            }
        }
    }

    public bool TryDequeue(out int connectionId, out byte[] data)
    {
        if (_incomingMessages.TryDequeue(out var msg))
        {
            connectionId = msg.connectionId;
            data = msg.data;
            return true;
        }
        connectionId = 0;
        data = Array.Empty<byte>();
        return false;
    }

    public void Send(int connectionId, ReadOnlySpan<byte> data)
    {
        if (_connections.TryGetValue(connectionId, out var conn))
            conn.Send(data);
    }

    public void Broadcast(ReadOnlySpan<byte> data)
    {
        foreach (var conn in _connections.Values)
            conn.Send(data);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        foreach (var conn in _connections.Values)
            conn.Dispose();
        _listener?.Stop();
        _cts?.Dispose();
    }
}
