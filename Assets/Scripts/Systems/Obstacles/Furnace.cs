using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Inventory;
using Blizzard.UI;
using Zenject;
using Blizzard.Inventory.Crafting;
using Unity.VisualScripting;

namespace Blizzard.Obstacles.Concrete
{
    public class Furnace : Structure, IInteractable
    {
        public class State
        {
            /// <summary>
            /// Whether an item is currently being smelted
            /// </summary>
            public bool isSmelting = false;
            /// <summary>
            /// The current smelting recipe
            /// </summary>
            public CraftingRecipe curSmeltingRecipe;
            /// <summary>
            /// Time left until current smelting recipe is finished
            /// </summary>
            public float smeltTimeRemaining;
        }

        public string PrimaryInteractText { get; private set; } = "Smelt";

        /// <summary>
        /// Base time taken to smelt a single smelting recipe
        /// </summary>
        [Header("Config")]
        private float _baseSmeltingTime = 5f;

        private InventorySlot _ingredient = new InventorySlot();
        private InventorySlot _result = new InventorySlot();
        private InventorySlot _fuel = new InventorySlot();

        /// <summary>
        /// Current state of the furnace
        /// </summary>
        private State _state = new State();

        // Services & config
        [Inject] private UIService _uiService;

        [Inject] private SmeltingDatabase _smeltingDatabase;



        protected override void Awake()
        {
            base.Awake();

            _ingredient.OnUpdate += OnFurnaceSlotUpdate;
            _result.OnUpdate += OnFurnaceSlotUpdate;
            _fuel.OnUpdate += OnFurnaceSlotUpdate;

            OnDestroy += () =>
            {
                _ingredient.OnUpdate -= OnFurnaceSlotUpdate;
                _result.OnUpdate -= OnFurnaceSlotUpdate;
                _fuel.OnUpdate -= OnFurnaceSlotUpdate;
            };
        }

        protected void Update()
        {
            // Count down smelting time
            if (_state.smeltTimeRemaining > 0)
            {
                _state.smeltTimeRemaining -= Time.deltaTime;
            }

            // Check if current smelt is done (if there is one)
            if (_state.isSmelting && _state.smeltTimeRemaining <= 0)
            {
                FinishSmelt();
            }
        }

        public void OnPrimaryInteract()
        {
            _uiService.InitUI("furnace", new FurnaceUI.Args
            {
                ingredientSlot = _ingredient,
                resultSlot = _result,
                fuelSlot = _fuel,
                furnaceState = _state
            });
        }

        private void OnFurnaceSlotUpdate()
        {
            // Check if we can trigger a new smelt (e.g. prev smelt just finished or item has been added)
            if (_ingredient.Item != null && !_state.isSmelting &&
                _smeltingDatabase.SmeltRecipeMap.TryGetValue(_ingredient.Item.id, out CraftingRecipe recipe))
            {
                // Recipe matched to ingredient! Check if we can smelt
                if (CanSmelt(recipe))
                {
                    StartSmelt(recipe);
                }
            }
        }

        /// <summary>
        /// Determines if smelting can be started
        /// </summary>
        private bool CanSmelt(CraftingRecipe recipe)
        {
            // TODO: check for fuel
            return _ingredient.Amount >= recipe.cost[0].amount // Can afford it 
                && _result.CanAdd(recipe.result, recipe.resultAmount); // Result can be added to result slot
        }

        /// <summary>
        /// Starts an item smelt.
        /// </summary>
        private void StartSmelt(CraftingRecipe recipe)
        {
            _state.isSmelting = true;

            int removed = _ingredient.Remove(recipe.cost[0].amount); // Consume ingredient
            Assert.IsTrue(removed == recipe.cost[0].amount, "Could not consume enough ingredients!");
            // Sanity check
            if (removed != recipe.cost[0].amount)
            {
                Debug.LogWarning("Attempted to smelt a recipe without consuming sufficient ingredients!");
                return;
            }

            _state.curSmeltingRecipe = recipe;
            _state.smeltTimeRemaining = _baseSmeltingTime; // TODO: associate with heat prolly
        }

        /// <summary>
        /// Finishes the current item smelt
        /// </summary>
        private void FinishSmelt()
        {
            Assert.IsTrue(_state.isSmelting, "Called FinishSmelt() when not smelting!");

            _state.smeltTimeRemaining = 0;
            _state.isSmelting = false; // Reset state before adding to result slot to trigger next smelt

            // Produce result
            bool added = _result.Add(_state.curSmeltingRecipe.result, _state.curSmeltingRecipe.resultAmount);
            Assert.IsTrue(added, "Failed to add result to result slot! This should have been prevented.");

            
        }
    }
}

// "Woobie shhhhhhh"