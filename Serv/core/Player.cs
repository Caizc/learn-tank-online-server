using System;

/// <summary>
/// 玩家角色
/// </summary>
public class Player
{
    // 玩家 id
    public string id;

    // 相应的客户端连接
    public Conn conn;

    // 角色数据
    public PlayerData data;

    // 角色临时数据
    public PlayerTempData tempData;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conn"></param>
    public Player(string id, Conn conn)
    {
        this.id = id;
        this.conn = conn;
        tempData = new PlayerTempData();
    }

    /// <summary>
    /// 向该玩家角色发送协议消息
    /// </summary>
    /// <param name="proto"></param>
    public void Send(ProtocolBase proto)
    {
        if (conn == null)
        {
            return;
        }

        ServNet.instance.Send(conn, proto);
    }

    /// <summary>
    /// 将该玩家踢下线
    /// </summary>
    /// <param name="id"></param>
    /// <param name="proto"></param>
    /// <returns></returns>
    public static bool KickOff(string id, ProtocolBase proto)
    {
        Conn[] conns = ServNet.instance.conns;
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null || !conns[i].isUse || conns[i].player == null)
            {
                continue;
            }

            if (conns[i].player.id == id)
            {
                lock (conns[i].player)
                {
                    if (proto != null)
                    {
                        // 向玩家发送登出协议消息
                        conns[i].player.Send(proto);
                    }

                    return conns[i].player.Logout();
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 玩家登出
    /// </summary>
    /// <returns></returns>
    public bool Logout()
    {
        // 登出
        ServNet.instance.handlePlayerEvent.OnLogout(this);

        // 保存角色数据
        if (!DataMgr.instance.SavePlayer(this))
        {
            return false;
        }

        // 下线
        conn.player = null;
        conn.Close();
        return true;
    }
}