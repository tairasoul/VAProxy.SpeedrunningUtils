using System;
using System.Net.Sockets;
using System.Text;

namespace SpeedrunningUtils
{
    public class Livesplit
    {
        private static TcpClient _client;
        private static readonly int ServerPort = 16834;

        public static void StartSocket()
        {
            try
            {
                Plugin.Log.LogInfo("Attempting to connect to server...");
                Connect();
                Plugin.Log.LogInfo("Connected. Have fun!");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Error connecting: {ex.Message}");
            }
        }

        private static void Connect()
        {
            _client = new TcpClient();
            _client.Connect("localhost", ServerPort);
        }

        public static string SendCommand(string command, bool read = false)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    Plugin.Log.LogInfo("Socket is not connected.");
                    return null;
                }

                byte[] data = Encoding.UTF8.GetBytes(command + "\r\n");
                _client.GetStream().Write(data, 0, data.Length);

                Plugin.Log.LogInfo($"Sending command {command}.");

                if (!read) return "";

                return ReadResponse();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Error sending command: {ex.Message}");
                return null;
            }
        }

        private static string ReadResponse()
        {
            try
            {
                var stream = _client.GetStream();
                var responseBuilder = new StringBuilder();
                var buffer = new byte[4096];
                int bytesRead;

                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                } while (stream.DataAvailable);

                return responseBuilder.ToString();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Error reading response: {ex.Message}");
                return null;
            }
        }

        public static void Close()
        {
            _client?.Close();
            Plugin.Log.LogInfo("Socket connection closed.");
        }
    }
}
