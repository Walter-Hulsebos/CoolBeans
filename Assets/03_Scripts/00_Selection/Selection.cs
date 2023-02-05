using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CoolBeans.Selection
{
    public sealed class Selection
    {
        #region Singleton

        //Lazy Singleton
        private static readonly Lazy<Selection> _lazy = new(valueFactory: () => new());
        public static Selection Instance => _lazy.Value;
        
        //Private constructor
        private Selection() { }

        #endregion
        
        public readonly HashSet<ISelectable> SelectedUnits = new();
        public readonly List<ISelectable>    ExistingUnits = new();
        
        public event Action<ISelectable> OnUnitAdded;
        public event Action<ISelectable> OnUnitRemoved;
        
        public void Add(ISelectable unit)
        {
            if(ExistingUnits.Contains(unit)) return; //Already added

            ExistingUnits.Add(item: unit);
            
            OnUnitAdded?.Invoke(unit);
        }
        
        public void Remove(ISelectable unit)
        {
            if (!ExistingUnits.Contains(unit)) return; //Not added

            ExistingUnits.Remove(item: unit);
            
            if(SelectedUnits.Contains(unit))
            {
                SelectedUnits.Remove(item: unit);
            }
            
            OnUnitRemoved?.Invoke(unit);
        }


        [PublicAPI]
        public void Select(ISelectable unit)
        {
            if(unit == null) return;
            if(SelectedUnits.Contains(unit)) return; //Already selected

            SelectedUnits.Add(item: unit);
            unit.Select();
        }

        [PublicAPI]
        public void Deselect(ISelectable unit)
        {
            if (!SelectedUnits.Contains(unit)) return; //Not selected

            SelectedUnits.Remove(item: unit);
            unit.Deselect();
        }

        [PublicAPI]
        public void DeselectAll()
        {
            foreach(ISelectable unit in SelectedUnits)
            {
                unit.Deselect();
            }
            SelectedUnits.Clear();
        }

        [PublicAPI]
        public Boolean IsSelected(ISelectable unit) => SelectedUnits.Contains(item: unit);
    }
}
