using System.IO.Pipes;
using System.Text;

namespace ImageFunctions.ComfiUINodes;

internal class PipeServer : IPipeServer
{
	public readonly string Id;
	const int BufferSize = 2048;
	readonly NamedPipeServerStream _pipeServer;
	readonly object _lockingObject = new();
	bool _isStopping;

	public PipeServer(string pipeName, int maxNumberOfServerInstances)
	{
		_pipeServer = new NamedPipeServerStream(
			pipeName,
			PipeDirection.InOut,
			maxNumberOfServerInstances,
			PipeTransmissionMode.Byte,
			PipeOptions.Asynchronous
		);
		Id = Guid.NewGuid().ToString();
	}

	class Info
	{
		public readonly byte[] Buffer;
		public readonly MemoryStream Store;

		public Info()
		{
			Buffer = new byte[BufferSize];
			Store = new MemoryStream();
		}
	}

	public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
	public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
	public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

	public string ServerId { get { return Id; }}

	/// <summary>
	/// This method begins an asynchronous operation to wait for a client to connect.
	/// </summary>
	public void Start()
	{
		_pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, null);
	}

	/// <summary>
	/// This method disconnects, closes and disposes the server
	/// </summary>
	public void Stop()
	{
		_isStopping = true;

		try
		{
			if (_pipeServer.IsConnected) {
				_pipeServer.Disconnect();
			}
		}
		finally {
			_pipeServer.Close();
			_pipeServer.Dispose();
		}
	}

	/// <summary>
	/// This method begins an asynchronous read operation.
	/// </summary>
	void BeginRead(Info info)
	{
		_pipeServer.BeginRead(info.Buffer, 0, BufferSize, EndReadCallBack, info);
	}

	/// <summary>
	/// This callback is called when the async WaitForConnection operation is completed,
	/// whether a connection was made or not. WaitForConnection can be completed when the server disconnects.
	/// </summary>
	void WaitForConnectionCallBack(IAsyncResult result)
	{
		if (!_isStopping) {
			lock (_lockingObject) {
				if (!_isStopping) {
					// Call EndWaitForConnection to complete the connection operation
					_pipeServer.EndWaitForConnection(result);
					OnConnected();
					BeginRead(new Info());
				}
			}
		}
	}

	/// <summary>
	/// This callback is called when the BeginRead operation is completed.
	/// We can arrive here whether the connection is valid or not
	/// </summary>
	void EndReadCallBack(IAsyncResult result)
	{
		var readBytes = _pipeServer.EndRead(result);
		if (readBytes > 0) {
			var info = (Info)result.AsyncState;

			// Get the read bytes and append them
			info.Store.Write(info.Buffer,0,readBytes);

			if (false && !_pipeServer.IsMessageComplete) { // Message is not complete, continue reading
				//BeginRead(info);
			}
			else { // Message is completed
				// Finalize the received string and fire MessageReceivedEvent
				var message = info.Store.ToArray();
				OnMessageReceived(message);
				// Begin a new reading operation
				BeginRead(new Info());
			}
		}
		else { // When no bytes were read, it can mean that the client have been disconnected
			if (!_isStopping) {
				lock (_lockingObject) {
					if (!_isStopping) {
						OnDisconnected();
						Stop();
					}
				}
			}
		}
	}

	/// <summary>
	/// This method fires MessageReceivedEvent with the given message
	/// </summary>
	void OnMessageReceived(byte[] message)
	{
		MessageReceivedEvent?.Invoke(this, new MessageReceivedEventArgs { Message = message });
	}

	/// <summary>
	/// This method fires ConnectedEvent 
	/// </summary>
	void OnConnected()
	{
		ClientConnectedEvent?.Invoke(this, new ClientConnectedEventArgs { ClientId = Id });
	}

	/// <summary>
	/// This method fires DisconnectedEvent 
	/// </summary>
	void OnDisconnected()
	{
		ClientDisconnectedEvent?.Invoke(this, new ClientDisconnectedEventArgs { ClientId = Id });
	}


	public bool Write(byte[] data)
	{
		if (!_pipeServer.IsConnected) {
			return false;
		}
		_pipeServer.BeginWrite(data, 0, data.Length, EndWriteCallBack, data);
		return true;
	}

	void EndWriteCallBack(IAsyncResult result)
	{
		_pipeServer.EndWrite(result);
	}
}