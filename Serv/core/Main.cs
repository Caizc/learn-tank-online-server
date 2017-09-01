using System;

namespace Serv
{
    class MainClass
    {
        // TODO: Socket Server IP&Port
        private static string IP = "192.168.1.111";

        private static int PORT = 1234;

        /// <summary>
        /// 服务端程序入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            RoomMgr roomMgr = new RoomMgr();
            DataMgr dataMgr = new DataMgr();
            ServNet servNet = new ServNet();
            servNet.proto = new ProtocolBytes();
            servNet.Start(IP, PORT);

            while (true)
            {
                string command = Console.ReadLine();
                switch (command)
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