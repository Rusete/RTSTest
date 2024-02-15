using DRC.RTS.Interactables;
using DRC.RTS.Player;
using UnityEngine;
using UnityEngine.AI;

namespace DRC.RTS.Buildings
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class GhostPlaceable : MonoBehaviour
    {
        [SerializeField] private Material placeableMaterial;
        [SerializeField] private Material unplaceableMaterial;
        Bounds bounds;
        [SerializeField] private LayerMask hitLayer;
        [SerializeField] MeshRenderer meshRenderer;
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

        public bool Place()
        {
            if (!placeable) return placeable;
            transform.GetComponent<NavMeshObstacle>().enabled = true;
            transform.GetComponent<Collider>().isTrigger = false;
            transform.GetComponent<Interactable>().ShowHpbar();
            var building = transform.GetComponent<IBuilding>();
            building.enabled = true;
            building.baseStats = Buildings.BuildingsHandler.instance.GetBasicBuildingStats(name);
            GetComponent<Health>().SetupHealth(true);
            transform.parent = PlayerManager.instance.playerBuildings.Find(name + "s");

            return placeable;
        }

        bool IsPositionOccupied(Vector3 position, Bounds bounds)
        {
            // Check if there is an object within the bounds at the specified position
            Collider[] colliders = Physics.OverlapBox(position, bounds.extents, Quaternion.identity);
            return colliders.Length > 2;
        }
    }

}