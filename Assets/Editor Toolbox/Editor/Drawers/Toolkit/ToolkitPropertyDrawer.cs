using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace Toolbox.Editor.Drawers
{

    /// <summary>
    /// Base UI Toolkit property drawer for custom classes and generic types.
    /// Wraps the property in a <see cref="Foldout"/> and delegates child rendering 
    /// to the Toolbox layout engine, preserving attribute-based sorting (e.g., [Order]) and filtering.
    /// </summary>
    public abstract class ToolkitPropertyDrawer: PropertyDrawer
    {

        /// <summary>
        /// Creates the visual element tree for the property.
        /// </summary>
        /// <param name="property">The serialized property to create the UI for.</param>
        /// <returns>A bound <see cref="Foldout"/> containing the correctly ordered children.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout()
            {
                text = property.displayName,
                value = property.isExpanded
            };

            // Populate children using Toolbox's handler to respect custom order/logic,
            // while deferring the actual drawing to the native UI Toolkit PropertyField.
            ToolboxEditorGui.DrawPropertyChildren(property, p => foldout.Add(new PropertyField(p)));

            // Natively binds the foldout's state (including isExpanded) and its children to the serialized data.
            foldout.BindProperty(property);

            return foldout;
        }

    }

}