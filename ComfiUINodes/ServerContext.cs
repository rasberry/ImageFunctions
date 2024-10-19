/*
using System.IO.Pipes;

namespace ImageFunctions.ComfiUINodes;

sealed class ServerContext : IDisposable
{
	const string PipeName = "IF3b17977d-b150-4e57-8ad9-ad6520f54ba3";

	public ServerContext()
	{
		Buffer = new byte[1024];
		Server = new NamedPipeServerStream(PipeName,
			PipeDirection.InOut,
			NamedPipeServerStream.MaxAllowedServerInstances,
			PipeTransmissionMode.Byte,
			PipeOptions.Asynchronous
		);
	}

	public readonly byte[] Buffer;
	public readonly NamedPipeServerStream Server;

	public static ServerContext FromResult(IAsyncResult result)
	{
		ArgumentNullException.ThrowIfNull(result.AsyncState);
		return (ServerContext)result.AsyncState;
	}

	public void Deconstruct(out ServerContext context, out NamedPipeServerStream server, out byte[] buffer)
	{
		context = this;
		server = Server;
		buffer = Buffer;
	}

	public void Dispose()
	{
		if (Server != null) {
			if (Server.IsConnected) {
				Server.Disconnect();
			}
			Server.Dispose();
		}
	}
}
*/