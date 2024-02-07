using DRC.RTS.Buildings;
using DRC.RTS.Buildings.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Interactables
{
    public class IBuilding : Interactable
    {
        public UI.HUD.PlayerActions actions;
        public GameObject spawnMarker = null;
        public GameObject spawnMarkerGraphics = null;
        public float maxMarkerDistance = 10f;


        public BuildingStatTypes.Base baseStats;
        public BasicBuilding buildingType;

        private void Start()
        {
            baseStats = buildingType.baseStats;
        }
        public override void OnInteractEnter()
        {
            InputManager.InputHandler.instance.selectedBuilding.GetComponent<PlayerBuilding>().SetSpawnLocation(spawnMarker);
            spawnMarkerGraphics.SetActive(true);
            base.OnInteractEnter();
        }

        public override void OnInteractExit()
        {
            UI.HUD.ActionFrame.instance.ClearActions();
            spawnMarkerGraphics.SetActive(false);
            base.OnInteractExit();
        }

        public void SetSpawnMarkerLocation(Vector3 point)
        {
            spawnMarker.transform.position = point;
        }
    }
}
