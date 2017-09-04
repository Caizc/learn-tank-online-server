using System;
using System.Linq;

/// <summary>
/// 字节流协议模型
/// </summary>
public class ProtocolBytes : ProtocolBase
{
    // 传输的字节流
    public byte[] bytes;

    /// <summary>
    /// 解码器
    /// </summary>
    /// <param name="readbuff"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.bytes = new byte[length];
        Array.Copy(readbuff, start, protocol.bytes, 0, length);

        return protocol;
    }

    /// <summary>
    /// 编码器
    /// </summary>
    /// <returns></returns>
    public override byte[] Encode()
    {
        return bytes;
    }

    /// <summary>
    /// 协议名称
    /// </summary>
    /// <returns></returns>
    public override string GetName()
    {
        return GetString(0);
    }

    /// <summary>
    /// 协议描述
    /// </summary>
    /// <returns></returns>
    public override string GetDesc()
    {
        string str = "";
        if (bytes == null)
        {
            return str;
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            int b = (int)bytes[i];
            str += b.ToString() + " ";
        }

        return str;
    }


    /// <summary>
    /// 添加字符串
    /// </summary>
    /// <param name="str"></param>
    public void AddString(string str)
    {
        Int32 len = str.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);

        if (bytes == null)
        {
            bytes = lenBytes.Concat(strBytes).ToArray();
        }
        else
        {
            bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取字符串
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public string GetString(int start, ref int end)
    {
        if (bytes == null)
        {
            return "";
        }

        if (bytes.Length < start + sizeof(Int32))
        {
            return "";
        }

        Int32 strLen = BitConverter.ToInt32(bytes, start);
        if (bytes.Length < start + sizeof(Int32) + strLen)
        {
            return "";
        }

        string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strLen);
        end = start + sizeof(Int32) + strLen;

        return str;
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取字符串
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public string GetString(int start)
    {
        int end = 0;

        return GetString(start, ref end);
    }

    /// <summary>
    /// 添加 int 整数
    /// </summary>
    /// <param name="num"></param>
    public void AddInt(int num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);

        if (bytes == null)
        {
            bytes = numBytes;
        }
        else
        {
            bytes = bytes.Concat(numBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取 int 整数
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public int GetInt(int start, ref int end)
    {
        if (bytes == null)
        {
            return 0;
        }

        if (bytes.Length < start + sizeof(Int32))
        {
            return 0;
        }

        end = start + sizeof(Int32);

        return BitConverter.ToInt32(bytes, start);
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取 int 整数
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public int GetInt(int start)
    {
        int end = 0;

        return GetInt(start, ref end);
    }

    /// <summary>
    /// 添加 float 浮点数
    /// </summary>
    /// <param name="num"></param>
    public void AddFloat(float num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);

        if (bytes == null)
        {
            bytes = numBytes;
        }
        else
        {
            bytes = bytes.Concat(numBytes).ToArray();
        }
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取 float 浮点数
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public float GetFloat(int start, ref int end)
    {
        if (bytes == null)
        {
            return 0;
        }

        if (bytes.Length < start + sizeof(float))
        {
            return 0;
        }

        end = start + sizeof(float);

        return BitConverter.ToSingle(bytes, start);
    }

    /// <summary>
    /// 从字节数组的 start 处开始读取 float 浮点数
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public float GetFloat(int start)
    {
        int end = 0;

        return GetFloat(start, ref end);
    }
}