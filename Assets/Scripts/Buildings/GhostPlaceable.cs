using DRC.RTS.Interactables;
using DRC.RTS.Player;
using UnityEngine;
using UnityEngine.AI;

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
        print(placeable);
        if (!placeable) return placeable;
        transform.parent = PlayerManager.instance.playerBuildings;
        transform.GetComponent<NavMeshObstacle>().enabled = true;
        transform.GetComponent<Collider>().isTrigger = false;
        transform.GetComponent<IBuilding>().enabled = true;
        enabled = false;
        StartCoroutine(GetComponent<IBuilding>().Construct());
        return placeable;
    }

    bool IsPositionOccupied(Vector3 position, Bounds bounds)
    {
        // Check if there is an object within the bounds at the specified position
        Collider[] colliders = Physics.OverlapBox(position, bounds.extents, Quaternion.identity);
        return colliders.Length > 2;
    }
}