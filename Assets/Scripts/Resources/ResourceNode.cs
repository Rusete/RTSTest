using DRC.RTS.Interactables;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class ResourceNode : MonoBehaviour
{
    public GameResources.EResourceType resourceType;
    public int resourceAmount;
    private int resourceAmountMax;
    private Transform resourceNodeTransform;
    //events
    public UnityEvent onQuantityChange; private 
    const RegenerationMethod regenerationMethod = RegenerationMethod.None;

    private enum RegenerationMethod
    {
        Constant,
        TimedFull,
        None
    }

    public Vector3 GetPosition()
    {
        return resourceNodeTransform.position;
    }

    public ResourceNode(Transform resourceNodeTransform, GameResources.EResourceType resourceType)
    {
        this.resourceNodeTransform = resourceNodeTransform;
        this.resourceType = resourceType;
        resourceAmountMax = 3;
        resourceAmount = resourceAmountMax;
    }

    public GameResources.EResourceType GrabResource()
    {
        resourceAmount -= 1;
        if (resourceAmount <= 0)
        {
            // Node is depleted
            //UpdateSprite();
            switch (regenerationMethod)
            {
                case RegenerationMethod.Constant:
                    // Crear función de Regeneración de Nodo
                    break;
                    case RegenerationMethod.TimedFull:
                    StartCoroutine(Regenerate());
                    break;
                    case RegenerationMethod.None:
                    // No se regenera
                    break;
            }
        }
        return resourceType;
    }
    IEnumerator Regenerate()
    {
        yield return new WaitForSeconds(resourceAmount);
        resourceAmount = resourceAmountMax;
    }
    public void GatherResource(int amount, IUnit gatheringUnit, out int gatheredResources)
    {
        resourceAmount -= amount;

        int amountToGive = amount;
        
        if(resourceAmount < 0)
        {
            amountToGive = amount + resourceAmount;
        }
        gatheredResources = amountToGive;
        if(resourceAmount <= 0)
        {
            Destroy(gameObject);
        }
        if(onQuantityChange != null)
        {
            onQuantityChange.Invoke();
        }
    }
    public void Gather()
    {
        print("TO DO: Actualizar el display de recursos para que el jugador sepa cuántos recursos quedan en el nodo. Además se puede plantear un cambio de sprite para que visualmente se vea cómo reduce la cantidad de recurso");
    }
}
