using System;
using Scripts.Character;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterVisual visual;
        [SerializeField] private Collider2D collider;
        [SerializeField] private Rigidbody2D rigidbody2D;

        [Header("Gameplay Settings")]
        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private int totalLife = 3;
        [SerializeField] private float hitImpulseForce = 10f;
        [SerializeField] private LayerMask hitMask;

        [SerializeField] private int mainWeaponMouseButton = 0;
        [SerializeField] private int secondaryWeaponMouseButton = 1;
        
        [Header("Events")]
        [SerializeField] UnityEvent OnHit;
        [SerializeField] UnityEvent OnDeath;

        private bool isWalking;

        public int TotalLife => totalLife;

        public int CurrentLife { get; private set; }

        public void Init()
        {
            CurrentLife = TotalLife;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (hitMask == (hitMask | 1 << col.gameObject.layer))
            {
                rigidbody2D.AddForce(col.relativeVelocity * hitImpulseForce, ForceMode2D.Impulse);
                CurrentLife -= 1;
                OnHit?.Invoke();
            }

            // Play game over sound when player dies
            if (CurrentLife == 0) 
            {
                OnDeath?.Invoke();
                Time.timeScale = 0.0f;
            }
        }

        void FixedUpdate()
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            Vector2 walkForce = new Vector2(inputX, inputY).normalized * moveSpeed;

            isWalking = walkForce.sqrMagnitude > 0;
            
            rigidbody2D.AddRelativeForce(walkForce, ForceMode2D.Impulse);
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;

            //BigBadSingleton.Instance.GameplayManager.Debug_FocusWorldPositionInGrid(transform.position, false);
            
            visual.UpdateWalking(isWalking, rigidbody2D.velocity.magnitude);

            var currentMousePos = Input.mousePosition;
            var playerPosTransformed = Camera.main.WorldToScreenPoint(visual.transform.position);
            var dir = (currentMousePos - playerPosTransformed).normalized;
            visual.UpdateOrientation(dir);
            
            visual.UpdateHands(isWalking, 
                Input.GetMouseButton(secondaryWeaponMouseButton), 
                Input.GetMouseButton(mainWeaponMouseButton));
        }
    }
}