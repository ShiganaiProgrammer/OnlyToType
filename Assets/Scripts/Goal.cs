using UnityEngine;

public class Goal : MonoBehaviour
{
    public event System.Action OnPlayerReached;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<AutoScrollPlayer>() != null)
            OnPlayerReached?.Invoke();
    }
}
