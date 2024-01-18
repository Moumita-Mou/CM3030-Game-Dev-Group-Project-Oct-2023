using UnityEngine;

namespace Scripts.Weapons
{
    public enum WeaponType
    {
        None = 0,
        Sword = 1,
        Shield = 2,
    }
    
    public class WeaponBase : MonoBehaviour
    {
        [SerializeField] private WeaponType type;
        [SerializeField] private int damage;
        [SerializeField] private float attackSpeed = 1;
        [SerializeField] private float attackRate = 0.2f;

        private float lastFireTimeStamp = 0;

        public virtual bool CanFire()
        {
            return Time.time > lastFireTimeStamp;
        }

        public virtual bool Fire()
        {
            lastFireTimeStamp += attackRate;
            return true;
        }

        public virtual float GetAttackSpeed()
        {
            return attackSpeed;
        }
    }
}