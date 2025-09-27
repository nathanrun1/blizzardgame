using Blizzard.Obstacles;
using Blizzard.Player;
using DG.Tweening;
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
        [Header("Behavior Config")]
        [SerializeField] float _baseSwingCooldown = 0.5f;
        [Header("Style Config")]
        [SerializeField] float _animDuration = 0.15f;
        [SerializeField] float _swingAnimOffset = 0.35f;
        [SerializeField] float _swingAnimStartLocalX = 0.35f;
        [SerializeField] float _swingAnimEndLocalX = -0.25f;
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

            _axeStationary.SetActive(true);
            _axeSwing.SetActive(false);
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
            Debug.Log("Axe input detected");
            if (!_inputService.IsPointerOverUIElement() && _cooldown <= 0) OnSwing();
        }

        /// <summary>
        /// Invoked when the player sends input to swing the axe
        /// </summary>
        private void OnSwing()
        {
            _cooldown = CalculateCooldown();
            StartCoroutine(AxeSwingAnim());
            Collider2D[] hitObjects = AxeDetectHit();

            foreach (Collider2D obj in hitObjects)
            {
                // Get 'Harvestable' component of object, or ignore if doesn't exist

                if (obj.TryGetComponent<Harvestable>(out Harvestable harvestable))
                {
                    Harvest(harvestable, CalculateDamage());
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
            
            Collider2D[] hitList = Physics2D.OverlapAreaAll(bottomLeft, topRight, layerMask: (int)CollisionAssistant.Hittable);

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

        private IEnumerator AxeSwingAnim()
        {
            _axeStationary.SetActive(false);
            _axeSwing.SetActive(true);

            Vector3 swingStartPos = new Vector3(_swingAnimStartLocalX, _swingAnimOffset, 0f);
            Vector3 swingEndPos = new Vector3(_swingAnimEndLocalX, _swingAnimOffset, 0f);

            Quaternion swingStartRot = Quaternion.Euler(new Vector3(0, 0, 0));
            Quaternion swingEndRot = Quaternion.Euler(new Vector3(0, 0, 90));

            _axeSwing.transform.localPosition = swingStartPos;
            _axeSwing.transform.localRotation = swingStartRot;


            Sequence sequence = DOTween.Sequence();
            sequence.Append(_axeSwing.transform.DOLocalRotateQuaternion(swingEndRot, _animDuration));
            sequence.Join(_axeSwing.transform.DOLocalMove(swingEndPos, _animDuration));

            sequence.OnComplete(() =>
            {
                _axeStationary.SetActive(true);
                _axeSwing.SetActive(false);
            });
            sequence.Play();
            yield return null;
        }
    }
}
