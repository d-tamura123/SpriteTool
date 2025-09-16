using UnityEngine;

// �v���r���[��ʁE�摜�o�͂ŋ��ʂ��Ďg���J�����̃��[�e�B���e�B
public static class SpriteToolCameraUtility
{
    public static void ConfigureCamera(
        Camera camera,
        Vector3 characterPosition,
        float zoom,
        float focusHeight,
        float pitch,
        float yaw,
        bool orthographic)
    {
        Vector3 forwardDirection = new Vector3(0, 0, 1);
        Vector3 pitchRotated = Quaternion.AngleAxis(pitch, Vector3.right) * forwardDirection;
        Vector3 yawRotated = Quaternion.AngleAxis(yaw, Vector3.up) * pitchRotated;
        Vector3 direction = yawRotated.normalized;

        Vector3 target = characterPosition + new Vector3(0, focusHeight, 0);
        Vector3 cameraPos = target - direction * zoom;

        camera.transform.position = cameraPos;
        camera.transform.rotation = Quaternion.LookRotation(target - cameraPos);
        camera.orthographic = orthographic;
        camera.orthographicSize = zoom;
    }
}
