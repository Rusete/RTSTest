using DRC.RPG.Utils;
using DRC.RTS.Interactables;
using DRC.RTS.Player;
using UnityEngine;
using UnityEngine.AI;

namespace DRC.RTS.Buildings
{
    public class GhostPlaceable : MonoBehaviour
    {
        [SerializeField] private Material placeableMaterial;
        [SerializeField] private Material unplaceableMaterial;
        Bounds bounds;
        [SerializeField] private LayerMask hitLayer;
        [SerializeField] MeshRenderer meshRenderer;
        public GameObject buildingPrefab;
        private bool placeable;

        private void Awake()
        {
            bounds = GetComponent<Collider>().bounds;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = placeableMaterial;
        }

        public void UpdateGhostStatus(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100000, hitLayer))
            {
                transform.position = hit.point + (bounds.size.y / 2) * Vector3.up;
                placeable = !IsPositionOccupied(hit.point, bounds);
                if (placeable && meshRenderer.material != unplaceableMaterial)
                {
                    meshRenderer.material = placeableMaterial;
                }
                else if (!placeable && meshRenderer.material != placeableMaterial)
                {
                    meshRenderer.material = unplaceableMaterial;
                }
            }
        }

        public GameObject Place()
        {
            if (!placeable) return null;
            var building = ObjectPoolManager.SpawnObject(buildingPrefab, transform.position, transform.rotation, ObjectPoolManager.PoolType.Building);
            building.GetComponent<SelectableBehaviour>().ShowHpbar();
            building.GetComponent<IBuilding>().baseStats = Buildings.BuildingsHandler.instance.GetBasicBuildingStats(buildingPrefab.name);
            building.GetComponent<Health>().SetupHealth(true);
            building.transform.parent = PlayerManager.instance.playerBuildings.Find(buildingPrefab.name + "s");
            return building;
        }

        bool IsPositionOccupied(Vector3 position, Bounds bounds)
        {
            // Check if there is an object within the bounds at the specified position
            Collider[] colliders = Physics.OverlapBox(position, bounds.extents, Quaternion.identity);
            return colliders.Length > 2;
        }
    }

}