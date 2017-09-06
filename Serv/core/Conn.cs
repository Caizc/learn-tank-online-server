using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;

/// <summary>
/// 客户端连接抽象类
/// </summary>
public class Conn
{
    // 常量，数据缓冲区大小
    public const int BUFFER_SIZE = 1024;

    // Socket
    public Socket socket;

    // 是否已使用
    public bool isUse = false;

    // 数据缓冲字节数组
    public byte[] readBuff = new byte[BUFFER_SIZE];

    // 当前缓冲的字节数
    public int buffCount = 0;

    // 表示消息体长度的字节数组（4 个字节）
    public byte[] lenBytes = new byte[sizeof(UInt32)];

    // 消息长度
    public Int32 msgLength = 0;

    // 最后更新的心跳时间
    public long lastTickTime = long.MinValue;

    // 对应的 Player
    public Player player;

    /// <summary>
    /// 构造器
    /// </summary>
    public Conn()
    {
        readBuff = new byte[BUFFER_SIZE];
    }

    /// <summary>
    /// 初始化连接
    /// </summary>
    /// <param name="socket"></param>
    public void Init(Socket socket)
    {
        this.socket = socket;
        isUse = true;
        buffCount = 0;

        // 心跳处理
        lastTickTime = Util.GetTimeStamp();
    }

    /// <summary>
    /// 剩余的缓冲区大小
    /// </summary>
    /// <returns></returns>
    public int BuffRemain()
    {
        return BUFFER_SIZE - buffCount;
    }

    /// <summary>
    /// 获取客户端地址
    /// </summary>
    /// <returns></returns>
    public string GetAdress()
    {
        if (!isUse)
        {
            return "无法获取地址";
        }

        return socket.RemoteEndPoint.ToString();
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public void Close()
    {
        if (!isUse)
        {
            return;
        }

        if (player != null)
        {
            // 玩家登出
            player.Logout();

            return;
        }

        Console.WriteLine("[断开链接]" + GetAdress());
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        isUse = false;
    }

    /// <summary>
    /// 向客户端发送协议消息
    /// </summary>
    /// <param name="protocol"></param>
    public void Send(ProtocolBase protocol)
    {
        ServNet.instance.Send(this, protocol);
    }
}