using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageColorFX : MonoBehaviour
    {
        [SerializeField] Color color;
        [SerializeField] float duration;

        Color originalColor;
        SpriteRenderer renderer;
        readonly List<Coroutine> coroutines = new List<Coroutine>();
    
        void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
            originalColor = renderer.color;
        }
    
        public void Flash()
        {
            coroutines.Add(StartCoroutine(FlashRoutine()));
        }
    
        private IEnumerator FlashRoutine()
        {
            renderer.color = color;
        
            yield return new WaitForSeconds(duration);
        
            renderer.color = originalColor;
        
            coroutines.RemoveAt(coroutines.Count-1);
        }

        private void OnDisable()
        {
            foreach(var routine in coroutines)
            {
                StopCoroutine(routine);
            }
            coroutines.Clear();
        }
    }
}