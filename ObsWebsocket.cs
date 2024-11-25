using WatsonWebsocket;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace SpeedrunningUtils.OBS;

public struct Message 
{
	public int op;
	public object? d;
	[JsonIgnore]
	public readonly string json 
	{
		get => JsonConvert.SerializeObject(this);
	}
}

public struct Hello 
{
	public string obsWebSocketVersion;
	public int rpcVersion;
	public Auth? authentication;
}

public struct Auth 
{
	public string challenge;
	public string salt;
}

public struct Identify 
{
	public int rpcVersion;
}

public struct IdentifyWithAuth
{
	public int rpcVersion;
	public string authentication;
}

public struct Request 
{
	public string requestType;
	public string requestId;
	public object? requestData;
}

public struct RequestStatus 
{
	public bool result;
	public int code;
	public string? comment;
}

public struct Response 
{
	public string requestType;
	public string requestId;
	public RequestStatus requestStatus;
	public object? responseData;
}

public struct StopRecordingResult 
{
	public string outputPath;
}

public static class ObsWebsocket 
{
	static WatsonWsClient clientSocket;
	internal static bool Identified = false;
	private static bool FirstStart = true;
	internal static bool SocketIsActive = false;
	internal static event Action<string> RecordingStopped;

	public static void StartSocket()
	{
		SocketIsActive = true;
		clientSocket = new(Plugin.WebsocketURL.Value, Plugin.WebsocketPort.Value, false);
		clientSocket.MessageReceived += MessageReceived;
		clientSocket.ServerConnected += ServerConnected;
		clientSocket.ServerDisconnected += ServerDisconnected;
		if (FirstStart) 
			Task.Run(FirstStartConnect);
		FirstStart = false;
	}

	private static void ServerConnected(object sender, EventArgs args) 
	{
		Plugin.WebsocketConnected = true;
	}

	private static void ServerDisconnected(object sender, EventArgs args) 
	{
		Plugin.WebsocketConnected = false;
	}

	public static void Connect()
	{
		if (clientSocket == null)
			StartSocket();
		Task.Run(ConnectAsync);
	}
	
	private static async Task FirstStartConnect() 
	{
		await ConnectAsync();
	}
	
	private static async Task ConnectAsync() 
	{
		try
		{
			Plugin.Log.LogInfo("Attempting to connect to OBS websocket.");
			bool result = await clientSocket.StartWithTimeoutAsync(30);
			if (result)
				Plugin.Log.LogInfo("OBS websocket connected.");
			else
				Plugin.Log.LogInfo("Failed to connect to OBS websocket.");
		}
		catch (Exception ex)
		{
			Plugin.Log.LogInfo($"Error connecting: {ex.Message}");
		}
	}
	
	public static void StartRecording() 
	{
		if (!SocketIsActive)
			return;
		Request request = new()
		{
			requestType = "StartRecord",
			requestId = Guid.NewGuid().ToString()
		};
		Message message = new() 
		{
			op = 6,
			d = request
		};
		string json = message.json;
		clientSocket.SendAsync(json);
	}
	
	public static void StopRecording() 
	{
		if (!SocketIsActive)
			return;
		Request request = new()
		{
			requestType = "StopRecord",
			requestId = Guid.NewGuid().ToString()
		};
		Message message = new() 
		{
			op = 6,
			d = request
		};
		string json = message.json;
		clientSocket.SendAsync(json);
	}
	
	static void MessageReceived(object sender, MessageReceivedEventArgs args) 
	{
		string json = Encoding.UTF8.GetString([.. args.Data]);
		Message msg = JsonConvert.DeserializeObject<Message>(json);
		if (msg.op == 0) {
			Plugin.Log.LogInfo("Received handshake message.");
			JObject dataObj = (JObject)msg.d;
			Hello hello = dataObj.ToObject<Hello>();
			if (hello.authentication.HasValue) 
			{
				Plugin.Log.LogInfo("Handshake requires authentication.");
				string auth = Auth(hello.authentication.Value.challenge, hello.authentication.Value.salt);
				sendIdent(auth);
				return;
			}
			sendIdent();
			return;
		}
		if (msg.op == 2) 
		{
			Identified = true;
			return;
		}
		if (msg.op == 7) 
		{
			Plugin.Log.LogInfo("Received request response.");
			JObject dataObj = (JObject)msg.d;
			Response response = dataObj.ToObject<Response>();
			if (response.requestType == "StopRecord") 
			{
				Plugin.Log.LogInfo("Received response for StopRecord call.");
				JObject respData = (JObject)response.responseData;
				StopRecordingResult result = respData.ToObject<StopRecordingResult>();
				RecordingStopped.Invoke(result.outputPath);
			}
		}
	}
	
	static void sendIdent(string? auth = null) 
	{
		if (auth == null) 
		{
			Plugin.Log.LogInfo("Sending identification (no auth).");
			Identify identify = new()
			{
				rpcVersion = 1
			};
			Message message = new() 
			{
				op = 1,
				d = identify
			};
			clientSocket.SendAsync(message.json);
		}
		else 
		{
			Plugin.Log.LogInfo("Sending identification (requires auth).");
			IdentifyWithAuth identify = new() 
			{
				rpcVersion = 1,
				authentication = auth
			};
			Message message = new() 
			{
				op = 1,
				d = identify
			};
			Plugin.Log.LogInfo($"Identification json: {message.json}");
			clientSocket.SendAsync(message.json);
		}
	}
	
	static string Auth(string challenge, string salt) 
	{
		string salted = Plugin.WebsocketPassword.Value + salt;
		string secret = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(salted)));
		string secretc = secret + challenge;
		return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(secretc)));
	}

	public static void Close()
	{
		if (clientSocket.Connected)
			clientSocket.Stop();
		Plugin.Log.LogInfo("Socket connection closed.");
	}
}