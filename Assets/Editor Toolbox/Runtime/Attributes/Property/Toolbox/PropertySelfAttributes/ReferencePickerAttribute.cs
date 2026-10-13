#if UNITY_2019_3_OR_NEWER
using System;
using System.Diagnostics;
using static UnityEngine.ReferencePickerAttribute;

namespace UnityEngine
{

    /// <summary>
    /// Creates dedicated drawer for fields marked with the <see cref="SerializeReference"/>.
    /// 
    /// <para>Supported types: any serializable type and field with the <see cref="SerializeReference"/> attribute.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class ReferencePickerAttribute: ToolboxSelfPropertyAttribute, IReferencePickerAttribute
    {

        /// <summary>
        /// Defines the shared contract for reference picker attributes.
        /// Acts as the core configuration interface for both IMGUI and UI Toolkit reference picking drawers.
        /// </summary>
        public interface IReferencePickerAttribute
        {

            /// <summary>
            /// Restricts the available types in the picker to the specified parent type and its derivatives.
            /// If null, the base type of the field will be used automatically.
            /// </summary>
            public Type ParentType { get; set; }

            /// <summary>
            /// Indicates if created instance should be uninitialized.
            /// </summary>
            public bool ForceUninitializedInstance { get; set; }

            /// <summary>
            /// Gets or sets grouping of selectable classes.
            /// Defaults to <see cref="TypeGrouping.None"/> unless explicitly specified.
            /// </summary>
            public TypeGrouping TypeGrouping { get; set; }

            /// <summary>
            /// Indicates if created popup menu should have an additional search field.
            /// </summary>
            public bool AddTextSearchField { get; set; }

            /// <summary>
            /// Indicates if confirmation box should appear after picking a new reference. 
            /// </summary>
            public bool AddConfirmationBox { get; set; }

        }


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

        public ReferencePickerAttribute() 
        { }

        public ReferencePickerAttribute(Type parentType) : this(parentType, TypeGrouping.None)
        { }

        public ReferencePickerAttribute(Type parentType, TypeGrouping typeGrouping)
        {
            ParentType   = parentType;
            TypeGrouping = typeGrouping;
        }

    }
}
#endif