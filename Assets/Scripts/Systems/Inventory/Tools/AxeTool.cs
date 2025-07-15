using Blizzard.Obstacles;
using Blizzard.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Blizzard
{
    public class AxeTool : ToolBehaviour
    {
        [Header("GameObjects")]
        [SerializeField] GameObject _axeHitbox;
        [SerializeField] GameObject _axeStationary;
        [SerializeField] GameObject _axeSwing;
        [Header("Axe Config")]
        [SerializeField] float _baseSwingCooldown = 0.5f;
        [Header("Testing")]
        [SerializeField] bool _visualizeHitbox = false;

        [Inject] InputService _inputService;

        private LineRenderer _hitboxLineRenderer;

        private float _cooldown;

        private void Awake()
        {
            if (_visualizeHitbox)
            {
                InitAxeHitboxVisualizer();
            }

            BindInput();
        }

        private void Update()
        {
            if (_cooldown > 0) _cooldown -= Time.deltaTime;
        }

        private void OnDestroy()
        {
            UnbindInput();
        }

        private void BindInput()
        {
            _inputService.inputActions.Player.Fire.performed += OnInputFire;
        }

        private void UnbindInput()
        {
            _inputService.inputActions.Player.Fire.performed -= OnInputFire;
        }

        private void OnInputFire(InputAction.CallbackContext ctx)
        {
            if (!_inputService.IsPointerOverUIElement() && _cooldown <= 0) OnSwing();
        }

        /// <summary>
        /// Invoked when the player sends input to swing the axe
        /// </summary>
        private void OnSwing()
        {
            _cooldown = CalculateCooldown();
            Collider2D[] hitObjects = AxeDetectHit();

            foreach (Collider2D obj in hitObjects)
            {
                // Get 'Harvestable' component of object, or ignore if doesn't exist
                Harvestable harvestable = obj.GetComponent<Harvestable>();
                if (harvestable != null)
                {
                    // Target is a harvestable
                    Harvest(harvestable, CalculateDamage());

                    Debug.Log($"Hit a {harvestable.name}! Applying damage.");
                }

                // TODO: handle enemies
            }
        }

        private int CalculateDamage()
        {
            // TODO: maybe base calculation on temperature? or maybe like a constant that affects some shit
            return _baseDamage;
        }

        private float CalculateCooldown()
        {
            // TODO: depend cooldown calc on temperature probably
            return _baseSwingCooldown;
        }

        /// <summary>
        /// Runs collision detection for axe on 'Damageable' layer. Returns all hit gameobjects
        /// </summary>
        private Collider2D[] AxeDetectHit()
        {
            // Vector from center of hitbox to its "right" edge
            Vector3 halfX = new Vector3(
                    (_axeHitbox.transform.lossyScale.x / 2) * Mathf.Cos(_axeHitbox.transform.eulerAngles.z * Mathf.Deg2Rad),
                    (_axeHitbox.transform.lossyScale.x / 2) * Mathf.Sin(_axeHitbox.transform.eulerAngles.z * Mathf.Deg2Rad),
                    0
                    );

            // Vector from center of hitbox to its "top" edge
            Vector3 halfY = new Vector3(
                        -(_axeHitbox.transform.lossyScale.y / 2) * Mathf.Sin(_axeHitbox.transform.eulerAngles.z * Mathf.Deg2Rad),
                        (_axeHitbox.transform.lossyScale.y / 2) * Mathf.Cos(_axeHitbox.transform.eulerAngles.z * Mathf.Deg2Rad),
                        0
                    );

            // Bottom Left = center of hitbox + Vector to 
            Vector2 bottomLeft = _axeHitbox.transform.position - halfX - halfY;
            Vector2 topRight = _axeHitbox.transform.position + halfX + halfY;
            
            Collider2D[] hitList = Physics2D.OverlapAreaAll(bottomLeft, topRight, layerMask: (int)CollisionLayer.Destroyable);

            if (_visualizeHitbox) StartCoroutine(VisualizeAxeHitbox(bottomLeft, topRight));

            return hitList;
        }

        private void InitAxeHitboxVisualizer()
        {
            _hitboxLineRenderer = gameObject.AddComponent<LineRenderer>();
            _hitboxLineRenderer.enabled = false;
            _hitboxLineRenderer.startWidth = 0.1f;
            _hitboxLineRenderer.endWidth = 0.1f;
            _hitboxLineRenderer.positionCount = 2;
        }

        private IEnumerator VisualizeAxeHitbox(Vector3 cornerA, Vector3 cornerB)
        {
            _axeHitbox.GetComponent<SpriteRenderer>().enabled = true;
            _hitboxLineRenderer.enabled = true;
            _hitboxLineRenderer.SetPositions(new Vector3[] { cornerA, cornerB });
            yield return new WaitForSeconds(0.3f);
            _axeHitbox.GetComponent<SpriteRenderer>().enabled = false; // temp
            _hitboxLineRenderer.enabled = false;
        }
    }
}
