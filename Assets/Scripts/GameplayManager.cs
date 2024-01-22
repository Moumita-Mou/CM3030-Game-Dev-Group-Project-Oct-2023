using System;
using Scripts.Map;
using Scripts.Player;
using Scripts.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Scripts
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private Transform debugGridPositionImage;
        [SerializeField] private EnemyPalette enemyPalette;
        [SerializeField] private FXPalette fxPalette;
        
        [Header("Containers")]
        [SerializeField] private Transform fxContainer;
        [SerializeField] private Transform enemiesContainer;
        
        [Header("UI")]
        [SerializeField] private UILifeBar lifebar;

        private PlayerController player;
        private MapEntry[] allMapsInScene;
        private MapEntry currentMap;

        public GameplayManager()
        {
            currentMap = null;
            allMapsInScene = Array.Empty<MapEntry>();
        }
        
        public void LoadSceneMaps(MapEntry[] allMaps)
        {
            allMapsInScene = allMaps;
        }

        public void LoadPlayer(PlayerController playerController)
        {
            Assert.IsTrue(playerController != null, "Couldn't find player instance!!!");
            player = playerController;
            player.Init();
            lifebar.SetTotalLife(player.TotalLife);
        }

        public Vector2Int GetGridPosition(Vector3 worldPos, out int mapId)
        {
            mapId = 0;
            foreach (var mapEntry in allMapsInScene)
            {
                if (mapEntry.IsWorldPosInsideMap(worldPos, out var gridPos))
                {
                    mapId = mapEntry.Id;
                    return gridPos;
                }
            }

            return Vector2Int.zero;
        }

        public Vector2Int GetPlayerGridPosition()
        {
            return GetGridPosition(GetPlayerWorldPosition(), out _);
        }

        public Vector3 GetPlayerWorldPosition()
        {
            return player.transform.position;
        }

        public void Debug_FocusWorldPositionInGrid(Vector3 worldPos, bool logPos)
        {
            foreach (var mapEntry in allMapsInScene)
            {
                if (mapEntry.IsWorldPosInsideMap(worldPos, out var gridPos))
                {
                    var gridCenterInWorldPos = mapEntry.GetWorldPosAtCenterOfGridPos(gridPos);
                    debugGridPositionImage.position = gridCenterInWorldPos;
                    if (logPos)
                    {
                        Debug.Log(gridPos);
                    }
                    mapEntry.gameObject.SetActive(true);
                }
                else
                {
                    mapEntry.gameObject.SetActive(false);
                }
            }
        }

        public Vector3 getGridCenterInWorldPos(Vector3 characterPos)
        {
            Vector3 gridCenterInWorldPos = new Vector3();

            foreach (var mapEntry in allMapsInScene)
            {
                if (mapEntry.IsWorldPosInsideMap(characterPos, out var gridPos))
                {
                    gridCenterInWorldPos = mapEntry.GetWorldPosAtCenterOfGridPos(gridPos);
                    mapEntry.gameObject.SetActive(true);
                }
                else
                {
                    mapEntry.gameObject.SetActive(false);
                }
            }
            
            return gridCenterInWorldPos;
        }

        void UpdateCurrentMap()
        {
            foreach (var mapEntry in allMapsInScene)
            {
                if (mapEntry.IsWorldPosInsideMap(player.transform.position, out var gridPos))
                {
                    var gridCenterInWorldPos = mapEntry.GetWorldPosAtCenterOfGridPos(gridPos);
                    debugGridPositionImage.position = gridCenterInWorldPos;
                    mapEntry.gameObject.SetActive(true);
                    currentMap = mapEntry;
                }
                else
                {
                    mapEntry.gameObject.SetActive(false);
                }
            }
        }

        public void SpawnFXAt(FXType fx, Vector3 worldPos)
        {
            Instantiate(fxPalette.GetFX(fx), worldPos, Quaternion.identity, fxContainer);
        }
        
        public void SpawnBigExplosionAt(Vector3 worldPos)
        {
            Instantiate(fxPalette.GetRandomBigExplosion(), worldPos, Quaternion.identity, fxContainer);
        }

        void Update()
        {
            lifebar.SetLife(player.CurrentLife);
            
            UpdateCurrentMap();
            
            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                if (currentMap.TryGetRandomSpawnPosition(out var position))
                {
                    var newEnemy = enemyPalette.GetEnemyPrefab(EnemyType.Crab);
                    Instantiate(newEnemy, position, Quaternion.identity, enemiesContainer);
                }
            }
        }
    }
}