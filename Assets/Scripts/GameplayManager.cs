using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private SlowMoFX slowMoFX;
        [SerializeField] private ProtoCameraController cameraController;
        [SerializeField] private PlayerController playerPrefab;

        [Header("Containers")]
        [SerializeField] private Transform fxContainer;
        [SerializeField] private Transform enemiesContainer;
        [SerializeField] private Transform temporaryObjectsContainer;

        [Header("Settings")]
        public bool DoSlowMoFx = true;

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
        private MapEntry[] allMapsInScene = Array.Empty<MapEntry>();
        private MapEntry currentMap = null;

        private const int s_targetFrameRate = 60;

        private float lastPlayerGridPosRequestTimeStamp = 0;
        private Vector3Int cachedPlayerGridPos;

        private float lastPlayerPosRequestTimeStamp = 0;
        private Vector3 cachedPlayerPos;

        public Rigidbody2D PlayerRigidBody => player.Rigidbody;

        public PlayerController Player => player;

        private bool fadeInComplete = false;

        void Awake()
        {
            Application.targetFrameRate = s_targetFrameRate;
        }

        public void LoadSceneMaps()
        {
            allMapsInScene = FindObjectsOfType<MapEntry>();
        }

        public void LoadPlayer()
        {
            var playerController = FindObjectsOfType<PlayerController>().FirstOrDefault();
            if (!playerController)
            {
                var playerSpawnPoint = FindObjectsOfType<PlayerSpawnPoint>().FirstOrDefault();
                if (playerSpawnPoint != null)
                {
                    playerController = Instantiate(playerPrefab,
                        playerSpawnPoint.transform.position,
                        Quaternion.identity,
                        transform);
                }
            }

            Assert.IsTrue(playerController != null, "Couldn't find player instance or PlayerSpawner!");
            SetupPlayer(playerController);

            cameraController.SetTarget(playerController.gameObject);
        }

        private void SetupPlayer(PlayerController playerController)
        {
            player = playerController;
            player.Init();

            InitCurrentMap();
        }

        public Vector3Int GetGridPosition(Vector3 worldPos, out int mapId)
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

            return Vector3Int.zero;
        }

        public Vector3Int GetPlayerGridPosition()
        {
            if (lastPlayerGridPosRequestTimeStamp < Time.time)
            {
                lastPlayerGridPosRequestTimeStamp = Time.time;
                cachedPlayerGridPos = GetGridPosition(GetPlayerWorldPosition(), out _);
            }
            return cachedPlayerGridPos;
        }

        public Vector3 GetPlayerWorldPosition()
        {
            if (lastPlayerPosRequestTimeStamp < Time.time)
            {
                lastPlayerPosRequestTimeStamp = Time.time;
                cachedPlayerPos = player.transform.position;
            }
            return cachedPlayerPos;
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

        void InitCurrentMap()
        {
            foreach (var mapEntry in allMapsInScene)
            {
                if (mapEntry.IsWorldPosInsideMap(player.transform.position, out var gridPos))
                {
                    mapEntry.gameObject.SetActive(true);
                    SetCurrentMap(mapEntry);
                }
                else
                {
                    mapEntry.gameObject.SetActive(false);
                }
            }

            currentMap.ToggleAdjacentMaps(true);
        }

        void UpdateCurrentMap()
        {
            if (!currentMap.IsWorldPosInsideMap(player.transform.position, out _))
            {
                foreach (var mapEntry in currentMap.adjacentMaps)
                {
                    if (mapEntry.map.IsWorldPosInsideMap(player.transform.position, out _))
                    {
                        currentMap.ToggleAdjacentMaps(false);
                        SetCurrentMap(mapEntry.map);
                    }
                }
            }
        }

        void SetCurrentMap(MapEntry nextMap)
        {
            currentMap = nextMap;
            currentMap.gameObject.SetActive(true);
            currentMap.ToggleAdjacentMaps(true);

            if (fadeInComplete)
            {
                if (!currentMap.IsVisited && currentMap.TryGetAnnouncementText(out var text))
                {
                    BigBadSingleton.Instance.UIManager.Announce(text, 2f, currentMap.Visit);
                }
                else
                {
                    currentMap.Visit();
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
            enemyType = EnemyType.Crab;

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

            BigBadSingleton.Instance.UIManager.RoomCompletionInfo.ShowType(UIRoomCompletionInfo.InfoType.None);

            Time.timeScale = 0;
            fadeInComplete = false;

            string roomText;
            if (currentMap.TryGetAnnouncementText(out roomText))
            {
                BigBadSingleton.Instance.UIManager.SetAnnounceText(roomText);
            }

            BigBadSingleton.Instance.UIManager.FadeScreen.DoFadeIn(1f, 0.4f, () =>
            {
                BigBadSingleton.Instance.UIManager.Announce(roomText, 1f, () =>
                {
                    currentMap.Visit();
                });
                fadeInComplete = true;
            });
        }

        public T SpawnTemporaryObject<T>(T prefab, Vector3 origin) where T : MonoBehaviour
        {
            return Instantiate<T>(prefab, origin, Quaternion.identity, temporaryObjectsContainer);
        }

        public GameObject SpawnEnemy(EnemyType type, Vector3 worldPos, Transform parent = null)
        {
            var newEnemy = enemyPalette.GetEnemyPrefab(type);
            if (parent == null)
            {
                parent = enemiesContainer;
            }
            var instance = Instantiate(newEnemy, worldPos, Quaternion.identity, enemiesContainer);
            return instance;
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
            
            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                if (currentMap.TryGetRandomSpawnPosition(out var position))
                {
                    SpawnEnemy(EnemyType.Crab, position, enemiesContainer);
                }
            }

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

        public void DoSlowmoFX(float delay, float duration)
        {
            slowMoFX.StartDelay(Time.timeScale, delay, duration, 1);
        }

        public void DoCameraShake()
        {
            cameraController.DoShake();
        }
    }
}