using UnityEngine;

namespace LoopLegacy.Region
{
    public class Waypoint : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                TutorialManager.Instance.OnWaypointReached();
            }
        }
    }
}
