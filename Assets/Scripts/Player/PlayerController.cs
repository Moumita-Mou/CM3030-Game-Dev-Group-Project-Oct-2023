﻿using System;
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
        [SerializeField] private float pickupOffset = 0.15f;
        [SerializeField] private int totalLife = 3;
        [SerializeField] private float hitImpulseForce = 10f;
        [SerializeField] private LayerMask hitMask;

        [SerializeField] private int mainWeaponMouseButton = 0;
        [SerializeField] private int secondaryWeaponMouseButton = 1;

        [Header("Events")]
        [SerializeField] UnityEvent OnHit;
        [SerializeField] UnityEvent OnDeath;

        public Rigidbody2D Rigidbody => rigidbody2D;

        private bool isWalking;
        private GameObject holdingItem = null;
        private GameObject canInteract = null;

        private bool hasKey = false; // Tracks if the player has picked up a key

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
                BigBadSingleton.Instance.GameplayManager.DoSlowmoFX(0.1f, 0.0f);
                rigidbody2D.AddForce(col.relativeVelocity * hitImpulseForce, ForceMode2D.Impulse);
                CurrentLife -= 1;

                BigBadSingleton.Instance.GameplayManager.DoCameraShake();

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

            applyInteractionLogic();
        }

        private void applyInteractionLogic()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canInteract != null && holdingItem == null)
                {
                    if (canInteract.CompareTag("PickUp"))
                    {
                        holdingItem = canInteract;
                        canInteract = null;
                    }
                    else if (canInteract.CompareTag("Lever"))
                    {
                        canInteract.GetComponent<Lever>().changeState();
                    }
                }
                else if (holdingItem != null)
                {
                    holdingItem.transform.position = BigBadSingleton.Instance.GameplayManager.getGridCenterInWorldPos(transform.position);
                    holdingItem = null;
                }
            }

            if (holdingItem != null)
            {
                holdingItem.transform.position = new Vector2(transform.position.x, transform.position.y + pickupOffset);
            }
        }

        // Updated OnTriggerEnter2D to account for key pickup 
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("PickUp") || collider.CompareTag("Lever") || collider.CompareTag("Key"))
            {
                canInteract = collider.gameObject;

                // Key pickup logic
                if (collider.CompareTag("Key"))
                {
                    hasKey = true; // Player has picked up the key
                    Destroy(collider.gameObject); // Remove the key from the scene
                }
            }
        }

        // Logic to use key on a door 
        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.CompareTag("Door") && hasKey && Input.GetKeyDown(KeyCode.E))
            {
                OpenDoor(collider.gameObject); // Calls open door method
                hasKey = false; // Consume the key upon use
            }
        }

        // OpenDoor method
        private void OpenDoor(GameObject door)
        {
            // Disables the door collider
            door.GetComponent<Collider2D>().enabled = false;
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.CompareTag("PickUp") || collider.CompareTag("Lever"))
            {
                canInteract = null;
            }
        }
    }
}