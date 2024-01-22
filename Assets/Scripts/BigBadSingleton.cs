using System;
using System.Linq;
using Scripts.Map;
using Scripts.Player;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(GameplayManager))]
    public class BigBadSingleton : MonoBehaviour
    {
        [SerializeField] private GameplayManager gamePlayManager;
        
        public static BigBadSingleton Instance { get; private set; }
        public GameplayManager GameplayManager => gamePlayManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var allMapsInScene = FindObjectsOfType<MapEntry>();
            var player = FindObjectsOfType<PlayerController>().FirstOrDefault();
            gamePlayManager.LoadSceneMaps(allMapsInScene);
            gamePlayManager.LoadPlayer(player);
        }

        private void Reset()
        {
            gamePlayManager = GetComponent<GameplayManager>();
        }
    }
}