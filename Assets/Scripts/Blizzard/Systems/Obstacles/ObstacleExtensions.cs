using Blizzard.Constants;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UIElements;
using ModestTree;
using Zenject;

namespace Blizzard.Obstacles
{
    [BurstCompile]
    public static class ObstacleExtensions
    {
        [Inject] private static DiContainer _diContainer;

        /// <summary>
        /// Generates a "preview" gameobject of this obstacle.
        /// </summary>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        public static GameObject CreatePreview(this Obstacle obstacle)
        {
            var preview = MonoBehaviour.Instantiate(obstacle).gameObject;


            // Set SpriteRenderer color alphas so that preview is transparent
            foreach (var spriteRenderer in preview.GetComponentsInChildren<SpriteRenderer>())
                spriteRenderer.color *= new Color(1, 1, 1, ObstacleConstants.PreviewAlpha);

            // Remove all scripts, rigidbodies, colliders (no physics or other interactions wanted)
            foreach (var component in preview.GetComponentsInChildren(typeof(Component)))
            {
                var mb = component as MonoBehaviour;
                if (mb != null)
                {
                    mb.enabled = false;
                    continue;
                }

                var collider = component as Collider2D;
                if (collider != null)
                {
                    collider.enabled = false;
                    continue;
                }

                var rb = component as Rigidbody2D;
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