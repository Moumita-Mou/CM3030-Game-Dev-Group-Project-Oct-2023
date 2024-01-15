using UnityEngine;
using Scripts.Weapons;

namespace Scripts.Character
{
    public class CharacterHand : MonoBehaviour
    {
        [SerializeField] private WeaponBase weapon;
        [SerializeField] private Animator handAnimation;
        [SerializeField] private Transform handPivot;

        public void UpdateHand(bool isWalking, bool isAttacking)
        {
            if (isAttacking && weapon.CanFire())
            {
                weapon.Fire();
                handAnimation.SetFloat("AttackSpeed", weapon.GetAttackSpeed());
                handAnimation.SetTrigger("Attack");
            }
            
            handAnimation.SetBool("IsWalking", isWalking);
        }
    }
}