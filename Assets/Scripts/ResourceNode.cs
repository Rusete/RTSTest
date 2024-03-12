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

    public void GatherResource(int amount, IUnit gatheringUnit)
    {
        quantity -= amount;

        int amountToGive = amount;
        
        if(quantity < 0)
        {
            amountToGive = amount + quantity;
        }

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
        print("pierdo cantidad de material");
    }
}
