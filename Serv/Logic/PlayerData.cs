using System;

/// <summary>
/// ��ҽ�ɫ����
/// </summary>
[Serializable]
public class PlayerData
{
    // �÷�
    public int score = 0;

    // ʤ����
    public int win = 0;

    // ʧ����
    public int fail = 0;

    /// <summary>
    /// ���캯��
    /// </summary>
    public PlayerData()
    {
        score = 100;
    }
}