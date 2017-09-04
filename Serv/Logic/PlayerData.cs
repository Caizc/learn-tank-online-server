using System;

/// <summary>
/// 玩家角色数据
/// </summary>
[Serializable]
public class PlayerData
{
    // 得分
    public int score = 0;

    // 胜场数
    public int win = 0;

    // 失败数
    public int fail = 0;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PlayerData()
    {
        score = 100;
    }
}