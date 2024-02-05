using System;
using System.Linq;
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
        [SerializeField] private SlowMoFX slowMoFX;
        [SerializeField] private ProtoCameraController cameraController;
        [SerializeField] private PlayerController playerPrefab;
        
        [Header("Containers")]
        [SerializeField] private Transform fxContainer;
        [SerializeField] private Transform enemiesContainer;
        [SerializeField] private Transform temporaryObjectsContainer;

        [Header("Settings")] 
        public bool DoSlowMoFx = true;

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

        void Start()
        {
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
            UpdateCurrentMap();
            
            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                if (currentMap.TryGetRandomSpawnPosition(out var position))
                {
                    SpawnEnemy(EnemyType.Crab, position, enemiesContainer);
                }
            }
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