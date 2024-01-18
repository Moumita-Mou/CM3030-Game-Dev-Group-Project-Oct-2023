using UnityEngine;

namespace Scripts
{
    public class ProtoCameraController : MonoBehaviour
    {
        [SerializeField] private GameObject objectToFollow;

        private Vector3 currentVelocity;

        [Header("Settings")] [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;

        // Update is called once per frame
        void LateUpdate()
        {
            var objectToFollowPos = objectToFollow.transform.position;
            var currentPos = transform.position;

            var targetPosition = new Vector3(objectToFollowPos.x, objectToFollowPos.y, currentPos.z);

            transform.position = Vector3.SmoothDamp(currentPos, targetPosition,
                ref currentVelocity, smoothTime, maxSpeed);
        }
    }
}
