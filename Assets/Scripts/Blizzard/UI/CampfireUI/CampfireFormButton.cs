using UnityEngine;
using UnityEngine.UI;

namespace Blizzard.UI.CampfireUI
{
    public class CampfireFormButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _buttonBg;
        [Header("Config")]
        [SerializeField] private Color _selectedTint;

        public Button button;
        
        /// <summary>
        /// Indicates this campfire form as active
        /// </summary>
        /// <param name="active"></param>
        [Sirenix.OdinInspector.Button]
        public void SetFormActive(bool active)
        {
            _buttonBg.color = active ? _selectedTint : Color.white;
        }
    }
}