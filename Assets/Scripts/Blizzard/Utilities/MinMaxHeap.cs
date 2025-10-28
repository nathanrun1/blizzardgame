using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Blizzard.Utilities
{
    /// <summary>
    /// A min-max heap, A.K.A. double-ended priority queue. Provides the minimum and maximum
    /// stored element. 
    /// </summary>
    /// <typeparam name="T">Hashable & Comparable type</typeparam>
    public class MinMaxHeap<T> where T : IComparable<T>
    {
        private List<T> _elements = new();

        /// <summary>
        /// Location of each value within _elements
        /// </summary>
        private Dictionary<T, int> _elementLocation = new();

        /// <summary>
        /// Amount of each element in MinMaxHeap. An element is only removed once its amount
        /// reaches 0.
        /// </summary>
        private Dictionary<T, int> _elementCount = new();

        /// <summary>
        /// Adds an element
        /// </summary>
        /// <param name="value">Value of element to add</param>
        public void Add(T value)
        {
            // Increment count
            if (!_elementCount.TryAdd(value, 1))
            {
                _elementCount[value]++;
            }

            if (_elementCount[value] > 1) return;
            // New value, add to MinMaxHeap
            _elementLocation.Add(value, _elements.Count);
            _elements.Add(value);
            BubbleUp(_elements.Count - 1);
        }

        /// <summary>
        /// Removes an element by value
        /// </summary>
        /// <param name="value">Value of element to remove</param>
        public void Remove(T value)
        {
            Assert.IsTrue(_elementCount.ContainsKey(value),
                $"Attempted to remove element from MinMaxHeap, yet has not yet been added!\nElement: {value}");
            if (_elementCount[value] > 1)
            {
                // More than one of this element in MinMaxHeap, decrease count.
                _elementCount[value]--;
                return;
            }

            // Count of this element has reached 0, remove it.

            // Fetch location
            var index = _elementLocation[value];

            // Re-map element locations
            _elementLocation[_elements[^1]] = index;
            _elementLocation.Remove(value);

            // Replace value with last leaf node's element
            _elements[index] = _elements[^1];
            _elements.RemoveAt(_elements.Count - 1);

            // Bubble up and then trickle down from replaced index.
            // Bubble up may swap the index with its parent, and the parent may be in incorrect location, so
            //   will also need to be trickled down regardless. Thus, we do both.
            // Both bubble up/trickle down will stop when heap properties are satisfied above/below the
            //   replaced location.
            BubbleUp(index);
            TrickleDown(index);
        }

        /// <summary>
        /// Removes maximum element and returns it, assuming MinMaxHeap is not empty.
        /// </summary>
        public T DeleteMax()
        {
            var maxIndex = GetMaxIndex();
            var maxValue = _elements[maxIndex];

            // Re-map element locations
            _elementLocation[_elements[^1]] = maxIndex;
            _elementLocation.Remove(_elements[maxIndex]);

            // Replace max value with last leaf node's element
            _elements[maxIndex] = _elements[^1];
            _elements.RemoveAt(_elements.Count - 1);

            // Trickle down from replaced node to correct heap structure
            TrickleDown(maxIndex);

            return maxValue;
        }

        /// <summary>
        /// Removes minimum element and returns it, assuming MinMaxHeap is not empty
        /// </summary>
        /// <returns></returns>
        public T DeleteMin()
        {
            var minIndex = GetMinIndex();
            var minValue = _elements[minIndex];

            // Re-map element locations
            _elementLocation[_elements[^1]] = minIndex;
            _elementLocation.Remove(_elements[minIndex]);

            // Replace max value with last leaf node's element
            _elements[minIndex] = _elements[^1];
            _elements.RemoveAt(_elements.Count - 1);

            // Trickle down from replaced node to correct heap structure
            TrickleDown(minIndex);

            return minValue;
        }

        /// <summary>
        /// Retrieves the maximum element, assuming the MinMaxHeap is not empty
        /// </summary>
        /// <returns></returns>
        public T GetMax()
        {
            return _elements[GetMaxIndex()];
        }

        /// <summary>
        /// Retrieves the minimum element, assuming the MinMaxHeap is not empty
        /// </summary>
        public T GetMin()
        {
            return _elements[GetMinIndex()];
        }

        /// <summary>
        /// Determines whether MinMaxHeap is empty
        /// </summary>
        public bool IsEmpty()
        {
            return _elements.Count == 0;
        }

        // -- Private --

        /// <summary>
        /// Retrieves the maximum element's index, assuming the MinMaxHeap is not empty
        /// </summary>
        private int GetMaxIndex()
        {
            return _elements.Count switch
            {
                0 => throw new InvalidOperationException(
                    "Tried to get maximum element, but MinMaxHeap contains no elements!"),
                > 2 =>
                    // Return max of the two elements on level 1 (one of them is max)
                    _elements[1].CompareTo(_elements[2]) > 0 ? 1 : 2,
                > 1 =>
                    // Only a single element on level 1, is max
                    1,
                _ => 0
            };
        }

        /// <summary>
        /// Retrieves the minimum element's index, assuming the MinMaxHeap is not empty
        /// </summary>
        private int GetMinIndex()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException(
                    "Tried to get minimum element, but MinMaxHeap contains no elements!");
            // Min element always at root
            return 0;
        }

        /// <summary>
        /// Re-orders subtree rooted at index such that it holds
        /// min-max heap properties, assuming that the node
        /// is on a min (even) level.
        /// </summary>
        private void TrickleDownMin(int index)
        {
            while (true)
            {
                var m = GetMinChildGrandchild(index);
                if (m == -1) return;
                if (IsGrandchild(index, m))
                {
                    if (_elements[m].CompareTo(_elements[index]) < 0)
                    {
                        // Larger than grandchild in min row, swap with grandchild
                        Swap(m, index);
                        if (_elements[m].CompareTo(_elements[GetParent(m)]) < 0)
                            // Parent of new grandchild (in max row) smaller than grandchild, swap
                            //   new grandchild and its parent
                            Swap(m, GetParent(m));

                        index = m;
                        continue;
                    }

                    // Position is correct, done.
                    break;
                }

                // Is child, and child is a leaf node (otherwise couldn't be smallest)
                if (_elements[m].CompareTo(_elements[index]) < 0)
                    // Larger than child, swap.
                    Swap(m, index);
                // Position is correct, done.
                break;
            }
        }

        /// <summary>
        /// Re-orders subtree rooted at index such that it holds
        /// min-max heap properties, assuming that the node
        /// is on a max (odd) level.
        /// </summary>
        private void TrickleDownMax(int index)
        {
            while (true)
            {
                var m = GetMaxChildGrandchild(index);
                if (m == -1) return;
                if (IsGrandchild(index, m))
                {
                    if (_elements[m].CompareTo(_elements[index]) > 0)
                    {
                        // Larger than grandchild in min row, swap with grandchild
                        Swap(m, index);
                        if (_elements[m].CompareTo(_elements[GetParent(m)]) > 0)
                            // Parent of new grandchild (in max row) larger than grandchild, swap
                            //   new grandchild and its parent
                            Swap(m, GetParent(m));

                        index = m;
                        continue;
                    }

                    // Position is correct, done.
                    return;
                }

                // Is child, and child is a leaf node (otherwise couldn't be largest)
                if (_elements[m].CompareTo(_elements[index]) > 0)
                    // Smaller than child, swap.
                    Swap(m, index);
                // Position is correct, done.
                return;
            }
        }

        /// <summary>
        /// Re-orders subtree rooted at index such that it holds
        /// min-max heap properties.
        /// </summary>
        private void TrickleDown(int index)
        {
            if (index % 2 == 0)
                TrickleDownMin(index);
            else
                TrickleDownMax(index);
        }

        /// <summary>
        /// Re-orders path from node to root such that the full tree
        /// holds min-max heap properties, assuming that the node is
        /// on a max (odd) level.
        /// </summary>
        private void BubbleUpMin(int index)
        {
            while (true)
            {
                if (index <= 2) return; // Has no grandparent
                var grandparentIndex = GetGrandparent(index);
                if (_elements[index].CompareTo(_elements[grandparentIndex]) < 0)
                {
                    // Smaller than grandparent, swap and continue
                    Swap(index, grandparentIndex);
                    index = grandparentIndex;
                    continue;
                }

                // In correct position, done.
                break;
            }
        }

        /// <summary>
        /// Re-orders path from node to root such that the full tree
        /// holds min-max heap properties, assuming that the node is
        /// on a max (odd) level.
        /// </summary>
        private void BubbleUpMax(int index)
        {
            while (true)
            {
                if (index <= 2) return; // Has no grandparent
                var grandparentIndex = GetGrandparent(index);
                if (_elements[index].CompareTo(_elements[grandparentIndex]) > 0)
                {
                    // Larger than grandparent, swap and continue
                    Swap(index, grandparentIndex);
                    index = grandparentIndex;
                    continue;
                }

                // In correct position, done.
                break;
            }
        }

        /// <summary>
        /// Re-orders path from node to root such that the full tree
        /// holds min-max heap properties
        /// </summary>
        private void BubbleUp(int index)
        {
            if (index == 0) return; // Index is root, done.
            var parentIndex = GetParent(index);
            if (index % 2 == 0)
            {
                // On a min level
                if (_elements[index].CompareTo(_elements[parentIndex]) > 0)
                {
                    // Larger than parent and on min level, swap with parent.
                    Swap(index, parentIndex);
                    // Value at 'index' guaranteed to be correct, 'parentIndex' might not (is new), bubble up.
                    BubbleUpMax(parentIndex);
                }
                else
                {
                    // Correctly smaller than parent, but may be too small for previous min levels, bubble up.
                    BubbleUpMin(index);
                }
            }
            else
            {
                // On a max level
                if (_elements[index].CompareTo(_elements[parentIndex]) < 0)
                {
                    // Smaller than parent and on max level, swap with parent
                    Swap(index, parentIndex);
                    // Value at 'index' guaranteed to be correct, 'parentIndex' might not (is new), bubble up.
                    BubbleUpMin(parentIndex);
                }
                else
                {
                    // Correctly larger than parent, but may be too big for previous max levels, bubble up.
                    BubbleUpMax(index);
                }
            }
        }

        // -- Helpers --
        /// <summary>
        /// Retrieves the index of the minimum element out of a node's
        /// children and grandchildren. Returns -1 if node has no children.
        /// </summary>
        private int GetMinChildGrandchild(int n)
        {
            var minIndex = -1;
            int cur;
            for (var i = 0; i < 2; ++i) // Children
            {
                cur = GetChild(n, i);
                if (cur >= _elements.Count) return minIndex;
                minIndex = _elements[minIndex].CompareTo(_elements[cur]) < 0 ? cur : minIndex;
            }

            for (var j = 0; j < 4; ++j) // Grandchildren
            {
                cur = GetGrandchild(n, j);
                if (cur >= _elements.Count) return minIndex;
                minIndex = _elements[minIndex].CompareTo(_elements[cur]) < 0 ? cur : minIndex;
            }

            return minIndex;
        }

        /// <summary>
        /// Retrieves the index of the minimum element out of a node's
        /// children and grandchildren. Returns -1 if node has no children.
        /// </summary>
        private int GetMaxChildGrandchild(int n)
        {
            var maxIndex = -1;
            int cur;
            for (var i = 0; i < 2; ++i) // Children
            {
                cur = GetChild(n, i);
                if (cur >= _elements.Count) return maxIndex;
                maxIndex = _elements[maxIndex].CompareTo(_elements[cur]) > 0 ? cur : maxIndex;
            }

            for (var j = 0; j < 4; ++j) // Grandchildren
            {
                cur = GetGrandchild(n, j);
                if (cur >= _elements.Count) return maxIndex;
                maxIndex = _elements[maxIndex].CompareTo(_elements[cur]) > 0 ? cur : maxIndex;
            }

            return maxIndex;
        }

        /// <summary>
        /// Swaps contents of t and f
        /// </summary>
        private void Swap(int t, int f)
        {
            // Update element location map
            _elementLocation[_elements[t]] = f;
            _elementLocation[_elements[f]] = t;

            // Swap elements
            (_elements[t], _elements[f]) = (_elements[f], _elements[t]);
        }


        /// <summary>
        /// Determines if c is a child of n
        /// </summary>
        private static bool IsChild(int n, int c)
        {
            // True if c == 2n + 1 or c == 2n + 2
            return c > n && c - 2 * n <= 2;
        }

        /// <summary>
        /// Determines if g is a grandchild of n
        /// </summary>
        private static bool IsGrandchild(int n, int g)
        {
            return 4 * n + 3 <= g && g <= 4 * n + 6;
        }

        /// <summary>
        /// Retrieves the ith child of n, where i is either 0 or 1.
        /// </summary>
        private static int GetChild(int n, int i)
        {
            return 2 * n + i + 1;
        }

        /// <summary>
        /// Retrieves the parent of n
        /// </summary>
        private static int GetParent(int n)
        {
            return (n - 1) / 2;
        }

        private static int GetGrandparent(int n)
        {
            return (n - 3) / 4;
        }

        /// <summary>
        /// Retrieves the ith grandchild of n, where i is between 0 and 3
        /// </summary>
        private static int GetGrandchild(int n, int i)
        {
            return 4 * n + 3 + i;
        }
    }
}