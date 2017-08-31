using System;

namespace Serv
{
	class MainClass
	{
        // TODO: Socket Server IP&Port
	    private static string ip = "192.168.1.111";
	    private static int port = 1234;

		public static void Main(string[] args)
		{
			RoomMgr roomMgr = new RoomMgr ();
			DataMgr dataMgr = new DataMgr ();
			ServNet servNet = new ServNet();
			servNet.proto = new ProtocolBytes ();
			servNet.Start(ip, port);

			while(true)
			{
				string str = Console.ReadLine();
				switch(str)
				{
				case "quit":
					servNet.Close();
					return;
				case "print":
					servNet.Print();
					break;
				}
			}

		}
	}
}
