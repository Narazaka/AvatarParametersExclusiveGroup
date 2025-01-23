using UnityEditor;
using UnityEditorInternal;
using net.narazaka.vrchat.avatar_parameters_driver;
using Narazaka.VRChat.AvatarParametersUtil.Editor;

namespace net.narazaka.vrchat.avatar_parameters_exclusive_group.editor
{
    [CustomEditor(typeof(AvatarParameterExclusiveGroup))]
    public class AvatarParameterExclusiveGroupEditor : Editor
    {
        SerializedProperty LocalOnlyProperty;
        SerializedProperty ExclusiveParametersProperty;
        ReorderableList ExclusiveParametersList;

        bool ShowDetail = false;

        void OnEnable()
        {
            AvatarParametersUtilEditor.Get(serializedObject, true);
            LocalOnlyProperty = serializedObject.FindProperty(nameof(AvatarParameterExclusiveGroup.LocalOnly));
            ExclusiveParametersProperty = serializedObject.FindProperty(nameof(AvatarParameterExclusiveGroup.ExclusiveParameters));
            ExclusiveParametersList = new ReorderableList(serializedObject, ExclusiveParametersProperty)
            {
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, T.ExclusiveParameters);
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = ExclusiveParametersProperty.GetArrayElementAtIndex(index);
                    var driveCondition = element.FindPropertyRelative(nameof(ExclusiveParameter.Parameter));
                    var fallbackValue = element.FindPropertyRelative(nameof(ExclusiveParameter.FallbackValue));
                    var mode = driveCondition.FindPropertyRelative(nameof(DriveCondition.Mode));
                    rect.y += EditorGUIUtility.standardVerticalSpacing;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, driveCondition, T.Parameter.GUIContent);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    if (mode.enumValueIndex < 2)
                    {
                        fallbackValue.floatValue = mode.enumValueIndex == 0 ? 0 : 1;
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.Toggle(rect, T.FallbackValue, fallbackValue.floatValue != 0);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUI.PropertyField(rect, fallbackValue, T.FallbackValue.GUIContent);
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
            EditorGUILayout.HelpBox(T.Description, MessageType.Info);
            ShowDetail = EditorGUILayout.Foldout(ShowDetail, T.Detail);
            if (ShowDetail)
            {
                EditorGUILayout.HelpBox(T.DetailDescription, MessageType.Info);
            }
            EditorGUILayout.PropertyField(LocalOnlyProperty, T.LocalOnly.GUIContent);
            ExclusiveParametersList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        static class T
        {
            public static istring Description = new istring(
                "Set a pair of parameters that do not conflict at the same time. Parameters other than the one that caused the conflict will be automatically changed to the fallback value.",
                "同時に成立しないパラメーターの組を設定します。競合を起こしたパラメーター以外がフォールバック値に自動的に変更されるようになります。"
                );
            public static istring Detail = new istring("Examples", "例");
            public static istring DetailDescription = new istring(
                "■Example: When a pair of A=ON and B=ON is set\n\nA=ON\n↓\nbecomes B=ON\n↓\n↓(automatic conflict resolution)\n↓\nswitched to A=OFF",
                "■例: A=ONとB=ONの組を設定した時\n\nA=ONの状態\n↓\nB=ONになった！\n↓\n↓（自動で競合を解消）\n↓\nA=OFFに切り替わる"
                );
            public static istring LocalOnly = new istring("Local Only", "ローカルのみ");
            public static istring ExclusiveParameters = new istring("Conflicting Parameter Sets", "競合するパラメータのセット");
            public static istring Parameter = new istring("Parameter", "パラメーター");
            public static istring FallbackValue = new istring("Fallback Value On Conflict", "競合時にフォールバックする値");
        }
    }
}
