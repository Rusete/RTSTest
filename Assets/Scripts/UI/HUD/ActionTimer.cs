using DRC.RTS.Buildings.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DRC.RTS.UI.HUD
{
    public class ActionTimer : MonoBehaviour
    {
        private PlayerBuilding building;

        private void Awake()
        {
            building = GetComponent<PlayerBuilding>();
        }

        public IEnumerator SpawnQeueTimer()
        {
            if(building.spawnQueue.Count > 0)
            {
                Debug.Log($"Waiting for {building.spawnQueue[0]}");

                yield return new WaitForSeconds(building.spawnQueue[0]);

                building.SpawnObject();
                building.spawnQueue.RemoveAt(0);

                if (building.spawnQueue.Count > 0)
                {
                    StartCoroutine(SpawnQeueTimer());
                }
            }
        }
    }
}