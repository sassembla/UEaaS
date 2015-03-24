#UEaaS


##Unity Editor as a Server context

[PROJECT_FOLDER/Assets/ServerContext/Editor/ServerContext.cs](https://github.com/sassembla/UEaaS/blob/master/Assets/ServerContext/Editor/ServerContext.cs#L7) is context of gaming server which is running in the Unity Editor.


##dependencies

[UniRx](https://github.com/neuecc/UniRx)  
[MiniJson](https://gist.github.com/darktable/1411710)  
[Redis 2.8.19 or later](http://redis.io)  

##start
1. add UniRx and MiniJSON from somewhere to PROJECT_FOLDER/Assets/Libs/.
1. cd PROJECT_FOLDER/Server
1. sh run.sh
1. Open Main.unity scene
1. Play game. 

you can see round-tripped JSON data in console and PROJECT_FOLDER/server.log.

##stop
1. cd PROJECT_FOLDER/Server
1. sh stop.sh

##settings
redis host & port is hardcoded in below.
[PROJECT_FOLDER/Assets/ServerContext/Editor/RedisConnectionController.cs](https://github.com/sassembla/UEaaS/blob/master/Assets/ServerContext/Editor/RedisConnectionController.cs#L19)

and

[PROJECT_FOLDER/Server/bin/lua/client.lua](https://github.com/sassembla/UEaaS/blob/master/Server/bin/lua/client.lua#L11)

##!Mac/Win only!(since Unity's limitation)
yes. and run on Windows is not easy. good luck.