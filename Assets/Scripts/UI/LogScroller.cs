using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public enum LogLevel
{
    Info,
    Warn,
    Error
}
public class LogScroller : MonoBehaviour
{
    [Header("���O�\���p��TextMeshPro")]
    [SerializeField] private TMP_Text logText;

    [Header("Scroll View��ScrollRect")]
    [SerializeField] private ScrollRect scrollRect;

    /// <summary>
    /// ���O��ǉ����A�ŉ��i�փX�N���[������
    /// </summary>
    /// <param name="message">���O�{��</param>
    /// <param name="level">���O���x���iInfo, Warn, Error�j</param>
    public void AddLog(string message, LogLevel level = LogLevel.Info)
    {
        if (logText == null || scrollRect == null)
        {
            Debug.LogWarning("LogScroller: logText �܂��� scrollRect �����ݒ�ł��B");
            return;
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string paddedLevel = $"[{level.ToString().PadRight(5)}]";
        string formatted = $"{timestamp} {paddedLevel} {message}";

        logText.text += "\n" + formatted;

        // ���C�A�E�g�X�V�ƃX�N���[���ʒu����
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        // Unity��Console�ɂ��o��
        switch (level)
        {
            case LogLevel.Warn:
                Debug.LogWarning(formatted);
                break;
            case LogLevel.Error:
                Debug.LogError(formatted);
                break;
            default:
                Debug.Log(formatted);
                break;
        }
    }
}
