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
            // 房间管理
            RoomMgr roomMgr = new RoomMgr();
            // 数据管理
            DataMgr dataMgr = new DataMgr();
            // 网络管理
            ServNet servNet = new ServNet();

            // 设置服务端默认协议为字节流协议
            servNet.proto = new ProtocolBytes();
            // 启动服务器
            servNet.Start(IP, PORT);

            while (true)
            {
                // 接收并执行用户输入到命令行窗口中的命令
                string command = Console.ReadLine();
                switch (command)
                {
                    case "quit":
                        servNet.Close();
                        return;
                    case "print":
                        servNet.Print();
                        break;
                    default:
                        servNet.Print();
                        break;
                }
            }
        }
    }
}