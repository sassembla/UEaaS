using System.Collections.Generic;
using MiniJSON;

/**
	Client-Server shared command definition.
*/
public class Commands {

	public class SetId : JSONString {
		public SetId (string identity) {
			this["command"] = "setId";
			this["playerId"] = identity;
		}
	}

	public class Log : JSONString {
		public Log (string identity, string log) {
			this["command"] = "log";
			this["playerId"] = identity;
			this["log"] = log;
		}
	}



	// base class
	public class JSONString : Dictionary<string, object> {
		public override string ToString () {
			return Json.Serialize(this);
		}
	}

	

}