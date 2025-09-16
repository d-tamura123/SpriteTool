using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToolUIInitializer : MonoBehaviour
{
    [Header("�Q�ƃR���e�L�X�g")]
    [Tooltip("SpriteToolContext���Q��")]
    public SpriteToolContext context;

    [Header("UI����")]
    [Tooltip("�A�j���[�V�����I��p�h���b�v�_�E��")]
    public TMP_Dropdown clipDropdown;

    [Header("���O�\��")]
    [Tooltip("���O�\���p��LogScroller")]
    public LogScroller logScroller;

    private void Start()
    {
        if (context == null || clipDropdown == null || logScroller == null)
        {
            logScroller?.AddLog("�R���e�L�X�g�܂���UI�����ݒ�ł�", LogLevel.Warn);
            return;
        }

        InitializeClipDropdown();
        logScroller.AddLog("SpriteToolUIInitializer�F����ɋN�����܂���", LogLevel.Info);
    }

    private void InitializeClipDropdown()
    {
        var options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("�A�j���[�V�����Ȃ�")
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

        logScroller.AddLog($"AnimationClip�ꗗ��UI�ɔ��f�F{options.Count}����", LogLevel.Info);
    }
}
