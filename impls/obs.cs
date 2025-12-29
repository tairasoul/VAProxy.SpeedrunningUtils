using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WatsonWebsocket;

namespace speedrunningutils.impls;

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

class OBS {
	WatsonWsClient client = null!;
	bool identified = false;
	bool socketActive = false;

	public async void Connect() {
		if (client == null) {
			client = new(Plugin.cfg.WebsocketURL.Value, Plugin.cfg.WebsocketPort.Value, false);
			socketActive = true;
		}
		try
		{
			Plugin.Log.LogInfo("Attempting to connect to OBS websocket.");
			bool result = await client.StartWithTimeoutAsync(30);
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
	
	public void StartRecording() 
	{
		if (!socketActive)
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
		client.SendAsync(json);
	}
	
	public void StopRecording() 
	{
		if (!socketActive)
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
		client.SendAsync(json);
	}
	
	void MessageReceived(object sender, MessageReceivedEventArgs args) 
	{
		string json = Encoding.UTF8.GetString([.. args.Data]);
		Message msg = JsonConvert.DeserializeObject<Message>(json);
		if (msg.op == 0) {
			Plugin.Log.LogInfo("Received handshake message.");
			JObject dataObj = (JObject)msg.d!;
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
			identified = true;
			return;
		}
		// if (msg.op == 7) 
		// {
			// Plugin.Log.LogInfo("Received request response.");
			// JObject dataObj = (JObject)msg.d!;
			// Response response = dataObj.ToObject<Response>();
			// if (response.requestType == "StopRecord") 
			// {
				// Plugin.Log.LogInfo("Received response for StopRecord call.");
				// JObject respData = (JObject)response.responseData!;
				// StopRecordingResult result = respData.ToObject<StopRecordingResult>();
			// }
		// }
	}
	
	void sendIdent(string? auth = null) 
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
			client.SendAsync(message.json);
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
			client.SendAsync(message.json);
		}
	}
	
	string Auth(string challenge, string salt) 
	{
		string salted = Plugin.cfg.WebsocketPassword.Value + salt;
		string secret = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(salted)));
		string secretc = secret + challenge;
		return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(secretc)));
	}

	public void Close()
	{
		if (client.Connected)
			client.Stop();
		Plugin.Log.LogInfo("Socket connection closed.");
	}
}