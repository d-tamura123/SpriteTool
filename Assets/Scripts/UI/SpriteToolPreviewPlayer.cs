using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class SpriteToolPreviewPlayer : MonoBehaviour
{
    [Header("�Q�ƃR���e�L�X�g")]
    public SpriteToolContext context;

    [Header("UI����")]
    public TMP_Dropdown clipDropdown;
    public TMP_InputField cameraZoomInput;
    public TMP_InputField cameraFocusHeightInput;
    public TMP_InputField cameraPitchInput;
    public Toggle cameraOrthographicToggle;

    [Header("�v���r���[�\��")]
    public RawImage previewRawImage;
    public Camera previewCamera;

    [Header("���O�\��")]
    public LogScroller logScroller;

    [Header("����t���O")]
    public bool isEnabled = true;

    private GameObject characterInstance;
    private Animator characterAnimator;
    private AnimationClip currentClip;

    private float cameraZoom = 3f;
    private float cameraFocusHeight = 1.5f;
    private float cameraPitch = 45f;

    private bool hasAnimationError = false;

    private RenderTexture previewTexture;

    // ���O�o�͐���t���O�iUpdate���ŃX�p������h���j
    private bool hasZoomInputError = false;
    private bool hasFocusHeightInputError = false;
    private bool hasPitchInputError = false;

    // Playables API�p�̍Đ��\��
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
            logScroller?.AddLog("PreviewPlayer�F�K�v�ȎQ�Ƃ��s�����Ă��܂�", LogLevel.Warn);
            return false;
        }
        return true;
    }

    private void InitializeRenderTexture()
    {
        previewTexture = new RenderTexture(512, 512, 16); // �T�C�Y�͔C�ӁAUI�ɍ��킹�Ē���
        previewTexture.Create();

        previewCamera.targetTexture = previewTexture;
        previewRawImage.texture = previewTexture;

        logScroller?.AddLog("RenderTexture�����������ARawImage�ɐڑ����܂���", LogLevel.Info);
    }

    private void InitializeCharacter()
    {
        characterInstance = Instantiate(context.characterPrefab);
        characterInstance.transform.position = Vector3.zero;
        characterAnimator = characterInstance.GetComponent<Animator>();

        if (characterAnimator == null)
        {
            logScroller?.AddLog("PreviewPlayer�FAnimator��������܂���", LogLevel.Warn);
        }

        // �L�����N�^�[�̌����� Z-�i������j�ɓ���
        Vector3 currentForward = characterInstance.transform.forward;
        float angleToSouth = Vector3.SignedAngle(currentForward, Vector3.back, Vector3.up);
        characterInstance.transform.rotation *= Quaternion.Euler(0, angleToSouth, 0);

        logScroller?.AddLog($"�L�����N�^�[�̌����������ɕ␳���܂����i{angleToSouth:F1}���j", LogLevel.Info);
    }

    private void ApplyCameraSettings()
    {
        if (!float.TryParse(cameraZoomInput.text, out float dist))
        {
            if (!hasZoomInputError)
            {
                logScroller?.AddLog("�J��������/�Y�[���̓��͂��s���ł�", LogLevel.Warn);
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
                logScroller?.AddLog("�J���������̓��͂��s���ł�", LogLevel.Warn);
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
                logScroller?.AddLog("�J�����p�x�iPitch�j�̓��͂��s���ł�", LogLevel.Warn);
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

        float yaw = 0f; // Preview�ł͓�����Œ�iZ-�j

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

        // index 0 �́u�A�j���[�V�����Ȃ��v
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
                logScroller?.AddLog("Animator��������܂���iPlayables�Đ��s�j", LogLevel.Warn);
                return;
            }

            // ������Graph���~�E�j��
            if (playableGraph.IsValid())
            {
                playableGraph.Stop();
                playableGraph.Destroy();
            }

            // �V����Graph���\�z
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
            hasAnimationError = false; // ����������G���[��Ԃ�����

            logScroller?.AddLog($"Playables API�ōĐ��J�n�F{selectedClip.name}", LogLevel.Info);
        }
        catch (System.Exception e)
        {
            if (!hasAnimationError)
            {
                logScroller?.AddLog($"Playables�Đ��G���[�F{selectedClip.name} �� {e.Message}", LogLevel.Error);
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

        logScroller?.AddLog("�v���r���[��~", LogLevel.Info);
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
            previewCamera.backgroundColor = Color.green; // �܂��� Color.black
        }
        else if (mode == PreviewMode.Export)
        {
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // ����
        }
        previewCamera.enabled = true;
    }
}
