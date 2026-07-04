using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var p = transform.position;
        p.x = cam.transform.position.x;
        transform.position = p;
    }
}
