using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Health
{
    public class EntityHealth : MonoBehaviour
    {
        public enum EntityTypes { Player, Ally, Enemy }
        public EntityTypes healthType = EntityTypes.Enemy;

        [Header("Health")]
        [SerializeField] float maxHealth = 10f;
        [SerializeField] float currentHealth = 0f;
        [SerializeField] float resistance = 1f;
        [Header("Damage")]
        public float damage;

        void Start()
        {
            currentHealth = maxHealth;
        }
        void Update()
        {
            CheckDeath();
            if (transform.parent == null)
            {
                switch (healthType)
                {
                    case EntityTypes.Enemy:
                        findParent(TurnBasedManager.enemyParentName);
                        break;
                    case EntityTypes.Player:
                    case EntityTypes.Ally:
                        findParent(TurnBasedManager.allyParentName);
                        break;
                    default:
                        break;
                }
            }
        }
        void findParent(string parentName)
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent == null)
            {
                return;
            }
            transform.parent = parent.transform;
        }
        public void DealDamage(float damage)
        {
            currentHealth = Mathf.Max(currentHealth - (damage / resistance), 0);
        }
        void CheckDeath()
        {
            if (currentHealth == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
