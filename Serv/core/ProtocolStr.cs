using System;
using System.Collections;

/// <summary>
/// 字符串协议模型（形式 名称,参数1,参数2,参数3）
/// </summary>
public class ProtocolStr : ProtocolBase
{
    // 传输的字符串
    public string str;

    /// <summary>
    /// 解码器
    /// </summary>
    /// <param name="readbuff"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public override ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        ProtocolStr protocol = new ProtocolStr();
        protocol.str = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
        return (ProtocolBase) protocol;
    }

    /// <summary>
    /// 编码器
    /// </summary>
    /// <returns></returns>
    public override byte[] Encode()
    {
        byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
        return b;
    }

    /// <summary>
    /// 协议名称
    /// </summary>
    /// <returns></returns>
    public override string GetName()
    {
        if (str.Length == 0) return "";
        return str.Split(',')[0];
    }

    /// <summary>
    /// 协议描述
    /// </summary>
    /// <returns></returns>
    public override string GetDesc()
    {
        return str;
    }
}