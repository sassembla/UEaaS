using ServerUtil;

using UnityEngine;
using UnityEditor;

using System;
using System.IO;



[InitializeOnLoad]
public class ServerInitializer {

	static ServerInitializer () {
		Init();
	}

	public static void Init () {
		Server.Setup(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "server.log"));

		var context = new ServerContext();
		var redisConnection = new RedisConnectionController(context);
		context.SetPublisher(redisConnection.Publish);
		context.Start();
	}
}
