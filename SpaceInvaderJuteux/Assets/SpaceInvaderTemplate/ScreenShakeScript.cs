using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake instance;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ShakeScreenWithCurve(Camera camera, float intensity, float duration, AnimationCurve curve)
    {
        if (camera == null) return;
        if (shakeCoroutine == null)
        {
            shakeCoroutine = StartCoroutine(ShakeWithCurveCoroutine(camera, intensity, duration, curve));
        }
    }


    public void ShakeScreen(Camera camera, float intensity, float duration)
    {
        if (camera == null) return;
        if (shakeCoroutine == null && GameManager.Instance.vfx9Enabled)
        {
            shakeCoroutine = StartCoroutine(ShakeCoroutine(camera, intensity, duration));
        }
    }

    private IEnumerator ShakeWithCurveCoroutine(Camera camera, float intensity, float duration, AnimationCurve curve)
    {
        originalPosition = camera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = intensity * curve.Evaluate(elapsed / duration);

            float x = (Random.value * 2 - 1) * strength;
            float y = (Random.value * 2 - 1) * strength;

            camera.transform.localPosition = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }

        camera.transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    private IEnumerator ShakeCoroutine(Camera camera, float intensity, float duration)
    {
        originalPosition = camera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = (Random.value * 2 - 1) * intensity;
            float y = (Random.value * 2 - 1) * intensity;

            camera.transform.localPosition = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }

        camera.transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
