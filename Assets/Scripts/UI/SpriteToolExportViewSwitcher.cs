using UnityEngine;
using UnityEngine.UI;

public class SpriteToolExportViewSwitcher : MonoBehaviour
{
    [Header("表示切替トグル")]
    public Toggle viewSwitchToggle;

    [Header("表示対象")]
    public GameObject logScrollView;
    public GameObject pngScrollView;

    private void Start()
    {
        // 初期状態：Log表示、PNG非表示
        SetViewState(showPNG: false);

        if (viewSwitchToggle != null)
        {
            viewSwitchToggle.onValueChanged.AddListener(SetViewState);
        }
    }

    private void SetViewState(bool showPNG)
    {
        if (logScrollView != null) logScrollView.SetActive(!showPNG);
        if (pngScrollView != null) pngScrollView.SetActive(showPNG);
    }
}
