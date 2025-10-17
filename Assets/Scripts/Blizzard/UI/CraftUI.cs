using System.Collections.Generic;
using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Blizzard.UI
{
    public class CraftUI : UIBase
    {
        [Header("References")]
        [SerializeField] CraftingDatabase _craftingDatabase;

        // -- Categories panel --
        [SerializeField] GameObject _categoriesPanel;
        [SerializeField] Transform _categoriesButtonParent;

        // -- Recipes panel (per category) --
        [SerializeField] GameObject _recipeListPanel;
        [SerializeField] Transform _recipeListButtonParent;

        // -- Recipe panel -- 
        [SerializeField] GameObject _recipePanel;
        [SerializeField] Button _craftButton;

        [Header("Prefabs")]
        [SerializeField] Button _categoryButtonPrefab;
        [SerializeField] Button _recipeButtonPrefab;

        [Inject] InventoryService _inventoryService;
        [Inject] UIService _uiService;
        [Inject] EnvPrefabService _envPrefabService;
        [Inject] PlayerService _playerService;

        private List<Button> _activeRecipeButtons = new List<Button>();

        private bool _setup = false;

        public override void Setup(object args)
        {
            if (!_setup) // Only need to setup once
            { 
                _categoriesPanel.SetActive(true);
                _recipeListPanel.SetActive(false);
                _recipePanel.SetActive(false);
                LoadCategoriesUI();
                _setup = true;
            }
        }

        private void LoadCategoriesUI()
        {
            foreach (CraftingCategory category in _craftingDatabase.craftingCategories)
            {
                Button categoryButton = Instantiate(_categoryButtonPrefab, _categoriesButtonParent);
                categoryButton.gameObject.SetActive(true);
                categoryButton.onClick.AddListener(() => LoadRecipeListUI(category));

                categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;
            }
        }

        private void LoadRecipeListUI(CraftingCategory category)
        {
            // Clear existing recipe buttons
            foreach (Button activeRecipeButton in _activeRecipeButtons)
            {
                Destroy(activeRecipeButton.gameObject);
            }
            _activeRecipeButtons.Clear();

            _recipeListPanel.SetActive(true);
            foreach (CraftingRecipe recipe in category.recipes)
            {
                Button recipeButton = Instantiate(_recipeButtonPrefab, _recipeListButtonParent);
                recipeButton.gameObject.SetActive(true);
                recipeButton.onClick.AddListener(() => LoadRecipeUI(recipe));

                recipeButton.GetComponentInChildren<TextMeshProUGUI>().text = recipe.result.displayName;

                _activeRecipeButtons.Add(recipeButton); // Track active recipe buttons to remove on category switch
            }
        }

        private void LoadRecipeUI(CraftingRecipe recipe)
        {
            _recipePanel.gameObject.SetActive(true);

            _craftButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Craft {recipe.result.displayName}";
            _craftButton.onClick.RemoveAllListeners();
            _craftButton.onClick.AddListener(() => OnCraft(recipe));
        }

        private void OnCraft(CraftingRecipe recipe)
        {
            if (_inventoryService.TryRemoveItems(recipe.cost))
            {
                // Spent recipe successfully, give player the result
                int amountAdded = _inventoryService.TryAddItem(recipe.result, recipe.resultAmount, fill: true);

                _uiService.ItemGain(recipe.result, amountAdded, default); // TODO: somehow get player position (playerservice?)

                if (amountAdded < recipe.resultAmount)
                {
                    // Drop item on ground, not successfully added to inventory
                    InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, recipe.result, recipe.resultAmount - amountAdded);
                }
            }
        }
    }
}
