using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Engine.Logging;

namespace Engine.Network.Tcp;

public sealed class TcpConnection : IDisposable
{
    public int Id { get; }
    private readonly System.Net.Sockets.TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly ConcurrentQueue<(int, byte[])> _messageQueue;
    private readonly ILogger _logger;

    public TcpConnection(int id, System.Net.Sockets.TcpClient client,
        ConcurrentQueue<(int, byte[])> messageQueue, ILogger logger)
    {
        Id = id;
        _client = client;
        _stream = client.GetStream();
        _messageQueue = messageQueue;
        _logger = logger;
    }

    public void StartReceiving(CancellationToken ct)
    {
        _ = ReceiveAsync(ct);
    }

    private async Task ReceiveAsync(CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (!ct.IsCancellationRequested && _client.Connected)
            {
                int read = await _stream.ReadAsync(buffer.AsMemory(0, 4096), ct);
                if (read == 0) break;

                var data = new byte[read];
                Buffer.BlockCopy(buffer, 0, data, 0, read);
                _messageQueue.Enqueue((Id, data));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Warning("TcpConnection", $"Connection {Id} error: {ex.Message}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Send(ReadOnlySpan<byte> data)
    {
        try
        {
            _stream.Write(data);
        }
        catch (Exception ex)
        {
            _logger.Warning("TcpConnection", $"Send error on {Id}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
    }
}
