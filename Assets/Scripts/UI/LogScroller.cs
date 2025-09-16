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
    [Header("ログ表示用のTextMeshPro")]
    [SerializeField] private TMP_Text logText;

    [Header("Scroll ViewのScrollRect")]
    [SerializeField] private ScrollRect scrollRect;

    /// <summary>
    /// ログを追加し、最下段へスクロールする
    /// </summary>
    /// <param name="message">ログ本文</param>
    /// <param name="level">ログレベル（Info, Warn, Error）</param>
    public void AddLog(string message, LogLevel level = LogLevel.Info)
    {
        if (logText == null || scrollRect == null)
        {
            Debug.LogWarning("LogScroller: logText または scrollRect が未設定です。");
            return;
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string paddedLevel = $"[{level.ToString().PadRight(5)}]";
        string formatted = $"{timestamp} {paddedLevel} {message}";

        logText.text += "\n" + formatted;

        // レイアウト更新とスクロール位置調整
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        // UnityのConsoleにも出力
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
