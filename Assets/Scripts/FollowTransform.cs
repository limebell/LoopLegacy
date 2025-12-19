using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform _target;

    // Update is called once per frame
    void Update()
    {
        if (_target != null)
        {
            transform.position = _target.position;
        }
    }
}
