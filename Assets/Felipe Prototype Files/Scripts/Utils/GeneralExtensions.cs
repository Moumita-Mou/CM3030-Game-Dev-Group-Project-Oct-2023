using UnityEngine;

namespace Scripts.Utils
{
    public static class GeneralExtensions
    {
        public static Vector2Int ToVector2Int(this Vector3Int vec3Int)
        {
            return new Vector2Int(vec3Int.x, vec3Int.y);
        }
        public static Vector3Int ToVector3Int(this Vector2Int vec2Int)
        {
            return new Vector3Int(vec2Int.x, vec2Int.y, 0);
        }
        public static Vector2Int ToVector2(this Vector3Int vec3)
        {
            return new Vector2Int(vec3.x, vec3.y);
        }
        public static Vector3Int ToVector3(this Vector2Int vec2)
        {
            return new Vector3Int(vec2.x, vec2.y, 0);
        }
    }
}