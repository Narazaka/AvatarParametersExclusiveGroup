using UnityEditor;
using UnityEditorInternal;
using net.narazaka.vrchat.avatar_parameters_driver;
using net.narazaka.vrchat.avatar_parameters_driver.editor;

namespace net.narazaka.vrchat.avatar_parameters_exclusive_group.editor
{
    [CustomEditor(typeof(AvatarParameterExclusiveGroup))]
    public class AvatarParameterExclusiveGroupEditor : Editor
    {
        SerializedProperty LocalOnlyProperty;
        SerializedProperty ExclusiveParametersProperty;
        ReorderableList ExclusiveParametersList;

        void OnEnable()
        {
            ParameterUtil.Get(serializedObject, true);
            LocalOnlyProperty = serializedObject.FindProperty(nameof(AvatarParameterExclusiveGroup.LocalOnly));
            ExclusiveParametersProperty = serializedObject.FindProperty(nameof(AvatarParameterExclusiveGroup.ExclusiveParameters));
            ExclusiveParametersList = new ReorderableList(serializedObject, ExclusiveParametersProperty)
            {
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "Exclusive Parameters");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = ExclusiveParametersProperty.GetArrayElementAtIndex(index);
                    var driveCondition = element.FindPropertyRelative(nameof(ExclusiveParameter.Parameter));
                    var fallbackValue = element.FindPropertyRelative(nameof(ExclusiveParameter.FallbackValue));
                    var mode = driveCondition.FindPropertyRelative(nameof(DriveCondition.Mode));
                    rect.y += EditorGUIUtility.standardVerticalSpacing;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, driveCondition);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    if (mode.enumValueIndex < 2)
                    {
                        fallbackValue.floatValue = mode.enumValueIndex == 0 ? 0 : 1;
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.Toggle(rect, fallbackValue.displayName, fallbackValue.floatValue != 0);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUI.PropertyField(rect, fallbackValue);
                    }
                },
                elementHeightCallback = (index) =>
                {
                    return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(LocalOnlyProperty);
            ExclusiveParametersList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
