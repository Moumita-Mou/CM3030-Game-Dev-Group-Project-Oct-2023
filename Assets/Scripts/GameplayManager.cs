﻿using System;
using Scripts.Map;
using UnityEngine;

namespace Scripts
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private Transform debugGridPositionImage;
        
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
    }
}