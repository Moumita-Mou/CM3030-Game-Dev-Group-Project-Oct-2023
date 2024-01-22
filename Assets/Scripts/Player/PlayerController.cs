﻿using Scripts.Character;
using UnityEngine;

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

        [SerializeField] private int mainWeaponMouseButton = 0;
        [SerializeField] private int secondaryWeaponMouseButton = 1;

        private bool isWalking;
        private GameObject holdingItem = null;
        private GameObject canInteract = null;

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

            BigBadSingleton.Instance.GameplayManager.Debug_FocusWorldPositionInGrid(transform.position, true);
            
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

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("PickUp"))
            {
                canInteract = collider.gameObject;
            }

        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.CompareTag("PickUp"))
            {
                canInteract = null;
            }
        }
    }
}