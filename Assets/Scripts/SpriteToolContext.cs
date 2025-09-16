using System.Collections.Generic;
using UnityEngine;

public class SpriteToolContext : MonoBehaviour
{
    [Header("キャラクター設定")]
    [Tooltip("キャプチャ対象のキャラクタープレハブ")]
    public GameObject characterPrefab;

    [Header("Animator Controller設定")]
    [Tooltip("Animator Controllerのリスト（複数可）")]
    public List<RuntimeAnimatorController> animatorControllers = new();

    [Header("追加Animation Clip")]
    [Tooltip("個別に指定するAnimation Clip（任意）")]
    public List<AnimationClip> additionalClips = new();

    [Header("マージ結果（内部処理）")]
    [Tooltip("Animator Controllerと追加Clipを名前ベースでマージした結果")]
    public List<AnimationClip> mergedClips = new();

    private void Awake()
    {
        mergedClips = MergeAnimationClips();
        Debug.Log($"[SpriteToolContext] マージ完了：{mergedClips.Count}個のAnimationClip");
    }

    private List<AnimationClip> MergeAnimationClips()
    {
        var clipDict = new Dictionary<string, AnimationClip>();

        // AnimatorControllerから取得
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

        // 追加Clipから取得
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
