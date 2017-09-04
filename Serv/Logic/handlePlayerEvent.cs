using System;

/// <summary>
/// 处理玩家事件
/// </summary>
public class HandlePlayerEvent
{
    /// <summary>
    /// 上线
    /// </summary>
    /// <param name="player"></param>
    public void OnLogin(Player player)
    {
    }

    /// <summary>
    /// 下线
    /// </summary>
    /// <param name="player"></param>
    public void OnLogout(Player player)
    {
        // 房间中
        if (player.tempData.status == PlayerTempData.Status.Room)
        {
            Room room = player.tempData.room;
            RoomMgr.instance.LeaveRoom(player);
            if (room != null)
            {
                room.Broadcast(room.GetRoomInfo());
            }
        }

        // 战斗中
        if (player.tempData.status == PlayerTempData.Status.Fight)
        {
            Room room = player.tempData.room;
            room.ExitFight(player);
            RoomMgr.instance.LeaveRoom(player);
        }
    }
}