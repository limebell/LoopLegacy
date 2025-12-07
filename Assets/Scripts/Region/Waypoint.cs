using UnityEngine;
using UnityEngine.Events;

namespace LoopLegacy.Region
{
    public class Waypoint : MonoBehaviour
    {
        public UnityEvent OnWaypointReached;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                OnWaypointReached?.Invoke();
            }
        }

        private void OnDestroy()
        {
            OnWaypointReached?.RemoveAllListeners();
        }
    }
}
