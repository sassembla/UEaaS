using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using wsControl;

public class Player : MonoBehaviour {
	const string userId = "100";

	void Awake () {
		WebSocketConnectionController.Init(
			userId,

			// connected
			() => {
				Debug.Log("connect succeeded!");
				Application.logMessageReceived += HandleLog;
				WebSocketConnectionController.SendCommand(new Commands.SetId(userId).ToString());
			},

			// messages comes from server
			(List<string> datas) => {
				Debug.Log("datas received:" + datas.Count);
				foreach (var data in datas) {
					Debug.Log("data:" + data);
				}
			},

			// connect fail
			(string connectionFailReason) => {
				Debug.Log("failed to connect! by reason:" + connectionFailReason);
			},

			// error on connection
			(string connectionError) => {
				Debug.Log("failed in running! by reason:" + connectionError);
			},

			// auto reconnect
			true,
			() => {
				Debug.Log("reconnecting!");
			}
		);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Debug.Log("update");
	}

	public void OnApplicationQuit () {
		Debug.Log("exitting..");
		WebSocketConnectionController.CloseCurrentConnection();
	}
	

	/**
		send log to server.
	*/
	void HandleLog(string logString, string stackTrace, LogType type) {
		WebSocketConnectionController.SendCommand(new Commands.Log(userId, logString).ToString());
	}
}
