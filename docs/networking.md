# Networking

The networking module provides TCP, UDP, HTTP, and binary packet serialization.

## TCP Server

```csharp
var server = new TcpServer();

server.OnClientConnected += (connectionId) =>
{
    logger.Info("Net", $"Client {connectionId} connected");
};

server.OnClientDisconnected += (connectionId) =>
{
    logger.Info("Net", $"Client {connectionId} disconnected");
};

server.Start(port: 7777);
```

### Receiving Messages

Poll for messages each frame (non-blocking):

```csharp
while (server.TryDequeue(out int connId, out byte[] data))
{
    // Process packet from client connId
}
```

### Sending Messages

```csharp
server.Send(connectionId, messageBytes);     // To one client
server.Broadcast(messageBytes);              // To all clients
```

## TCP Client

```csharp
var client = new TcpClient();
client.Connect("127.0.0.1", 7777);

client.Send(messageBytes);

while (client.TryReceive(out byte[] data))
{
    // Process server message
}

client.Disconnect();
```

## UDP Server / Client

Same pattern as TCP but for unreliable, low-latency communication (movement updates, etc.).

## HTTP Helper

Simple HTTP requests:

```csharp
var http = new HttpHelper();

byte[] response = await http.Get("https://api.example.com/data");
byte[] response = await http.Post("https://api.example.com/data", body);
```

## Packet Serialization

Zero-alloc binary serialization using Span:

### Writing Packets

```csharp
var writer = new PacketWriter();
writer.WriteByte(PacketType.PlayerMove);
writer.WriteInt32(playerId);
writer.WriteFloat(x);
writer.WriteFloat(y);
writer.WriteFloat(z);
writer.WriteString("hello");

byte[] packet = writer.ToArray();
server.Send(connId, packet);
```

### Reading Packets

```csharp
var reader = new PacketReader(data);
byte type = reader.ReadByte();
int id = reader.ReadInt32();
float x = reader.ReadFloat();
float y = reader.ReadFloat();
float z = reader.ReadFloat();
string msg = reader.ReadString();
```

## Architecture

- Use **TCP** for reliable messages: chat, inventory, login, game state
- Use **UDP** for frequent unreliable messages: position updates, input state
- Use **PacketWriter/Reader** for efficient binary serialization instead of JSON/XML
- Poll `TryDequeue`/`TryReceive` in your game update loop — no callbacks on background threads
