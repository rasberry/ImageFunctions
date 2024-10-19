/*
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Pipes;

namespace ImageFunctions.ComfiUINodes;

public class PipeServer : IPipeServer
{
	private const int MaxNumberOfServerInstances = 10;
	private readonly string _pipeName;
	private readonly SynchronizationContext _synchronizationContext;
	private readonly IDictionary<string, IPipeServer> _servers;

	public PipeServer(string serverName)
	{
		_pipeName = serverName;
		_synchronizationContext = AsyncOperationManager.SynchronizationContext;
		_servers = new ConcurrentDictionary<string, IPipeServer>();
	}

	public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
	public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
	public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;

	public string ServerId { get { return _pipeName; }}

	public void Start()
	{
		StartNamedPipeServer();
	}

	public void Stop()
	{
		foreach (var server in _servers.Values) {
			try {
				UnregisterFromServerEvents(server);
				server.Stop();
			}
			catch (Exception) {
				//Logger.Error("Fialed to stop server");
			}
		}

		_servers.Clear();
	}

	/// <summary>
	/// Starts a new NamedPipeServerStream that waits for connection
	/// </summary>
	private void StartNamedPipeServer()
	{
		var server = new InternalPipeServer(_pipeName, MaxNumberOfServerInstances);
		_servers[server.Id] = server;

		server.ClientConnectedEvent += ClientConnectedHandler;
		server.ClientDisconnectedEvent += ClientDisconnectedHandler;
		server.MessageReceivedEvent += MessageReceivedHandler;

		server.Start();
	}

	/// <summary>
	/// Stops the server that belongs to the given id
	/// </summary>
	/// <param name="id">Server ID</param>
	private void StopNamedPipeServer(string id)
	{
		UnregisterFromServerEvents(_servers[id]);
		_servers[id].Stop();
		_servers.Remove(id);
	}

	/// <summary>
	/// Unregisters from the given server's events
	/// </summary>
	/// <param name="server">Server</param>
	private void UnregisterFromServerEvents(IPipeServer server)
	{
		server.ClientConnectedEvent -= ClientConnectedHandler;
		server.ClientDisconnectedEvent -= ClientDisconnectedHandler;
		server.MessageReceivedEvent -= MessageReceivedHandler;
	}

	/// <summary>
	/// Fires MessageReceivedEvent in the current thread
	/// </summary>
	/// <param name="eventArgs">Message event Args</param>
	private void OnMessageReceived(MessageReceivedEventArgs eventArgs)
	{
		_synchronizationContext.Post(e => MessageReceivedEvent.SafeInvoke(this, (MessageReceivedEventArgs)e), eventArgs);
	}

	/// <summary>
	/// Fires ClientConnectedEvent in the current thread
	/// </summary>
	/// <param name="eventArgs">Client connected event Args</param>
	private void OnClientConnected(ClientConnectedEventArgs eventArgs)
	{
		_synchronizationContext.Post(e => ClientConnectedEvent.SafeInvoke(this, (ClientConnectedEventArgs)e), eventArgs);
	}

	/// <summary>
	/// Fires ClientDisconnectedEvent in the current thread
	/// </summary>
	/// <param name="eventArgs">Client disconnected event Args</param>
	private void OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
	{
		_synchronizationContext.Post(e => ClientDisconnectedEvent.SafeInvoke(this, (ClientDisconnectedEventArgs)e), eventArgs);
	}

	/// <summary>
	/// Handles a client connection. Fires the relevant event and prepares for new connection.
	/// </summary>
	/// <param name="sender">Event sender</param>
	/// <param name="eventArgs">Client connected event Args</param>
	private void ClientConnectedHandler(object sender, ClientConnectedEventArgs eventArgs)
	{
		OnClientConnected(eventArgs);

		// Create a additional server as a preparation for new connection
		StartNamedPipeServer();
	}

	/// <summary>
	/// Hanldes a client disconnection. Fires the relevant event ans removes its server from the pool
	/// </summary>
	/// <param name="sender">Event sender</param>
	/// <param name="eventArgs">Client disconnected event Args</param>
	private void ClientDisconnectedHandler(object sender, ClientDisconnectedEventArgs eventArgs)
	{
		OnClientDisconnected(eventArgs);

		StopNamedPipeServer(eventArgs.ClientId);
	}

	/// <summary>
	/// Handles a message that is received from the client. Fires the relevant event.
	/// </summary>
	/// <param name="sender">Event sender</param>
	/// <param name="eventArgs">Message received event Args</param>
	private void MessageReceivedHandler(object sender, MessageReceivedEventArgs eventArgs)
	{
		OnMessageReceived(eventArgs);
	}

	#endregion
}


/*
public static class PipeServer
{
	public static void WaitForConnection()
	{
		WaitForConnectionInitializer();
	}

	static void WaitForConnectionInitializer()
	{
		var context = new ServerContext();
		var server = context.Server;

		try {
			Core.Logging.Log.Message("Waiting for a client ...");
			server.BeginWaitForConnection(WaitForConnectionCallback, context);
		}
		catch {
			// We need to cleanup here only when something goes wrong.
			context.Dispose();
			throw;
		}
	}

	static void WaitForConnectionCallback(IAsyncResult result)
	{
		var (context, server, _) = ServerContext.FromResult(result);
		server.EndWaitForConnection(result);
		BeginRead(context);
	}

	static void BeginRead(ServerContext context)
	{
		var (_, server, requestBuffer) = context;
		server.BeginRead(requestBuffer, 0, requestBuffer.Length, ReadCallback, context);
	}

	static void BeginWrite(ServerContext context)
	{
		var (_, server, responseBuffer) = context;
		server.BeginWrite(responseBuffer, 0, responseBuffer.Length, WriteCallback, context);
	}

	static void ReadCallback(IAsyncResult result)
	{
		var (context, server, requestBuffer) = ServerContext.FromResult(result);
		var bytesRead = server.EndRead(result);

		if (bytesRead > 0) {
			if (!server.IsMessageComplete) {
				BeginRead(context);
			}
			else {
				var index = BitConverter.ToInt32(requestBuffer, 0);
				Console.WriteLine($"{index} Request.");
				BeginWrite(context);
			}
		}
	}

	static void WriteCallback(IAsyncResult result)
	{
		var (context, server, responseBuffer) = ServerContext.FromResult(result);
		var index = -1;

		try {
			server.EndWrite(result);
			server.WaitForPipeDrain();

			index = BitConverter.ToInt32(responseBuffer, 0);
			Console.WriteLine($"{index} Pong.");
		}
		finally {
			context.Dispose();
			Console.WriteLine($"{index} Disposed.");
		}
	}
}
*/