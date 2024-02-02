using Scripts.Utils;
using SuperTiled2Unity;
using UnityEngine;

namespace Scripts.Map
{
    public class MapEntry : MonoBehaviour
    {
        private static int GlobalIdIndex = 0;        
        
        public int Id { get; private set; }
        [SerializeField] private SuperMap map;
        [SerializeField] private Grid grid;
        [SerializeField] private MapEntry[] adjacentMaps;
        [SerializeField] private EnemySpawnPoint[] spawnPoints;

        private const short mapMinPosX = 0;
        private const short mapMinPosY = -1;

        public void Awake()
        {
            Id = GlobalIdIndex;
            GlobalIdIndex++;

            spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
        }

        // Spawn an enemy at a random spawn-point
        public bool TryGetRandomSpawnPosition(out Vector3 position)
        {
            if (spawnPoints.Length > 0)
            {
                var randomPoint = spawnPoints[Mathf.RoundToInt(Random.value * (spawnPoints.Length - 1))]
                    .transform.position;
                if (IsWorldPosInsideMap(randomPoint, out var gridPos))
                {
                    position = GetWorldPosAtCenterOfGridPos(gridPos);
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        // Spawn enemies at a specific spawn point (used in GameplayManager to spawn an enemy at each spawn-point at once)
        public bool TryGetSpawnPosition(int spawnPoint, out Vector3 position)
        {
            if (spawnPoints.Length > 0)
            {
                if (IsWorldPosInsideMap(spawnPoints[spawnPoint].transform.position, out var gridPos))
                {
                    position = GetWorldPosAtCenterOfGridPos(gridPos);
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        public bool IsWorldPosInsideMap(Vector3 worldPos, out Vector2Int gridPos)
        {
            gridPos = grid.WorldToCell(worldPos).ToVector2Int();
            
            return gridPos.x >= mapMinPosX &&
                   gridPos.y <= mapMinPosY &&
                   gridPos.x <= (map.m_Width - 1) &&
                   gridPos.y >= -map.m_Height;
        }

        public Vector3 GetWorldPosAtCenterOfGridPos(Vector2Int gridPos)
        {
            return grid.GetCellCenterWorld(gridPos.ToVector3Int());
        }
    }
}