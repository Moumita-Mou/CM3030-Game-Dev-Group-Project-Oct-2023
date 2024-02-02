using UnityEngine;
using Scripts.Weapons;
using UnityEngine.Events;

namespace Scripts.Character
{
    public class CharacterHand : MonoBehaviour
    {
        [SerializeField] private WeaponBase weapon;
        [SerializeField] private Animator handAnimation;
        [SerializeField] private Transform handPivot;

        [Header("Events")]
        [SerializeField] UnityEvent PlaySound;

        public void UpdateHand(bool isWalking, bool isAttacking)
        {
            if (isAttacking && weapon.CanFire())
            {
                weapon.Fire();
                handAnimation.SetFloat("AttackSpeed", weapon.GetAttackSpeed());
                handAnimation.SetTrigger("Attack");
                PlaySound?.Invoke();
            }
            
            handAnimation.SetBool("IsWalking", isWalking);
        }
    }
}