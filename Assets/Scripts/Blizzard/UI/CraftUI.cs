using System;
using System.Collections.Generic;
using Blizzard.Inventory;
using Blizzard.Inventory.Crafting;
using Blizzard.Player;
using Blizzard.UI.Basic;
using Blizzard.Utilities;
using Blizzard.UI.Core;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Extensions;
using Blizzard.Utilities.Logging;
using DG.Tweening;
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

        [SerializeField] private Transform _root;
        
        // -- Categories panel --
        [SerializeField] private GameObject _categoriesPanel;
        [SerializeField] private Transform _categoriesButtonParent;

        // -- Recipes panel (per category) --
        [SerializeField] private GameObject _recipeListPanel;
        [SerializeField] private Transform _recipeListButtonParent;

        // -- Recipe panel -- 
        [SerializeField] private GameObject _recipePanel;
        [SerializeField] private Button _craftButton;
        [SerializeField] private Image _recipeIcon;
        [SerializeField] private TextMeshProUGUI _recipeTitle;
        [SerializeField] private Transform _costsParent;
        
        [Header("Animation Config")]
        [SerializeField] private float _outXPos = -630f;  // X position when not visible
        [SerializeField] private float _inXPos = 0f;      // X position when visible
        [SerializeField] private float _tweenDuration = 0.5f;
        
        [Header("Prefabs")] 
        [SerializeField] private Button _categoryButtonPrefab;
        [SerializeField] private ItemDisplay _recipeButtonPrefab;
        [SerializeField] private ItemDisplay _itemCostPrefab;

        [Inject] private InventoryService _inventoryService;
        [Inject] private UIService _uiService;
        [Inject] private EnvPrefabService _envPrefabService;
        [Inject] private PlayerService _playerService;

        private List<ItemDisplay> _activeRecipeButtons = new();

        private bool _setup = false;

        public override void Setup(object args)
        {
            if (!_setup)
            {
                _categoriesPanel.SetActive(true);
                _recipeListPanel.SetActive(false);
                _recipePanel.SetActive(false);
                LoadCategoriesUI();
                LoadRecipeListUI(_craftingDatabase.craftingCategories[0]);  
                LoadRecipeUI(_craftingDatabase.craftingCategories[0].recipes[0]);
                _setup = true;
            }
            TweenXPosition(_outXPos, _inXPos);
        }

        public override void Close()
        {
            TweenXPosition(_inXPos, _outXPos, () => base.Close());
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
            foreach (ItemDisplay activeRecipeButton in _activeRecipeButtons) Destroy(activeRecipeButton.gameObject);
            _activeRecipeButtons.Clear();

            _recipeListPanel.SetActive(true);
            foreach (CraftingRecipe recipe in category.recipes)
            {
                ItemDisplay recipeButton = Instantiate(_recipeButtonPrefab, _recipeListButtonParent);
                recipeButton.gameObject.SetActive(true);
                recipeButton.button.onClick.AddListener(() => LoadRecipeUI(recipe));
                
                recipeButton.DisplayItem(recipe.result, 1);

                _activeRecipeButtons.Add(recipeButton); // Track active recipe buttons to remove on category switch
            }
        }

        private void LoadRecipeUI(CraftingRecipe recipe)
        {
            _recipePanel.gameObject.SetActive(true);
            
            _craftButton.onClick.RemoveAllListeners();
            _craftButton.onClick.AddListener(() =>
            {
                OnCraft(recipe);
                LoadRecipeUI(recipe); // Refresh recipe UI given inventory changes (e.g. counts)
            });
            
            // Basic info
            _recipeIcon.sprite = recipe.result.icon;
            _recipeTitle.text = $"{recipe.result.displayName}";
            
            // Recipe cost
            _costsParent.DestroyChildren();
            foreach (ItemAmountPair itemCost in recipe.cost)
            {
                ItemDisplay costDisplay = Instantiate(_itemCostPrefab, _costsParent);
                costDisplay.SetIcon(itemCost.item.icon);
                
                int ownedCount = _inventoryService.CountOfItem(itemCost.item);
                costDisplay.SetCountText($"{ownedCount} / {itemCost.amount}");
            }
        }

        private void OnCraft(CraftingRecipe recipe)
        {
            if (!_inventoryService.TryRemoveItems(recipe.cost)) return;
            // Spent recipe successfully, give player the result
            int amountAdded = _inventoryService.TryAddItem(recipe.result, recipe.resultAmount, true);

            _uiService.ItemGain(recipe.result, amountAdded,
                default);

            if (amountAdded < recipe.resultAmount)
                // Drop item on ground, not successfully added to inventory
                InventoryServiceExtensions.DropItem(_envPrefabService, _playerService, recipe.result,
                    recipe.resultAmount - amountAdded);
        }

        private void TweenXPosition(float startValue, float endValue, Action onComplete = null)
        {
            RectTransform rootRect = (_root.transform as RectTransform)!;
            rootRect.anchoredPosition = new Vector2(startValue, rootRect.anchoredPosition.y);
            
            BLog.Log($"Tweening from {rootRect.anchoredPosition.x} to {endValue}");
            rootRect.DOAnchoredMoveX(endValue, _tweenDuration)
                .SetEase(Ease.OutExpo)
                .OnComplete(() => onComplete?.Invoke())
                .Play();
        }
    }
}