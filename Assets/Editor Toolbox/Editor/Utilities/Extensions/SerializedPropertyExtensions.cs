using System.Collections.Generic;
using UnityEditor;


namespace Toolbox.Editor
{

    public static class SerializedPropertyExtensions
    {
        public static IEnumerable<SerializedProperty> GetChildProperties(this SerializedProperty parent)
        {
            SerializedProperty property = parent.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(property, endProperty))
                {
                    break;
                }

                yield return property.Copy();

                enterChildren = false;
            }
        }
    }

}