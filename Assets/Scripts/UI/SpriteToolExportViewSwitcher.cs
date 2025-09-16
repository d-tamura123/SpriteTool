using UnityEngine;
using UnityEngine.UI;

public class SpriteToolExportViewSwitcher : MonoBehaviour
{
    [Header("�\���ؑփg�O��")]
    public Toggle viewSwitchToggle;

    [Header("�\���Ώ�")]
    public GameObject logScrollView;
    public GameObject pngScrollView;

    private void Start()
    {
        // ������ԁFLog�\���APNG��\��
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
