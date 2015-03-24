using ServerUtil;

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

using UniRx;

using MiniJSON;


public class RedisConnectionController : IDisposable {
	readonly ServerContext context;

	/*
		redis connection settings
	*/
	const int timeout = -1;
	const string host = "127.0.0.1";
	const int port = 6379;

	// redis commands
	const string REDIS_COMMAND_PUBLISH		= "PUBLISH";
	const string REDIS_COMMAND_SUBSCRIBE	= "SUBSCRIBE";
	const string REDIS_COMMAND_PUNSUBSCRIBE	= "PUNSUBSCRIBE";


	// publisher socket
	private Socket pubSocket;
	const string PUBLISH_KEY_CENTRAL = "central";

	// subscriber socket
	private Socket subSocket;
	const string SUBSCLIBE_KEY_CLIENT = "client";
	const int bufferSize = 16*1024;
	private byte [] buf = new byte[bufferSize];


	// webSocket server state for each connection. syncronized to nginx-lua client.lua code.
	const char STATE_CONNECT		= '1';
	const char STATE_MESSAGE		= '2';
	const char STATE_DISCONNECT_1	= '3';
	const char STATE_DISCONNECT_2	= '4';



	public RedisConnectionController (ServerContext context) {
		this.context = context;

		// start publish connection
		pubSocket = GenSocket(host, port, timeout);
		
		// start subscribe connection
		subSocket = GenSocket(host, port, timeout);
		var subStream = new NetworkStream(subSocket);
		
		var subChannelArgs = new string [] {PUBLISH_KEY_CENTRAL};
		SendCommand(subSocket, REDIS_COMMAND_SUBSCRIBE, subChannelArgs);
		Observable.EveryUpdate().Subscribe(_ => Reading(subStream));

		Server.Log("RedisConnectionController: succeded to connect redis @:" + host + ":" + port);
	}

	public void Publish (object messageObj, string[] targetConnectionIds) {
		var connectionIdsStr = "cons" + targetConnectionIds.Length + ":" + string.Join(",", targetConnectionIds);
		var messageStr = connectionIdsStr + Json.Serialize(messageObj);

		var pubChannelArgs = new string [] {SUBSCLIBE_KEY_CLIENT, messageStr};
		SendCommand(pubSocket, REDIS_COMMAND_PUBLISH, pubChannelArgs);
	}

	public void Receive (byte[] dataArray, int len) {
		if (len < 2) return;
		if (dataArray[0] == 's' && dataArray[1] == 't') {
			// OK
		} else {
			return;
		}

		// for (int i = 0; i < len; i++) {
		// 	Server.Log("i:" + i + " c:" + (char)dataArray[i]);
		// }

		var state = (char)dataArray[2];

		// dataArray[3],[4],[5] = "con", length = definitely 36.
		var connectionId = Encoding.ASCII.GetString(dataArray, 6, 36);
		
		switch (state) {
			case STATE_CONNECT: {
				context.OnConnected(connectionId);
				break;
			}

			case STATE_MESSAGE: {
				// the case which data type is string.
				var data = "{}"; // representation of empty json
				if (6 + 36 < len) {
					data = Encoding.UTF8.GetString(dataArray, 6 + 36, len);
				}

				var sourceDict = Json.Deserialize(data) as Dictionary<string, object>;
				context.OnMessage(connectionId, sourceDict);
				break;
			}

			case STATE_DISCONNECT_1: {
				context.OnDisconnected(connectionId, "reason1");
				break;
			}

			case STATE_DISCONNECT_2: {
				context.OnDisconnected(connectionId, "reason2");
				break;
			}

			default: {
				Server.Log("undefined websocket state:" + state);
				break;
			}
		}
		
		
	}


	public void Reading (NetworkStream stream) {
		while (stream.DataAvailable) {
			int i = 0;
			for (int j = 0; j < bufferSize; j++) buf[j] = 0xff;

			// get 1st byte of lines
			var b = stream.ReadByte();

			// check contains end signal or not.
			if (b == -1) return;
			buf[i++] = (byte)b;


			int c;
			while ((c = stream.ReadByte()) != -1) {
				if (c == '\r') continue;
				if (c == '\n') break;
				buf[i++] = (byte)c;
			}

			if (b == '-') {
				var s = Encoding.UTF8.GetString(buf);
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
			Receive(buf, i);
		}
	}

	/**
		適当にsocket作り出して、正しければOK。駄目だったら即エラーで落としたい。
	*/
	public Socket GenSocket (string host, int port, int timeout) {
		var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.NoDelay = true;
		socket.SendTimeout = timeout;

		try {
			socket.Connect(host, port);
		} catch (Exception e) {
			Server.Log("ERROR: RedisConnectionController: failed to connect to redis @:" + host + ":" + port);
			Server.Log("ERROR: reason:" + e);
		}

		if (socket.Connected) return socket;

		// failed, 

		socket.Close();
		socket = null;
		throw new Exception("failed to connect to redis @:" + host + ":" + port);
	}

	bool SendCommand (Socket socket, string cmd, params object [] args) {
		if (socket == null) {
			Server.Log("socket is not exist!");
			return false;
		}

		string resp = "*" + (1 + args.Length).ToString() + "\r\n";
		resp += "$" + cmd.Length + "\r\n" + cmd + "\r\n";
		foreach (object arg in args) {
			string argStr = arg.ToString();
			int argStrLength = Encoding.UTF8.GetByteCount(argStr);
			resp += "$" + argStrLength + "\r\n" + argStr + "\r\n";
		}

		byte [] r = Encoding.UTF8.GetBytes(resp);
		try {
			socket.Send(r);
		} catch (SocketException) {
			// timeout;
			socket.Close();
			socket = null;

			return false;
		}
		return true;
	}
	
	public void Dispose () {
		pubSocket.Close();
		pubSocket = null;
		subSocket.Close();
		subSocket = null;
	}

	public class ResponseException : Exception {
		public ResponseException (string code) : base ("Response error") {
			Code = code;
		}

		public string Code { get; private set; }
	}

}