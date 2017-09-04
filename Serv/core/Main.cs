using System;

namespace Serv
{
    class MainClass
    {
        // TODO: Socket Server IP&Port
        private static string IP = "192.168.1.111";

        private static int PORT = 1234;

        /// <summary>
        /// ����˳������
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // �������
            RoomMgr roomMgr = new RoomMgr();
            // ���ݹ���
            DataMgr dataMgr = new DataMgr();
            // �������
            ServNet servNet = new ServNet();

            // ���÷����Ĭ��Э��Ϊ�ֽ���Э��
            servNet.proto = new ProtocolBytes();
            // ����������
            servNet.Start(IP, PORT);

            while (true)
            {
                // ���ղ�ִ���û����뵽�����д����е�����
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