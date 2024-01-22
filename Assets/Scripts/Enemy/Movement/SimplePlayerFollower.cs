using UnityEngine;

namespace Scripts.Map.Movement
{
    public class SimplePlayerFollower : IPlayerFollower
    {
        public Vector2 GetMoveDirection(Vector3 worldPosition, out float sqrDistance)
        {
            var playerGridPos = BigBadSingleton.Instance.GameplayManager.GetPlayerGridPosition();
            var gridPos = BigBadSingleton.Instance.GameplayManager.GetGridPosition(worldPosition, out var mapId);

            var dir = playerGridPos - gridPos;
            sqrDistance = dir.sqrMagnitude;

            return dir;
        }
    }
}