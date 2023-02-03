using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sisus.Shared.EditorOnly
{
    /// <summary>
    /// Class that can wrap the <see cref="IMGUIContainer.onGUIHandler"/>
    /// of a component header <see cref="IMGUIContainer"/> and allow injecting
    /// drawing logic to occure before and after its drawing.
    /// </summary>
    internal class ComponentHeaderWrapper
    {
        private readonly IMGUIContainer headerElement;
        private readonly Component component;
        private readonly Action wrappedOnGUIHandler;
        private readonly bool supportsRichText;

        public Component Component => component;

        public ComponentHeaderWrapper(IMGUIContainer headerElement, Component component, bool supportsRichText)
		{
            this.headerElement = headerElement;
            this.component = component;
            this.supportsRichText = supportsRichText;
            wrappedOnGUIHandler = headerElement.onGUIHandler;
        }

		public void DrawWrappedHeaderGUI()
        {
            if(component == null)
			{
                Unwrap();
                return;
			}

            Rect headerRect = headerElement.contentRect;
            bool HeaderIsSelected = headerElement.focusController.focusedElement == headerElement;

            ComponentHeader.InvokeBeforeHeaderGUI(component, headerRect, HeaderIsSelected, supportsRichText);
            wrappedOnGUIHandler?.Invoke();
            ComponentHeader.InvokeAfterHeaderGUI(component, headerRect, HeaderIsSelected, supportsRichText);
        }

        private void Unwrap()
		{
            if(headerElement is null)
            {
                #if DEV_MODE
			    Debug.Log("headerElement null...");
			    #endif

                return;
            }

            #if DEV_MODE
            if(headerElement.onGUIHandler != null) Debug.Log("Unwrapping...");
			#endif

            headerElement.onGUIHandler = wrappedOnGUIHandler;
		}
    }
}