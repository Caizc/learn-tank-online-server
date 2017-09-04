/// <summary>
/// 玩家角色临时数据
/// </summary>
public class PlayerTempData
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public PlayerTempData()
    {
        status = Status.None;
    }

    // 当前状态
    public enum Status
    {
        None,
        Room,
        Fight,
    }

    public Status status;

    // room 状态
    public Room room;

    public int team = 1;
    public bool isOwner = false;

    // 战场相关
    public long lastUpdateTime;

    public float posX;
    public float posY;
    public float posZ;
    public long lastShootTime;
    public float hp = 200;
}