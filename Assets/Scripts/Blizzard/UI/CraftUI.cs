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
        [Header("References")] [SerializeField]
        private CraftingDatabase _craftingDatabase;

        // -- Categories panel --
        [SerializeField] private GameObject _categoriesPanel;
        [SerializeField] private Transform _categoriesButtonParent;

        // -- Recipes panel (per category) --
        [SerializeField] private GameObject _recipeListPanel;
        [SerializeField] private Transform _recipeListButtonParent;

        // -- Recipe panel -- 
        [SerializeField] private GameObject _recipePanel;
        [SerializeField] private Button _craftButton;

        [Header("Prefabs")] [SerializeField] private Button _categoryButtonPrefab;
        [SerializeField] private Button _recipeButtonPrefab;

        [Inject] private InventoryService _inventoryService;
        [Inject] private UIService _uiService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;

        private List<Button> _activeRecipeButtons = new();

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
            foreach (var category in _craftingDatabase.craftingCategories)
            {
                var categoryButton = Instantiate(_categoryButtonPrefab, _categoriesButtonParent);
                categoryButton.gameObject.SetActive(true);
                categoryButton.onClick.AddListener(() => LoadRecipeListUI(category));

                categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = category.categoryName;
            }
        }

        private void LoadRecipeListUI(CraftingCategory category)
        {
            // Clear existing recipe buttons
            foreach (var activeRecipeButton in _activeRecipeButtons) Destroy(activeRecipeButton.gameObject);
            _activeRecipeButtons.Clear();

            _recipeListPanel.SetActive(true);
            foreach (var recipe in category.recipes)
            {
                var recipeButton = Instantiate(_recipeButtonPrefab, _recipeListButtonParent);
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
                var amountAdded = _inventoryService.TryAddItem(recipe.result, recipe.resultAmount, true);

                _uiService.ItemGain(recipe.result, amountAdded,
                    default); // TODO: somehow get player position (playerservice?)

                if (amountAdded < recipe.resultAmount)
                    // Drop item on ground, not successfully added to inventory
                    InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, recipe.result,
                        recipe.resultAmount - amountAdded);
            }
        }
    }
}