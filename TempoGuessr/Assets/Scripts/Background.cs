using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    [SerializeField] float loopDelay = 1.0f;

    Rect rectSize;
    Vector3 basePos;
    float maxRatio = 0.5f;

    RectTransform rectTransform;
    void Start()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        rectSize = rectTransform.rect;
        basePos = rectTransform.anchoredPosition;
        maxRatio = (rectSize.width / 2.0f - GetComponent<Image>().canvas.GetComponent<CanvasScaler>().referenceResolution.x / 2.0f) / rectSize.width;
    }

    float clamp = 0.0f;
    bool sens = true;
    float speed = 0.0f;
    float correction = 0.05f;
    void Update()
    {
        clamp = Mathf.SmoothDamp(clamp, (sens) ? maxRatio : -maxRatio, ref speed, loopDelay);

        if (Mathf.Abs(clamp) >= maxRatio - correction)
        {
            sens = !sens;
            clamp = Mathf.Clamp(clamp, -maxRatio + correction, maxRatio - correction);
        }

        var newPos = basePos;

        newPos.x += rectSize.width * clamp;
        rectTransform.anchoredPosition = newPos;
    }
}
