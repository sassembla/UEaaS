using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using ClientLSWebSocket;
using UniRx;


/**
	connect to WebSocket server and get push from the server.
	all received datas will appear in main thread.
*/
namespace wsControl {
	public class WebSocketConnectionController {
		public static WebSocket webSocket;

		public static string WEBSOCKET_ENTRYPOINT = "ws://127.0.0.1:80/client";
		private static int RECONNECTION_MILLISEC = 1000;

		public static List<string> messageQueue = new List<string>();

		public static void Init (
			string userId, 
			Action connected, 
			Action<List<string>> message, 
			Action<string> connectionFailed, 
			Action<string> disconnected,
			bool autoReconnect,
			Action reconnected
		) {
			Observable.EveryUpdate().Subscribe(
				_ => {
					if (messageQueue.Any()) {
						message(new List<string>(messageQueue));
						messageQueue.Clear();
					}
				}
			);

			webSocket = new WebSocket(WEBSOCKET_ENTRYPOINT);

			webSocket.OnOpen += (sender, e) => {
				MainThreadDispatcher.Post(
					() => {
						connected();
					}
				);
			};


			webSocket.OnMessage += (sender, e) => {
				messageQueue.Add(e.Data);
			};


			webSocket.OnError += (sender, e) => {
				MainThreadDispatcher.Post(
					() => {
						disconnected(e.Message);
					}
				);
				
				CloseCurrentConnection();
				
				if (autoReconnect) {
					// auto reconnect after RECONNECTION_MILLISEC.
					Observable.TimerFrame(RECONNECTION_MILLISEC).Subscribe(
						_ => {
							webSocket.Connect();
							if (!webSocket.IsAlive) {
								connectionFailed("could not re-connect to entry point:" + WEBSOCKET_ENTRYPOINT);
							}
						}
					);
				}
			};

			webSocket.OnClose += (sender, e) => {

			};
			
			webSocket.Connect();
			if (!webSocket.IsAlive) {
				connectionFailed("could not connect to entry point:" + WEBSOCKET_ENTRYPOINT);
			}
		}

		public static void SendCommand (string command) {
			if (webSocket.IsAlive) webSocket.Send(command);
		}

		public static void CloseCurrentConnection () {
			if (webSocket.IsAlive) webSocket.Close();
		}
	}
}