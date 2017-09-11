using System;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// 数据管理
/// </summary>
public class DataMgr
{
    // MySQL 连接适配器
    MySqlConnection sqlConn;

    // 单例模式
    public static DataMgr instance;

    /// <summary>
    /// 构造器（单例）
    /// </summary>
    public DataMgr()
    {
        instance = this;

        Connect();
    }

    /// <summary>
    /// 连接 MySQL 数据库
    /// </summary>
    public void Connect()
    {
        // TODO: MySQL 数据库连接信息
        string connStr = "Database=game;Data Source=127.0.0.1;";
        connStr += "User Id=root;Password=123zxc;port=3306";
        sqlConn = new MySqlConnection(connStr);

        try
        {
            // 打开连接
            sqlConn.Open();
        }
        catch (Exception e)
        {
            Console.Write("[DataMgr]Connect " + e.Message);
            return;
        }
    }

    /// <summary>
    /// 判断字符串内容是否安全，防止 SQL 注入
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public bool IsSafeStr(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    /// <summary>
    /// 用户 ID 是否已存在
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool CanRegister(string id)
    {
        // 防sql注入
        if (!IsSafeStr(id))
        {
            return false;
        }

        // 查询id是否存在
        string cmdStr = string.Format("select * from user where id='{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return !hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pw"></param>
    /// <returns></returns>
    public bool Register(string id, string pw)
    {
        // 防sql注入
        if (!IsSafeStr(id) || !IsSafeStr(pw))
        {
            Console.WriteLine("[DataMgr]Register 使用非法字符");
            return false;
        }

        //能否注册
        if (!CanRegister(id))
        {
            Console.WriteLine("[DataMgr]Register !CanRegister");
            return false;
        }

        // 写入数据库User表
        string cmdStr = string.Format("insert into user set id ='{0}' ,pw ='{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]Register " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CreatePlayer(string id)
    {
        // 防sql注入
        if (!IsSafeStr(id))
        {
            return false;
        }

        // 序列化角色数据 PlayerData
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();

        PlayerData playerData = new PlayerData();
        try
        {
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
            return false;
        }

        byte[] byteArr = stream.ToArray();

        // 写入数据库 player 表
        string cmdStr = string.Format("insert into player set id ='{0}' ,data =@data;", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 检查用户名密码
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pw"></param>
    /// <returns></returns>
    public bool CheckPassWord(string id, string pw)
    {
        // 防sql注入
        if (!IsSafeStr(id) || !IsSafeStr(pw))
        {
            return false;
        }

        //查询
        string cmdStr = string.Format("select * from user where id='{0}' and pw='{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CheckPassWord " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 获取角色数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public PlayerData GetPlayerData(string id)
    {
        PlayerData playerData = null;

        // 防sql注入
        if (!IsSafeStr(id))
        {
            return playerData;
        }

        // 查询
        string cmdStr = string.Format("select * from player where id ='{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        byte[] buffer;

        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (!dataReader.HasRows)
            {
                dataReader.Close();
                return playerData;
            }
            dataReader.Read();

            long len = dataReader.GetBytes(1, 0, null, 0, 0); //1是data  
            buffer = new byte[len];
            dataReader.GetBytes(1, 0, buffer, 0, (int) len);
            dataReader.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
            return playerData;
        }

        // 反序列化角色数据 PlayerData
        MemoryStream stream = new MemoryStream(buffer);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            playerData = (PlayerData) formatter.Deserialize(stream);
            return playerData;
        }
        catch (SerializationException e)
        {
            Console.WriteLine("[DataMgr]GetPlayerData 反序列化 " + e.Message);
            return playerData;
        }
    }


    /// <summary>
    /// 保存角色数据 PlayerData
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool SavePlayer(Player player)
    {
        string id = player.id;
        PlayerData playerData = player.data;

        // 序列化角色数据
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        try
        {
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
            return false;
        }

        byte[] byteArr = stream.ToArray();

        // 写入数据库 player 表
        string formatStr = "update player set data =@data where id = '{0}';";
        string cmdStr = string.Format(formatStr, player.id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
            return false;
        }
    }
}