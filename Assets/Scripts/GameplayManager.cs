using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Map;
using Scripts.Player;
using Scripts.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Spawn variables for enemy wave spawning
        [Header("Enemy Wave Spawn Perameters")]
        [SerializeField] private bool stopSpawning;
        [SerializeField] private float spawnTimer;
        [SerializeField] private float spawnDelay;

        // GameObject which controls background audio
        [Header("Background Audio Object/Script")]
        [SerializeField] private BackgroundAudio bgAudio;

        //Trigger Audio and UI events
        [Header("Events")]
        [SerializeField] private UnityEvent GameOver;

        // Array of enemies
        //private List<GameObject> enemies;
        private GameObject[] enemies;

        // Enemy type select
        [SerializeField] private int waveNumber = 0;
        [SerializeField] private EnemyType enemyType;
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        // Checks if the current 'state of the game' (is the player dead, is the game paused, is the player in combat, etc.)
        // This is to trigger events which control background music play and possibly UI changes
        void CheckGameState()
        {
            // Enemy dies and Game Over
            if (player.CurrentLife == 0)
            {
                GameOver?.Invoke();
                print("Game Over");
                Time.timeScale = 0.0f;
            }
        }

        // Spawns enemies into the scene and plays the combat music
        public void EnemyWaveSpawner()
        {
            if(bgAudio.combatMusic.isPlaying == false)
            {
                bgAudio.combatMusic.volume = 1.0f;
                bgAudio.playCombatMusic();
            }

            // Determine enemy type based on wave number
            if (waveNumber % 2 == 0)
            {
                enemyType = EnemyType.Thing;
            }
            else
            {
                enemyType = EnemyType.Crab;
            }

            // Spawn multiple enemies
            for (int i = 0; i < 4; i++)
            {

                if (currentMap.TryGetSpawnPosition(i, out var position))
                {
                    var newEnemy = enemyPalette.GetEnemyPrefab(enemyType);
                    Instantiate(newEnemy, position, Quaternion.identity, enemiesContainer);
                }
            }
        }

        // Playes wave incoming SFX, then delays the spawning of the enemies into the scene
        public void SpawnEnemies()
        {
            if (bgAudio.combatMusic.isPlaying == false)
            {
                bgAudio.WaveAlertSound();
            }

            waveNumber++;

            // Delay the spawning of enemies until the count-down is complete
            Invoke("EnemyWaveSpawner", 4.25f);
        }

        private void Start()
        {
            // Play background music at game startup
            bgAudio.playBackgroundMusic();

            // Conditionals to check if enemies must be spawned given a certain time interval
            if (!stopSpawning)
            {
                InvokeRepeating("SpawnEnemies", spawnTimer, spawnDelay);
            }
            
            if(stopSpawning)
            {
                CancelInvoke("SpawnEnemies");
            }
        }

        void Update()
        {
            lifebar.SetLife(player.CurrentLife);

            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // Check if enemy wave has been cleared and play background music
            if(enemies.Length == 0 && bgAudio.outOfCombatMusic.isPlaying == false)
            {
                float startingVol = bgAudio.combatMusic.volume;

                //try to fade out combat music (needs ammending)
                while (bgAudio.combatMusic.volume > 0)
                {
                    bgAudio.combatMusic.volume -= startingVol - Time.deltaTime / 5;
                }

                bgAudio.playBackgroundMusic();
            }

            UpdateCurrentMap();

            CheckGameState();

            // Old method of spawning enemies
            //if (Input.GetKeyUp(KeyCode.Alpha0))
            //{
            //    // Spawn multiple enemies
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (currentMap.TryGetSpawnPosition(i, out var position))
            //        {
            //            var newEnemy = enemyPalette.GetEnemyPrefab(EnemyType.Crab);
            //            Instantiate(newEnemy, position, Quaternion.identity, enemiesContainer);

            //            // Add enemy to an array
            //            //System.Array.Resize(ref enemies, +1);
            //            //enemies[i] = newEnemy;
            //            //print(enemies);
            //        }
            //    } 
            //}
        }
    }
}