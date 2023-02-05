using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = System.Random;

using F32 = System.Single;
using I32 = System.Int32;

namespace CoolBeans.Utils
{
    public static class CollectionExtensions
    {
        private static readonly System.Random _rand = new();

        public static T Random<T>(this T[] items) 
        {
            return items[_rand.Next(0, items.Length)];
        }

        public static T Random<T>(this List<T> items) 
        {
            return items[_rand.Next(0, items.Count)];
        }

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, F32> weightSelector) 
        {
            IEnumerable<T> __weightedItems = sequence as T[] ?? sequence.ToArray();
            
            F32 __totalWeight = __weightedItems.Sum(weightSelector);
            F32 __itemWeightIndex = ((F32)new Random().NextDouble()) * __totalWeight;
            F32 __currentWeightIndex = 0;

            foreach (T __weightedItem in __weightedItems)
            {
                F32 __weight = weightSelector(__weightedItem);
                __currentWeightIndex += __weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (__currentWeightIndex >= __itemWeightIndex) return __weightedItem;
            }

            return default;
        }

        public static T Random<T>(this IEnumerable<T> sequence) 
        {
            IEnumerable<T> __enumerable = sequence as T[] ?? sequence.ToArray();
            
            I32 __max  = __enumerable.Count();
            I32 __rate = UnityEngine.Random.Range(minInclusive: 0, maxExclusive: 101);
            I32 __idx  = Mathf.Clamp(value: Mathf.RoundToInt(__max * __rate / 100f), 0, __max - 1);
            return __enumerable.Skip(__idx).First();
        }
    }
}
