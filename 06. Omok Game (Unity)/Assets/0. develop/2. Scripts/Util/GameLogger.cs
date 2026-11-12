using System.Diagnostics;

// �ش� ��ũ��Ʈ�� �޼������? ����Ƽ ������ �Ǵ� ���� ����(Development Build)������ ����˴ϴ�?.
// ���� ������ ���忡���� ���Ե��� �ʾ� ���ɿ� ������ ���� �ʽ��ϴ�.
public static class GameLogger
{
    // Conditional ��Ʈ����Ʈ�� ���� �ڼ��� ������ �Ʒ� ���? �������� 5�� �׸��� �������ּ���
    // https://www.notion.so/26cb1f49203e8098a26ae89bf84a9d54?source=copy_link
    
    /// <summary>
    /// Conditional Attribute�� ���� �ڼ��� ������
    /// <see href="https://www.notion.so/26cb1f49203e8098a26ae89bf84a9d54?source=copy_link">5�� �׸�</see>�� �����ϼ���.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    /// <summary>
    /// Conditional Attribute�� ���� �ڼ��� ������
    /// <see href="https://www.notion.so/26cb1f49203e8098a26ae89bf84a9d54?source=copy_link">5�� �׸�</see>�� �����ϼ���.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }
    
    /// <summary>
    /// Conditional Attribute�� ���� �ڼ��� ������
    /// <see href="https://www.notion.so/26cb1f49203e8098a26ae89bf84a9d54?source=copy_link">5�� �׸�</see>�� �����ϼ���.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }
}