using ServerUtil;

using System;
using System.Collections.Generic;
using UniRx;

public class ServerContext {

	private void OnUpdate () {
		// Server.Log("updating");
	}

	public void OnConnected (string connectionId) {
		Server.Log("connected, connectionId:" + connectionId);
	}

	public void OnMessage (string connectionId, Dictionary<string, object> dataDict) {
		if (!dataDict.ContainsKey("command")) return;

		var command = dataDict["command"] as string;
		
		switch (command) {
			case "setId": {
				SetIdentity(connectionId, dataDict);
				break;
			}
			case "log": {
				Server.Log("pId:" + dataDict["playerId"] + ":" + "\"" + dataDict["log"] + "\"");
				break;
			}
			default: {
				Server.Log("undefined command:" + command);
				break;
			}
		}
	}

	public void OnDisconnected (string connectionId, string reason) {
		Server.Log("disconnected, connectionId:" + connectionId + " reason:" + reason);
	}



	private void SetIdentity (string connectionId, Dictionary<string, object> dataDict) {
		var playerId = (string)dataDict["playerId"];
		Server.Log("playerId:" + playerId + " is connected as connectionId:" + connectionId);

		var welcomeMessageDict = new Dictionary<string, string>(){
			{"message", "you are joinning!"}
		};

		PublishTo(welcomeMessageDict, new string[]{connectionId});
	}




	/**
		publisher methods
	*/
	Action<object, string[]> PublishTo;
	public void Publish (object messageData) {
		PublishTo(messageData, new string[0]);
	}

	public void SetPublisher (Action<object, string[]> publisher) {
		PublishTo = publisher;
	}

	public void Start () {
		Observable.EveryUpdate().Subscribe(_ => OnUpdate());
	}
}