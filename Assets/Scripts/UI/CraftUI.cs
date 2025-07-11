using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
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

        public override void Setup(object args)
        {
            _categoriesPanel.SetActive(true);
            _recipeListPanel.SetActive(false);
            _recipePanel.SetActive(false);
            LoadCategoriesUI();
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
            _recipeListPanel.SetActive(true);
            foreach (CraftingRecipe recipe in category.recipes)
            {
                Button recipeButton = Instantiate(_recipeButtonPrefab, _recipeListButtonParent);
                recipeButton.gameObject.SetActive(true);
                recipeButton.onClick.AddListener(() => LoadRecipeUI(recipe));

                recipeButton.GetComponentInChildren<TextMeshProUGUI>().text = recipe.result.displayName;
            }
        }

        private void LoadRecipeUI(CraftingRecipe recipe)
        {
            _recipePanel.gameObject.SetActive(true);

            _craftButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Craft {recipe.result.displayName}";
            _craftButton.onClick.AddListener(() => OnCraft(recipe));
        }



        private void Awake()
        {
            // TEMP: move to setup
            //LinkButtons();
        }

        private void LinkButtons()
        {
            //_craftButton.onClick.AddListener(() => OnCraft(_craftingDatabase.craftingCategories[0].recipes[0])); // TEMP hardcoded
        }

        private void OnCraft(CraftingRecipe recipe)
        {
            if (_inventoryService.TryRemoveItems(recipe.cost))
            {
                // Spent recipe successfully, give player the result
                int amountAdded = _inventoryService.TryAddItem(recipe.result, recipe.resultAmount, fill: true);

                _uiService.ItemGain(recipe.result, amountAdded, default); // TODO: somehow get player position (playerservice?)
            }
        }
    }
}
