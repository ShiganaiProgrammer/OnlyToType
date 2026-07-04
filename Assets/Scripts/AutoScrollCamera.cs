using System.Collections;
using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset = new(6f, 1.5f, -10f);
    [SerializeField] float smoothSpeed = 5f;

    Transform target;
    float minX;
    Vector3 shakeOffset;

    public void SetTarget(Transform player)
    {
        target = player;
        ResetPosition();
    }

    public void ResetPosition()
    {
        if (target == null) return;

        minX = target.position.x + offset.x;
        var pos = new Vector3(minX, target.position.y + offset.y, offset.z);
        pos.z = Mathf.Max(pos.z, -0.890449f);
        transform.position = pos;
    }

    public void Shake(float duration, float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, intensity));
    }

    IEnumerator ShakeRoutine(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f) * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = Mathf.Max(minX, target.position.x + offset.x);
        minX = targetX;

        var desired = new Vector3(targetX, target.position.y + offset.y, offset.z);
        var pos = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime) + shakeOffset;
        pos.z = Mathf.Max(pos.z, -0.890449f);
        transform.position = pos;
    }
}
