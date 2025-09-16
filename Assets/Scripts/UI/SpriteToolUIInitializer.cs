using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToolUIInitializer : MonoBehaviour
{
    [Header("参照コンテキスト")]
    [Tooltip("SpriteToolContextを参照")]
    public SpriteToolContext context;

    [Header("UI項目")]
    [Tooltip("アニメーション選択用ドロップダウン")]
    public TMP_Dropdown clipDropdown;

    [Header("ログ表示")]
    [Tooltip("ログ表示用のLogScroller")]
    public LogScroller logScroller;

    private void Start()
    {
        if (context == null || clipDropdown == null || logScroller == null)
        {
            logScroller?.AddLog("コンテキストまたはUIが未設定です", LogLevel.Warn);
            return;
        }

        InitializeClipDropdown();
        logScroller.AddLog("SpriteToolUIInitializer：正常に起動しました", LogLevel.Info);
    }

    private void InitializeClipDropdown()
    {
        var options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("アニメーションなし")
        };

        foreach (var clip in context.mergedClips)
        {
            if (clip != null)
            {
                options.Add(new TMP_Dropdown.OptionData(clip.name));
            }
        }

        clipDropdown.ClearOptions();
        clipDropdown.AddOptions(options);

        logScroller.AddLog($"AnimationClip一覧をUIに反映：{options.Count}項目", LogLevel.Info);
    }
}
