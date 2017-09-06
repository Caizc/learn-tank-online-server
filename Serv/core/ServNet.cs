using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Reflection;

/// <summary>
/// 网络管理
/// </summary>
public class ServNet
{
    // 监听 Socket
    public Socket listenfd;

    // 客户端链接数组
    public Conn[] conns;

    // 最大链接数
    public int maxConn = 50;

    // ServNet 单例
    public static ServNet instance;

    // 主定时器
    System.Timers.Timer timer = new System.Timers.Timer(1000);

    // 心跳时间间隔
    public long heartBeatTime = 180;

    // 协议基类
    public ProtocolBase proto;

    #region 消息分发及处理

    // 处理连接协议
    public HandleConnMsg handleConnMsg = new HandleConnMsg();

    // 处理玩家协议
    public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();

    // 处理玩家事件
    public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

    #endregion

    /// <summary>
    /// 构造器（单例）
    /// </summary>
    public ServNet()
    {
        instance = this;
    }

    /// <summary>
    /// 获取链接池索引，返回负数表示获取失败
    /// </summary>
    /// <returns></returns>
    public int NewIndex()
    {
        if (conns == null)
        {
            return -1;
        }

        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 启动服务端程序
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void Start(string host, int port)
    {
        // 定时器
        timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
        timer.AutoReset = false;
        timer.Enabled = true;

        // 初始化链接池
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++)
        {
            conns[i] = new Conn();
        }

        // Socket
        listenfd = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        // Bind
        IPAddress ipAdr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenfd.Bind(ipEp);
        // Listen
        listenfd.Listen(maxConn);
        // Accept
        listenfd.BeginAccept(AcceptCb, null);

        Console.WriteLine("[服务器]启动成功");
    }


    /// <summary>
    /// Socket 异步 Accept 回调
    /// </summary>
    /// <param name="ar"></param>
    private void AcceptCb(IAsyncResult ar)
    {
        try
        {
            Socket socket = listenfd.EndAccept(ar);

            int index = NewIndex();

            if (index < 0)
            {
                socket.Close();
                Console.Write("[警告]链接已满");
            }
            else
            {
                Conn conn = conns[index];
                conn.Init(socket);

                string adr = conn.GetAdress();
                Console.WriteLine("客户端连接 [" + adr + "] conn池ID：" + index);

                // 开始异步接收数据
                conn.socket.BeginReceive(conn.readBuff,
                    conn.buffCount, conn.BuffRemain(),
                    SocketFlags.None, ReceiveCb, conn);
            }

            // 继续异步 Accept 新的连接，实现循环处理连接请求
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch (Exception e)
        {
            Console.WriteLine("AcceptCb失败:" + e.Message);
        }
    }

    /// <summary>
    /// 关闭所有 Socket 连接
    /// </summary>
    public void Close()
    {
        foreach (Conn conn in conns)
        {
            if (conn == null)
            {
                continue;
            }

            if (!conn.isUse)
            {
                continue;
            }

            lock (conn)
            {
                conn.Close();
            }
        }
    }

    /// <summary>
    /// Socket 异步 Receive 回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCb(IAsyncResult ar)
    {
        Conn conn = (Conn) ar.AsyncState;

        lock (conn)
        {
            try
            {
                // 获取接收到的字节数
                int count = conn.socket.EndReceive(ar);

                // 关闭信号
                if (count <= 0)
                {
                    Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开链接");
                    conn.Close();
                    return;
                }

                // 增加接收缓冲队列的大小
                conn.buffCount += count;

                // 粘包分包处理
                ProcessData(conn);

                // 继续异步接收新的数据，实现循环处理字节消息
                conn.socket.BeginReceive(conn.readBuff,
                    conn.buffCount, conn.BuffRemain(),
                    SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception e)
            {
                Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开链接 " + e.Message);
                conn.Close();
            }
        }
    }

    /// <summary>
    /// 粘包分包处理
    /// </summary>
    /// <param name="conn"></param>
    private void ProcessData(Conn conn)
    {
        // 小于表示消息长度的 4 个字节，表明还未有足够多的数据可以解析
        if (conn.buffCount < sizeof(Int32))
        {
            return;
        }

        // 消息长度
        Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
        conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
        if (conn.buffCount < conn.msgLength + sizeof(Int32))
        {
            return;
        }

        // 处理消息，解析协议
        ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
        HandleMsg(conn, protocol);

        // 清除已处理的消息
        int count = conn.buffCount - conn.msgLength - sizeof(Int32);
        Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
        conn.buffCount = count;
        if (conn.buffCount > 0)
        {
            ProcessData(conn);
        }
    }

    /// <summary>
    /// 将协议消息分发给各处理器
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="protoBase"></param>
    private void HandleMsg(Conn conn, ProtocolBase protoBase)
    {
        string name = protoBase.GetName();
        string methodName = "Msg" + name;

        // 连接协议分发
        if (conn.player == null || name == "HeatBeat" || name == "Logout")
        {
            // 反射
            MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
            if (mm == null)
            {
                string str = "[警告]HandleMsg没有处理连接方法 ";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[] {conn, protoBase};
            Console.WriteLine("[处理链接消息]" + conn.GetAdress() + " :" + name);
            mm.Invoke(handleConnMsg, obj);
        }
        // 角色协议分发
        else
        {
            // 反射
            MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
            if (mm == null)
            {
                string str = "[警告]HandleMsg没有处理玩家方法 ";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[] {conn.player, protoBase};
            Console.WriteLine("[处理玩家消息]" + conn.player.id + " :" + name);
            mm.Invoke(handlePlayerMsg, obj);
        }
    }

    /// <summary>
    /// 向客户端发送消息
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="protocol"></param>
    public void Send(Conn conn, ProtocolBase protocol)
    {
        // 编码协议消息
        byte[] bytes = protocol.Encode();
        // 消息长度
        byte[] length = BitConverter.GetBytes(bytes.Length);
        // 待发送的字节数组
        byte[] sendbuff = length.Concat(bytes).ToArray();

        try
        {
            // 异步发送消息
            conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
        }
        catch (Exception e)
        {
            Console.WriteLine("[发送消息]" + conn.GetAdress() + " : " + e.Message);
        }
    }

    /// <summary>
    /// 向所有客户端广播消息
    /// </summary>
    /// <param name="protocol"></param>
    public void Broadcast(ProtocolBase protocol)
    {
        for (int i = 0; i < conns.Length; i++)
        {
            if (!conns[i].isUse)
            {
                continue;
            }

            if (conns[i].player == null)
            {
                continue;
            }

            Send(conns[i], protocol);
        }
    }

    /// <summary>
    /// 主定时器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        // 处理心跳
        HeartBeat();

        timer.Start();
    }

    /// <summary>
    /// 定时检查每个 Socket 连接的心跳时间
    /// </summary>
    private void HeartBeat()
    {
        // Console.WriteLine ("[主定时器执行]");

        long timeNow = Util.GetTimeStamp();

        foreach (Conn conn in conns)
        {
            if (conn == null)
            {
                continue;
            }

            if (!conn.isUse)
            {
                continue;
            }

            // 关闭心跳时间超时的连接
            if (conn.lastTickTime < timeNow - heartBeatTime)
            {
                Console.WriteLine("[心跳引起断开连接]" + conn.GetAdress());
                lock (conn)
                {
                    conn.Close();
                }
            }
        }
    }

    /// <summary>
    /// 打印服务端概要信息
    /// </summary>
    public void Print()
    {
        Console.WriteLine("===服务器登录信息===");

        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
            {
                continue;
            }

            if (!conns[i].isUse)
            {
                continue;
            }

            string str = "连接[" + conns[i].GetAdress() + "] ";
            if (conns[i].player != null)
            {
                str += "玩家id " + conns[i].player.id;
            }

            Console.WriteLine(str);
        }
    }
}