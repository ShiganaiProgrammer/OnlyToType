using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset = new(0f, 1.5f, -10f);
    [SerializeField] float smoothSpeed = 5f;

    Transform target;
    float minX;

    public void SetTarget(Transform player)
    {
        target = player;
        ResetPosition();
    }

    public void ResetPosition()
    {
        if (target == null) return;

        minX = target.position.x + offset.x;
        transform.position = new Vector3(minX, target.position.y + offset.y, offset.z);
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = Mathf.Max(minX, target.position.x + offset.x);
        minX = targetX;

        var desired = new Vector3(targetX, target.position.y + offset.y, offset.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
