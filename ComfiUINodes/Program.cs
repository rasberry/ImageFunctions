using ImageFunctions.Core.Samplers;

namespace ImageFunctions.ComfiUINodes;

// https://github.com/alekshura/Compentio.Pipes/tree/main

//TODO switch to http port 41414
//named pipes seems too much of hassle
// cool but windows only tho.. https://andrewlock.net/using-named-pipes-with-aspnetcore-and-httpclient/

internal class Program
{
	const string PipeName = "IF3b17977d-b150-4e57-8ad9-ad6520f54ba3";

	static void Main(string[] args)
	{
		var pipeServer = new PipeServer(PipeName, 1);
		pipeServer.ClientConnectedEvent += OnConnect;
		pipeServer.ClientDisconnectedEvent += OnDisconnect;
		pipeServer.MessageReceivedEvent += OnMessage;

		try {
			pipeServer.Start();
			Core.Logging.Log.Message("Press any key to stop server");
			Console.ReadKey(true); //block
		}
		finally {
			pipeServer.Stop();
		}
	}

	static void OnConnect(object sender, ClientConnectedEventArgs args)
	{
		Core.Logging.Log.Message($"OnConnect {args.ClientId}");
	}

	static void OnDisconnect(object sender, ClientDisconnectedEventArgs args)
	{
		Core.Logging.Log.Message($"OnDisconnect {args.ClientId}");
	}

	static void OnMessage(object sender, MessageReceivedEventArgs args)
	{
		var m = BitConverter.ToString(args.Message);
		Core.Logging.Log.Message($"OnMessage {m}");
		((PipeServer)sender).Write("OK"u8.ToArray());
	}
}
