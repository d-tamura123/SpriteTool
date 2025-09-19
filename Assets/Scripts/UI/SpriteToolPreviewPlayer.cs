using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class SpriteToolPreviewPlayer : MonoBehaviour
{
    [Header("参照コンテキスト")]
    public SpriteToolContext context;

    [Header("UI入力")]
    public TMP_Dropdown clipDropdown;
    public TMP_InputField cameraZoomInput;
    public TMP_InputField cameraFocusHeightInput;
    public TMP_InputField cameraPitchInput;
    public Toggle cameraOrthographicToggle;

    [Header("プレビュー表示")]
    public RawImage previewRawImage;
    public Camera previewCamera;

    [Header("ログ表示")]
    public LogScroller logScroller;

    [Header("制御フラグ")]
    public bool isEnabled = true;

    private GameObject characterInstance;
    private Animator characterAnimator;
    private AnimationClip currentClip;

    private float cameraZoom = 3f;
    private float cameraFocusHeight = 1.5f;
    private float cameraPitch = 45f;

    private bool hasAnimationError = false;

    private RenderTexture previewTexture;

    // ログ出力制御フラグ（Update内でスパム化を防ぐ）
    private bool hasZoomInputError = false;
    private bool hasFocusHeightInputError = false;
    private bool hasPitchInputError = false;

    // Playables API用の再生構造
    private PlayableGraph playableGraph;
    public GameObject GetCharacterInstance()
    {
        return characterInstance;
    }
    private void Start()
    {
        if (!ValidateContext()) return;
        InitializeRenderTexture();
        InitializeCharacter();
        ApplyCameraSettings();
        ApplyAnimationClip();
        SetCameraBackground(PreviewMode.Preview);
    }

    private void Update()
    {
        if (!isEnabled || characterInstance == null) return;

        ApplyCameraSettings();
        ApplyAnimationClip();
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }
    }

    private bool ValidateContext()
    {
        if (context == null || context.characterPrefab == null || clipDropdown == null || previewCamera == null || previewRawImage == null)
        {
            logScroller?.AddLog("PreviewPlayer：必要な参照が不足しています", LogLevel.Warn);
            return false;
        }
        return true;
    }

    private void InitializeRenderTexture()
    {
        previewTexture = new RenderTexture(512, 512, 16); // サイズは任意、UIに合わせて調整
        previewTexture.Create();

        previewCamera.targetTexture = previewTexture;
        previewRawImage.texture = previewTexture;

        logScroller?.AddLog("RenderTextureを初期化し、RawImageに接続しました", LogLevel.Info);
    }

    private void InitializeCharacter()
    {
        characterInstance = Instantiate(context.characterPrefab);
        characterInstance.transform.position = Vector3.zero;
        characterAnimator = characterInstance.GetComponent<Animator>();

        if (characterAnimator == null)
        {
            logScroller?.AddLog("PreviewPlayer：Animatorが見つかりません", LogLevel.Warn);
        }

        // キャラクターの向きを Z-（南向き）に統一
        Vector3 currentForward = characterInstance.transform.forward;
        float angleToSouth = Vector3.SignedAngle(currentForward, Vector3.back, Vector3.up);
        characterInstance.transform.rotation *= Quaternion.Euler(0, angleToSouth, 0);

        logScroller?.AddLog($"キャラクターの向きを南向きに補正しました（{angleToSouth:F1}°）", LogLevel.Info);
    }

    private void ApplyCameraSettings()
    {
        if (!float.TryParse(cameraZoomInput.text, out float dist))
        {
            if (!hasZoomInputError)
            {
                logScroller?.AddLog("カメラ距離/ズームの入力が不正です", LogLevel.Warn);
                hasZoomInputError = true;
            }
            dist = cameraZoom;
        }
        else
        {
            hasZoomInputError = false;
        }

        if (!float.TryParse(cameraFocusHeightInput.text, out float focusHeight))
        {
            if (!hasFocusHeightInputError)
            {
                logScroller?.AddLog("カメラ高さの入力が不正です", LogLevel.Warn);
                hasFocusHeightInputError = true;
            }
            focusHeight = cameraFocusHeight;
        }
        else
        {
            hasFocusHeightInputError = false;
        }

        if (!float.TryParse(cameraPitchInput.text, out float pitch))
        {
            if (!hasPitchInputError)
            {
                logScroller?.AddLog("カメラ角度（Pitch）の入力が不正です", LogLevel.Warn);
                hasPitchInputError = true;
            }
            pitch = cameraPitch;
        }
        else
        {
            hasPitchInputError = false;
        }

        cameraZoom = dist;
        cameraFocusHeight = focusHeight;
        cameraPitch = pitch;

        float yaw = 0f; // Previewでは南向き固定（Z-）

        SpriteToolCameraUtility.ConfigureCamera(
            previewCamera,
            characterInstance.transform.position,
            cameraZoom,
            cameraFocusHeight,
            cameraPitch,
            yaw,
            cameraOrthographicToggle.isOn
        );
    }

    private void ApplyAnimationClip()
    {
        int index = clipDropdown.value;

        // index 0 は「アニメーションなし」
        if (index == 0)
        {
            currentClip = null;
            return;
        }

        int clipIndex = index - 1;
        if (clipIndex < 0 || clipIndex >= context.mergedClips.Count) return;

        AnimationClip selectedClip = context.mergedClips[clipIndex];
        if (selectedClip == currentClip) return;

        try
        {
            Animator animator = characterInstance.GetComponent<Animator>();
            if (animator == null)
            {
                logScroller?.AddLog("Animatorが見つかりません（Playables再生不可）", LogLevel.Warn);
                return;
            }

            // 既存のGraphを停止・破棄
            if (playableGraph.IsValid())
            {
                playableGraph.Stop();
                playableGraph.Destroy();
            }

            // 新しいGraphを構築
            playableGraph = PlayableGraph.Create("PreviewGraph");
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, selectedClip);

            clipPlayable.SetApplyFootIK(false);
            clipPlayable.SetApplyPlayableIK(false);
            clipPlayable.SetDuration(selectedClip.length);
            clipPlayable.SetTime(0);
            clipPlayable.SetSpeed(1);

            playableOutput.SetSourcePlayable(clipPlayable);
            playableGraph.Play();

            currentClip = selectedClip;
            hasAnimationError = false; // 成功したらエラー状態を解除

            logScroller?.AddLog($"Playables APIで再生開始：{selectedClip.name}", LogLevel.Info);
        }
        catch (System.Exception e)
        {
            if (!hasAnimationError)
            {
                logScroller?.AddLog($"Playables再生エラー：{selectedClip.name} → {e.Message}", LogLevel.Error);
                hasAnimationError = true;
            }

            currentClip = null;
        }
    }

    public void StopPreview(bool destroyCharacter = true)
    {
        isEnabled = false;

        if (playableGraph.IsValid())
        {
            playableGraph.Stop();
            playableGraph.Destroy();
        }

        if (destroyCharacter && characterInstance != null)
        {
            Destroy(characterInstance);
            characterInstance = null;
        }

        logScroller?.AddLog("プレビュー停止", LogLevel.Info);
    }

    public enum PreviewMode
    {
        Preview,
        Export
    }

    public void SetCameraBackground(PreviewMode mode)
    {
        previewCamera.clearFlags = CameraClearFlags.SolidColor;

        if (mode == PreviewMode.Preview)
        {
            previewCamera.backgroundColor = Color.green; // または Color.black
        }
        else if (mode == PreviewMode.Export)
        {
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // 透明
        }
        previewCamera.enabled = true;
    }
}
