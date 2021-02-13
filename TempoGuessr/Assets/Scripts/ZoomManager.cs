using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AlmostEngine;

public class ZoomManager : Singleton<ZoomManager>
{
    [SerializeField] Image image;
    [SerializeField] float openCloseDelay = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    Coroutine zoomCoroutine = null;
    private void StopZoom()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
    }

    public void TurnOn(Sprite sprite)
    {
        StopZoom();
        image.sprite = sprite;
        zoomCoroutine = StartCoroutine(ZoomUntil(1.0f));
    }

    public void TurnOff()
    {
        StopZoom();
        zoomCoroutine = StartCoroutine(ZoomUntil(0.0f));
    }

    IEnumerator ZoomUntil(float scale)
    {
        Vector3 vecTarget = new Vector3(scale, scale, scale);
        Vector3 velocity = Vector3.zero;

        while (gameObject.transform.localScale != vecTarget)
        {
            gameObject.transform.localScale = Vector3.SmoothDamp(gameObject.transform.localScale, vecTarget, ref velocity, 0.2f);
            yield return null;
        }
        yield break;
    }
}
