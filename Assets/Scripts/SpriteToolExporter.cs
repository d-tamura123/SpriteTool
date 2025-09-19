using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public struct ExportSettings
{
    public int captureDirections; // 4 or 8
    public int frameCount;
    public int chipWidth;
    public int chipHeight;
    public float cameraZoom;
    public float cameraFocusHeight;
    public float cameraPitch;
    public bool orthographic;
    public bool splitFiles;
    public AnimationClip clip;
}

public class SpriteToolExporter
{
    public Texture2D[] capturedFrames { get; private set; }
    public Texture2D composedSpriteSheet { get; private set; }
    public ExportSettings settings { get; private set; }

    private GameObject characterInstance;
    private Animator animator;
    private PlayableGraph playableGraph;
    private Camera captureCamera;
    private RenderTexture renderTexture;

    public SpriteToolExporter(Camera camera)
    {
        captureCamera = camera;

        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0, 0, 0, 0); // ���S����
        captureCamera.allowHDR = false; // HDR�������œ����w�i���ێ�
    }
    public void ConfigureRenderTexture(int chipWidth, int chipHeight)
    {
        int width = chipWidth;
        int height = chipHeight;

        // ������RenderTexture������ꍇ�A�T�C�Y����v���Ă���΍ė��p
        if (renderTexture != null)
        {
            if (renderTexture.width == width && renderTexture.height == height)
            {
                return; // �T�C�Y�ύX�Ȃ� �� �ė��p
            }

            renderTexture.Release();
            Object.Destroy(renderTexture);
            renderTexture = null;
        }

        //renderTexture = new RenderTexture(width, height, 16);
        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        captureCamera.targetTexture = renderTexture;
    }


    public void SetCharacterInstance(GameObject instance)
    {
        characterInstance = instance;
        animator = characterInstance?.GetComponent<Animator>();
    }

    public IEnumerator CaptureMotionFrames(ExportSettings inputSettings)
    {
        settings = inputSettings;
        int totalFrames = settings.captureDirections * settings.frameCount;
        capturedFrames = new Texture2D[totalFrames];

        if (characterInstance == null)
        {
            Debug.LogWarning("SpriteToolExporter�F�L�����N�^�[�����ݒ�̂��߃L���v�`���s��");
            yield break;
        }
        
        bool usePlayable = (animator != null && settings.clip != null);
        AnimationClipPlayable clipPlayable = default;

        if (usePlayable)
        {
            playableGraph = PlayableGraph.Create("CaptureGraph");
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            clipPlayable = AnimationClipPlayable.Create(playableGraph, settings.clip);

            clipPlayable.SetApplyFootIK(false);
            clipPlayable.SetApplyPlayableIK(false);
            clipPlayable.SetDuration(settings.clip.length);
            clipPlayable.SetSpeed(0);
            playableOutput.SetSourcePlayable(clipPlayable);
            playableGraph.Play();
        }

        Vector3 charPos = characterInstance.transform.position;
        float frameInterval = usePlayable ? settings.clip.length / settings.frameCount : 0f;

        for (int dir = 0; dir < settings.captureDirections; dir++)
        {
            float yaw = dir * (360f / settings.captureDirections);

            for (int frame = 0; frame < settings.frameCount; frame++)
            {
                if (usePlayable)
                {
                    float time = frame * frameInterval;
                    clipPlayable.SetTime(time);
                    yield return null; // Pose�]���̂���1�t���[���҂�
                    playableGraph.Evaluate();
                }

                SpriteToolCameraUtility.ConfigureCamera(
                    captureCamera,
                    charPos,
                    settings.cameraZoom,
                    settings.cameraFocusHeight,
                    settings.cameraPitch,
                    yaw,
                    settings.orthographic
                );

                int index = dir * settings.frameCount + frame;
                captureCamera.Render();
                capturedFrames[index] = CaptureFrame(captureCamera, renderTexture);

                if (capturedFrames[index] == null)
                {
                    Debug.LogWarning($"CaptureMotionFrames�F�L���v�`�����s index={index}");
                }
            }
        }

        if (usePlayable && playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }

        ComposeSpriteSheet(); // �L���v�`����ɍ���
    }
    private void ComposeSpriteSheet()
    {
        if (capturedFrames == null || capturedFrames.Length == 0)
        {
            composedSpriteSheet = null;
            return;
        }

        int framesPerDirection = settings.frameCount;
        int directionCount = settings.captureDirections;

        int columns = framesPerDirection; // ���Ƀt���[��
        int rows = directionCount;        // �c�ɕ���

        Texture2D sheet = new Texture2D(
            settings.chipWidth * columns,
            settings.chipHeight * rows,
            TextureFormat.RGBA32,
            false
        );

        for (int i = 0; i < capturedFrames.Length; i++)
        {
            if (capturedFrames[i] == null)
            {
                Debug.LogWarning($"ComposeSpriteSheet�Fframes[{i}] �� null �ł��B�󔒂Ƃ��ď������܂�");
                continue;
            }

            int dirIndex = i / framesPerDirection;
            int frameIndex = i % framesPerDirection;

            int x = frameIndex * settings.chipWidth;
            int y = (rows - 1 - dirIndex) * settings.chipHeight;

            sheet.SetPixels(x, y, settings.chipWidth, settings.chipHeight, capturedFrames[i].GetPixels());
        }

        sheet.Apply();
        composedSpriteSheet = sheet;
    }


    public bool ExportToFiles(string outputPath)
    {
        if (capturedFrames == null || settings.Equals(default)) return false;

        if (settings.splitFiles)
        {
            return ExportMultiplePNGs(outputPath);
        }
        else
        {
            if (composedSpriteSheet == null)
            {
                Debug.LogWarning("ExportToFiles�F�X�v���C�g�V�[�g���������ł�");
                return false;
            }

            string path = Path.Combine(outputPath, "spritesheet.png");
            return ExportPNG(composedSpriteSheet, path);
        }
    }

    private static Texture2D CaptureFrame(Camera camera, RenderTexture renderTexture)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;
        return tex;
    }

    private bool ExportMultiplePNGs(string basePath)
    {
        for (int i = 0; i < capturedFrames.Length; i++)
        {
            if (capturedFrames[i] == null)
            {
                Debug.LogWarning($"ExportMultiplePNGs�Fframes[{i}] �� null �̂��߃X�L�b�v");
                continue;
            }

            string path = Path.Combine(basePath, $"frame_{i:D2}.png");
            if (!ExportPNG(capturedFrames[i], path)) return false;
        }
        return true;
    }

    private static bool ExportPNG(Texture2D tex, string outputPath)
    {
        if (tex == null) return false;

        byte[] pngData = tex.EncodeToPNG();
        if (pngData == null || pngData.Length == 0) return false;

        try
        {
            File.WriteAllBytes(outputPath, pngData);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PNG�����o���G���[ �� {e.Message}");
            return false;
        }
    }
}
