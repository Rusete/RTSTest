using DRC.RPG.Utils;
using DRC.RTS.Units.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DRC.RTS.Interactables
{
    [System.Serializable]
    public class Health : MonoBehaviour
    {
        public float maxHealth, armor, currentHealth;
        public bool dead;
        [SerializeField] private HealthBar hpBar;

        public void SetupHealth(bool startingHealth = false)
        {
            if (gameObject.GetComponent<IUnit>())
            {
                maxHealth = gameObject.GetComponent<IUnit>().baseStats.health;
                armor = gameObject.GetComponent<IUnit>().baseStats.armor;
            }
            else if (gameObject.GetComponent<IBuilding>())
            {
                maxHealth = gameObject.GetComponent<IBuilding>().baseStats.health;
                armor = gameObject.GetComponent<IBuilding>().baseStats.armor;
            }
            if (startingHealth)
                currentHealth = 1;
            else
                currentHealth = maxHealth;

            hpBar.UpdateHealthBar(maxHealth, currentHealth, true);
        }

        public virtual void SetDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            GetComponent<Interactable>().ShowHpbar();
            hpBar.UpdateHealthBar(maxHealth, currentHealth);
            if (currentHealth == 0)
            {
                Die();
            }
        }

        public void SetHealing(float heal)
        {
            currentHealth += heal;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            hpBar.UpdateHealthBar(maxHealth, currentHealth);
            if(currentHealth == maxHealth)
                GetComponent<Interactable>().HideHpBar();
        }
        private void Die()
        {
            if (GetComponentInParent<PlayerUnit>())
            {
                InputManager.InputHandler.instance.selectedUnits.Remove(transform);
                //Hacer lo del Pool para despawnear
                ObjectPoolManager.ReturnObjectToPool(gameObject);
            }
            else
            {
                //hacer lo del Pool para despawnear
                ObjectPoolManager.ReturnObjectToPool(gameObject);
            }
        }
    }
}
