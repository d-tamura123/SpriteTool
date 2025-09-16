using System.Collections.Generic;
using UnityEngine;

public class SpriteToolContext : MonoBehaviour
{
    [Header("�L�����N�^�[�ݒ�")]
    [Tooltip("�L���v�`���Ώۂ̃L�����N�^�[�v���n�u")]
    public GameObject characterPrefab;

    [Header("Animator Controller�ݒ�")]
    [Tooltip("Animator Controller�̃��X�g�i�����j")]
    public List<RuntimeAnimatorController> animatorControllers = new();

    [Header("�ǉ�Animation Clip")]
    [Tooltip("�ʂɎw�肷��Animation Clip�i�C�Ӂj")]
    public List<AnimationClip> additionalClips = new();

    [Header("�}�[�W���ʁi���������j")]
    [Tooltip("Animator Controller�ƒǉ�Clip�𖼑O�x�[�X�Ń}�[�W��������")]
    public List<AnimationClip> mergedClips = new();

    private void Awake()
    {
        mergedClips = MergeAnimationClips();
        Debug.Log($"[SpriteToolContext] �}�[�W�����F{mergedClips.Count}��AnimationClip");
    }

    private List<AnimationClip> MergeAnimationClips()
    {
        var clipDict = new Dictionary<string, AnimationClip>();

        // AnimatorController����擾
        foreach (var controller in animatorControllers)
        {
            if (controller == null) continue;
            foreach (var clip in controller.animationClips)
            {
                if (clip != null && !clipDict.ContainsKey(clip.name))
                {
                    clipDict.Add(clip.name, clip);
                }
            }
        }

        // �ǉ�Clip����擾
        foreach (var clip in additionalClips)
        {
            if (clip != null && !clipDict.ContainsKey(clip.name))
            {
                clipDict.Add(clip.name, clip);
            }
        }

        return new List<AnimationClip>(clipDict.Values);
    }
}
