using DRC.RTS.Units.Player;
using UnityEngine;
using UnityEngine.UI;
using DRC.RTS.Interactables;

namespace DRC.RTS.Units
{
    [System.Serializable]
    public class Health : MonoBehaviour
    {
        public float maxHealth, armor, currentHealth;
        public bool dead;
        [SerializeField] private HealthBar hpBar;

        public void SetupHealth()
        {
            maxHealth = gameObject.GetComponent<IUnit>().baseStats.health;
            armor = gameObject.GetComponent<IUnit>().baseStats.armor;
            currentHealth = maxHealth;
        }

        public virtual void SetDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            hpBar.UpdateHealthBar(maxHealth, currentHealth);
            if (currentHealth == 0)
            {
                Die();
            }
        }

        public void SetHealing(float heal)
        {
            currentHealth += heal;
            Mathf.Clamp(currentHealth, 0, maxHealth);
            hpBar.UpdateHealthBar(maxHealth, currentHealth);
        }
        private void Die()
        {
            if (GetComponentInParent<PlayerUnit>())
            {
                InputManager.InputHandler.instance.selectedUnits.Remove(transform);
                //Hacer lo del Pool para despawnear
                Destroy(gameObject);
            }
            else
            {
                //hacer lo del Pool para despawnear
                Destroy(gameObject);
            }
        }
    }
}
