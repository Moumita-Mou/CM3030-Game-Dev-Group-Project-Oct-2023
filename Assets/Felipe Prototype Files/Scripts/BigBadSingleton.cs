using System;
using Scripts.Map;
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
            gamePlayManager.LoadSceneMaps(allMapsInScene);
        }

        private void Reset()
        {
            gamePlayManager = GetComponent<GameplayManager>();
        }
    }
}