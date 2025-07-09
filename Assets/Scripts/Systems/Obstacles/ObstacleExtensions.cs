using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using ModestTree;
using Zenject;

namespace Blizzard.Obstacles
{
    [BurstCompile]
    public static class ObstacleExtensions
    {
        [Inject] static DiContainer _diContainer;

        /// <summary>
        /// Generates a "preview" gameobject of this obstacle.
        /// </summary>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        public static GameObject CreatePreview(this Obstacle obstacle)
        {
            GameObject preview = MonoBehaviour.Instantiate(obstacle).gameObject;

            
            // Set SpriteRenderer color alphas so that preview is transparent
            foreach (SpriteRenderer spriteRenderer in preview.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.color *= new Color(1, 1, 1, ObstacleConstants.PreviewAlpha);
            }

            // Remove all scripts, rigidbodies, colliders (no physics or other interactions wanted)
            foreach (Component component in preview.GetComponentsInChildren(typeof(Component)))
            {
                MonoBehaviour mb = component as MonoBehaviour;
                if (mb != null)
                {
                    mb.enabled = false;
                    continue;
                }

                Collider2D collider = component as Collider2D;
                if (collider != null)
                {
                    collider.enabled = false;
                    continue;
                }

                Rigidbody2D rb = component as Rigidbody2D;
                if (rb != null)
                {
                    MonoBehaviour.Destroy(rb);
                    continue;
                }
            }

            return preview;
        }
    }
}
