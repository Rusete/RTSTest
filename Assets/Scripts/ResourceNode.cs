using DRC.RTS.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum EResourceType
{
    Ore
}
public class ResourceNode : MonoBehaviour
{
    public EResourceType type;
    public int quantity;

    //events
    public UnityEvent onQuantityChange;

    public void GatherResource(int amount, IUnit gatheringUnit, out int gatheredResources)
    {
        quantity -= amount;

        int amountToGive = amount;
        
        if(quantity < 0)
        {
            amountToGive = amount + quantity;
        }
        gatheredResources = amountToGive;
        if(quantity <= 0)
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
