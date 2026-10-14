#if UNITY_2019_3_OR_NEWER
using System;
using System.Diagnostics;
using static UnityEngine.ReferencePickerAttribute;

namespace UnityEngine
{

    /// <summary>
    /// UI Toolkit adapter for the <see cref="ReferencePickerAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class SubclassPickerAttribute : PropertyAttribute, IReferencePickerAttribute
    {

        /// <inheritdoc />
        public Type ParentType { get; set; }

        /// <inheritdoc />
        public bool ForceUninitializedInstance { get; set; }

        /// <inheritdoc />
        public TypeGrouping TypeGrouping { get; set; }

        /// <inheritdoc />
        public bool AddTextSearchField { get; set; }

        /// <inheritdoc />
        public bool AddConfirmationBox { get; set; }

        public SubclassPickerAttribute()
        { }

        public SubclassPickerAttribute(Type parentType) : this(parentType, TypeGrouping.None)
        { }

        public SubclassPickerAttribute(Type parentType, TypeGrouping typeGrouping)
        {
            ParentType   = parentType;
            TypeGrouping = typeGrouping;
        }

    }
}
#endif