using SFB; // StandaloneFileBrowserの名前空間
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class SpriteToolExportController : MonoBehaviour
{
    [Header("参照コンテキスト")]
    public SpriteToolContext context;
    public SpriteToolPreviewPlayer previewPlayer;
    public Camera exportCamera;
    public Camera otherCamera;

    [Header("UI参照")]
    public TMP_Dropdown directionsDropdown;
    public TMP_Dropdown clipDropdown;
    public TMP_InputField frameNumInput;
    public TMP_InputField chipWidthInput;
    public TMP_InputField chipHeightInput;
    public TMP_InputField cameraZoomInput;
    public TMP_InputField cameraFocusHeightInput;
    public TMP_InputField cameraPitchInput;
    public Toggle orthographicToggle;
    public Toggle splitFileToggle;
    public RawImage pngPreviewRawImage;
    public LogScroller logScroller;
    private SpriteToolExporter exporter;
    public Toggle ViewSwitchToggle;


    private void Start()
    {
        if (previewPlayer == null || exportCamera == null || previewPlayer.previewRawImage == null)
        {
            Debug.LogError("SpriteToolExportController：PreviewPlayerまたはexportCameraの参照が不足しています");
            return;
        }

        // RenderTexture は RawImage から取得
        exporter = new SpriteToolExporter(exportCamera);
    }

    public void OnLoadButtonPressed()
    {
        if (exporter == null || context == null || context.mergedClips == null)
        {
            Debug.LogError("SpriteToolExportController：初期化が不完全です");
            return;
        }

        previewPlayer.StopPreview(false);
        previewPlayer.enabled = false;
        
        ExportSettings settings = CollectSettingsFromUI();
        exporter.SetCharacterInstance(previewPlayer.GetCharacterInstance());
        exporter.ConfigureRenderTexture(settings.chipWidth, settings.chipHeight);
        
        //exporter.CaptureMotionFrames(settings);
        StartCoroutine(CaptureAndApply(settings));
        
    }

    private IEnumerator CaptureAndApply(ExportSettings settings)
    {
        yield return StartCoroutine(exporter.CaptureMotionFrames(settings));

        if (exporter.composedSpriteSheet != null)
        {
            pngPreviewRawImage.texture = exporter.composedSpriteSheet;
            StartCoroutine(ApplyGridCellSizeNextFrame());

            logScroller.AddLog(
                $"スプライトシート生成成功：{exporter.composedSpriteSheet.width}x{exporter.composedSpriteSheet.height} / {exporter.capturedFrames.Length}フレーム",
                LogLevel.Info
            );
            ViewSwitchToggle.isOn = true;
        }
        else
        {
            Debug.LogWarning("SpriteToolExportController：プレビュー用スプライトシートが生成されていません");
        }

        previewPlayer.enabled = true;
    }

    private IEnumerator ApplyGridCellSizeNextFrame()
    {
        yield return null; // 1フレーム待機
        GridLayoutGroup grid = pngPreviewRawImage.GetComponentInParent<GridLayoutGroup>();
        if (grid != null && exporter.composedSpriteSheet != null)
        {
            grid.cellSize = new Vector2(exporter.composedSpriteSheet.width, exporter.composedSpriteSheet.height);
            Canvas.ForceUpdateCanvases();
        }
    }

    public void OnExportButtonPressed()
    {
        if (exporter == null)
        {
            Debug.LogError("SpriteToolExportController：exporterが未初期化です");
            return;
        }

        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Export Folder", "", false);
        if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string outputPath = paths[0];
            bool success = exporter.ExportToFiles(outputPath);
            Debug.Log(success ? "出力成功" : "出力失敗");
        }
    }

    private ExportSettings CollectSettingsFromUI()
    {
        AnimationClip selectedClip = null;
        int index = clipDropdown.value;

        if (index > 0 && index - 1 < context.mergedClips.Count)
        {
            selectedClip = context.mergedClips[index - 1];
        }

        return new ExportSettings
        {
            captureDirections = directionsDropdown.value == 0 ? 4 : 8,
            frameCount = int.Parse(frameNumInput.text),
            chipWidth = int.Parse(chipWidthInput.text),
            chipHeight = int.Parse(chipHeightInput.text),
            cameraZoom = float.Parse(cameraZoomInput.text),
            cameraFocusHeight = float.Parse(cameraFocusHeightInput.text),
            cameraPitch = float.Parse(cameraPitchInput.text),
            orthographic = orthographicToggle.isOn,
            splitFiles = splitFileToggle.isOn,
            clip = selectedClip
        };
    }
}
