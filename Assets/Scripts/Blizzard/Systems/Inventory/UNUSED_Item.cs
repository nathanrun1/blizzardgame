using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

// >> CURRENTLY UNUSED! <<

//namespace Blizzard.Inventory
//{
//    public class Item : ICloneable, IEquatable<Item>
//    {
//        public ItemData Data { get => _itemData; }
//        private ItemData _itemData;
//        public int Id { get => _itemData.id; }
//        public string DisplayName { get => _itemData.displayName; }
//        public ItemCategory Category { get => _itemData.category; }


//        public Dictionary<string, object> dynamicAttributes = new Dictionary<string, object>();

//        public Item(ItemData itemData)
//        {
//            _itemData = itemData;
//            SetDynamicAttributes();
//        }

//        public bool Equals(Item other)
//        {
//            if (other == null || Id != other.Id) return false;
//            BLog.Log($"Comparing equality between {this.DisplayName} (id {Id}) and {other.DisplayName} (id {other.Id})");
//            if (dynamicAttributes.Count != other.dynamicAttributes.Count) return false;

//            foreach (KeyValuePair<string, object> attr in dynamicAttributes)
//            {
//                if (!other.dynamicAttributes.ContainsKey(attr.Key) || other.dynamicAttributes[attr.Key] != attr.Value) return false;
//            }

//            return true;
//        }

//        public object Clone()
//        {
//            Item clone = new Item(_itemData);
//            clone.dynamicAttributes = new Dictionary<string, object>(dynamicAttributes);
//            return clone;
//        }

//        private void SetDynamicAttributes()
//        {
//            switch (_itemData.category)
//            {
//                case (ItemCategory.Clothing):
//                    ClothingItemData clothingItemData = _itemData as ClothingItemData;
//                    Assert.IsNotNull(clothingItemData, "ItemData category set to 'Clothing', yet not downcastable to ClothingItemData!");

//                    dynamicAttributes.Add("insulation", clothingItemData.insulation);
//                    break;
//            }
//        }
//    }
//}