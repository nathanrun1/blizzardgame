using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Blizzard.Utilities // TODO: Separate stats counter namespace or something?
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _fpsText;
        [SerializeField] private float _updateDelay = 2f;

        private float _timeSinceUpdate = 0.0f;
        private int _framesSinceLastUpdate = 0;

        // Update is called once per frame
        void Update()
        {
            _framesSinceLastUpdate++;
            _timeSinceUpdate += Time.unscaledDeltaTime;
            if (_timeSinceUpdate > _updateDelay)
            {
                int fps = Mathf.RoundToInt(_framesSinceLastUpdate / _updateDelay);
                _framesSinceLastUpdate = 0;
                _timeSinceUpdate -= _updateDelay;
                _fpsText.text = $"FPS: {fps}";
            }
        }
    }
}
