using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 房间管理
/// </summary>
public class RoomMgr
{
    // 单例
    public static RoomMgr instance;

    /// <summary>
    /// 构造器（单例）
    /// </summary>
    public RoomMgr()
    {
        instance = this;
    }

    // 房间列表
    public List<Room> list = new List<Room>();

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="player"></param>
    public void CreateRoom(Player player)
    {
        Room room = new Room();

        lock (list)
        {
            list.Add(room);
            room.AddPlayer(player);
        }
    }

    /// <summary>
    /// 玩家离开
    /// </summary>
    /// <param name="player"></param>
    public void LeaveRoom(Player player)
    {
        PlayerTempData tempDate = player.tempData;
        if (tempDate.status == PlayerTempData.Status.None)
        {
            return;
        }

        Room room = tempDate.room;

        lock (list)
        {
            room.DelPlayer(player.id);
            if (room.list.Count == 0)
            {
                list.Remove(room);
            }
        }
    }

    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <returns></returns>
    public ProtocolBytes GetRoomList()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");

        int count = list.Count;
        //房间数量
        protocol.AddInt(count);

        //每个房间信息
        for (int i = 0; i < count; i++)
        {
            Room room = list[i];
            protocol.AddInt(room.list.Count);
            protocol.AddInt((int) room.status);
        }

        return protocol;
    }
}