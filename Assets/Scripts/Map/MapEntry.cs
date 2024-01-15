using Scripts.Utils;
using SuperTiled2Unity;
using UnityEngine;

namespace Scripts.Map
{
    public class MapEntry : MonoBehaviour
    {
        [SerializeField] private SuperMap map;
        [SerializeField] private Grid grid;
        [SerializeField] private MapEntry[] adjacentMaps;

        private const short mapMinPosX = 0;
        private const short mapMinPosY = -1;

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