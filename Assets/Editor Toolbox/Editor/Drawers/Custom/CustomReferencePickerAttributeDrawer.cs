#if UNITY_2019_3_OR_NEWER
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Toolbox.Editor.Drawers
{
    using System.Collections.Generic;
    using Toolbox.Editor.Internal;
    using Toolbox.Editor.Internal.Types;


    [CustomPropertyDrawer(typeof(CustomReferencePickerAttribute))]
    public class CustomReferencePickerAttributeDrawer: PropertyDrawer
    {
        private const float labelWidthOffset = -80.0f;

        private static readonly TypeConstraintContext sharedConstraint = new TypeConstraintSerializeReference(null);
        private static readonly TypeAppearanceContext sharedAppearance = new TypeAppearanceContext(sharedConstraint, TypeGrouping.None, true);
        private static readonly TypeField typeField = new TypeField(sharedConstraint, sharedAppearance);
        private static readonly GUIContent tempChildLabel = new GUIContent();

        private void UpdateContexts(CustomReferencePickerAttribute attribute)
        {
            sharedAppearance.TypeGrouping = attribute.TypeGrouping;
        }

        private Type GetParentType(CustomReferencePickerAttribute attribute, SerializedProperty property)
        {
            var fieldInfo = property.GetFieldInfo(out _);
            var fieldType = property.GetProperType(fieldInfo);
            var candidateType = attribute.ParentType;
            if (candidateType != null)
            {
                if (fieldType.IsAssignableFrom(candidateType))
                {
                    return candidateType;
                }

                ToolboxEditorLog.AttributeUsageWarning(attribute, property,
                    $"Provided {nameof(attribute.ParentType)} ({candidateType}) cannot be used because it's not assignable from: '{fieldType}'");
            }

            return fieldType;
        }

        private static Type GetCurrentManagedReferenceType(SerializedProperty property, out bool hasMixedValues)
        {
            var fullTypeName = property.managedReferenceFullTypename;
            hasMixedValues = false;
            TypeUtility.TryGetTypeFromManagedReferenceFullTypeName(fullTypeName, out var targetType);

            var currentSerializedObject = property.serializedObject;
            if (currentSerializedObject.isEditingMultipleObjects)
            {
                var targets = currentSerializedObject.targetObjects;
                foreach (var target in targets)
                {
                    using (var tempSerializedObject = new SerializedObject(target))
                    {
                        var tempProperty = tempSerializedObject.FindProperty(property.propertyPath);
                        if (tempProperty == null)
                        {
                            break;
                        }

                        if (tempProperty.managedReferenceFullTypename != fullTypeName)
                        {
                            hasMixedValues = true;
                            break;
                        }
                    }
                }
            }

            return targetType;
        }

        private void CreateTypeProperty(SerializedProperty property, Type parentType, CustomReferencePickerAttribute attribute, Rect position)
        {
            var searchFieldRequired = attribute.AddTextSearchField;
            var confirmationRequired = attribute.AddConfirmationBox;

            var currentType = GetCurrentManagedReferenceType(property, out var hasMixedValues);
            EditorGUI.showMixedValue = hasMixedValues;
            typeField.OnGui(position, searchFieldRequired, (type) =>
            {
                try
                {
                    var isOperationAllowed = true;
                    if (confirmationRequired)
                    {
                        isOperationAllowed = EditorUtility.DisplayDialog("Confirm Assignment",
                            "Are you sure you want to assign a new reference? This action will replace the current one.",
                            "Assign",
                            "Cancel");
                    }

                    if (isOperationAllowed)
                    {
                        if (!property.serializedObject.isEditingMultipleObjects)
                        {
                            UpdateTypeProperty(property, type, attribute);
                        }
                        else
                        {
                            var targets = property.serializedObject.targetObjects;
                            foreach (var target in targets)
                            {
                                using (var so = new SerializedObject(target))
                                {
                                    var sp = so.FindProperty(property.propertyPath);
                                    UpdateTypeProperty(sp, type, attribute);
                                }
                            }
                        }
                    }
                }
                catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException)
                {
                    ToolboxEditorLog.LogWarning("Invalid attempt to update disposed property.");
                }
            }, currentType, parentType);
            EditorGUI.showMixedValue = false;
        }

        private void UpdateTypeProperty(SerializedProperty property, Type targetType, CustomReferencePickerAttribute attribute)
        {
            var forceUninitializedInstance = attribute.ForceUninitializedInstance;
            var newInstance = ReflectionUtility.CreateInstance(targetType, forceUninitializedInstance);
            property.serializedObject.Update();
            property.managedReferenceValue = newInstance;
            property.serializedObject.ApplyModifiedProperties();

            DrawerStorageManager.ClearStorages();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var attr = (CustomReferencePickerAttribute)attribute;
            UpdateContexts(attr);

            bool hasLabel = !string.IsNullOrEmpty(label.text);

            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect labelRect = new Rect(headerRect.x, headerRect.y, EditorGUIUtility.labelWidth, headerRect.height);

            float currentLabelWidth = hasLabel ? EditorGUIUtility.labelWidth + labelWidthOffset : 0;
            Rect inputRect = new Rect(headerRect.x + currentLabelWidth, headerRect.y, headerRect.width - currentLabelWidth, headerRect.height);

            if (hasLabel)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            }

            var parentType = GetParentType(attr, property);
            CreateTypeProperty(property, parentType, attr, inputRect);

            EditorGUI.EndProperty();

            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    PropertyDrawer customDrawer = GetCustomPropertyDrawer(property);
                    if (customDrawer != null)
                    {
                        Rect indentedRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, customDrawer.GetPropertyHeight(property, label));
                        customDrawer.OnGUI(indentedRect, property, label);
                    }
                    else
                    {
                        // ToolboxEditorGui.DrawPropertyChildren(property);
                        Rect childPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, 0);

                        foreach (SerializedProperty childProperty in property.GetChildProperties())
                        {
                            float height = EditorGUI.GetPropertyHeight(childProperty, true);
                            childPosition.height = height;

                            EditorGUI.PropertyField(childPosition, childProperty, true);

                            childPosition.y += height + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }
        }

        private PropertyDrawer GetCustomPropertyDrawer(SerializedProperty property)
        {
            Type propertyType = ManagedReferenceUtility.GetType(property.managedReferenceFullTypename);
            if (propertyType != null && PropertyDrawerCache.TryGetPropertyDrawer(propertyType, out PropertyDrawer drawer))
            {
                return drawer;
            }
            return null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (!property.isExpanded || string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;

            PropertyDrawer customDrawer = GetCustomPropertyDrawer(property);
            if (customDrawer != null)
            {
                height += customDrawer.GetPropertyHeight(property, label);
                return height;
            }

            height += GetChildrenHeight(property);
            return height;
        }

        private static float GetChildrenHeight(SerializedProperty property)
        {
            float height = 0f;
            bool first = true;

            foreach (SerializedProperty child in property.GetChildProperties())
            {
                if (!first)
                {
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
                first = false;

                tempChildLabel.text = child.displayName;
                tempChildLabel.tooltip = child.tooltip;

                height += EditorGUI.GetPropertyHeight(child, tempChildLabel, true);
            }

            return height;
        }
    }
}
#endif