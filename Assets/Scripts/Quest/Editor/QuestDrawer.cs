using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Quest))]
public class QuestDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var linePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(linePosition, property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;
        linePosition.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        var iterator = property.Copy();
        var endProperty = iterator.GetEndProperty();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            enterChildren = false;

            if (iterator.name == "sideQuestsToUnlock")
            {
                DrawSideQuestUnlocks(linePosition, iterator, property.serializedObject.targetObject as QuestSystem);
                linePosition.y += GetSideQuestUnlocksHeight(iterator) + VerticalSpacing;
            }
            else
            {
                EditorGUI.PropertyField(linePosition, iterator, true);
                linePosition.y += EditorGUI.GetPropertyHeight(iterator, true) + VerticalSpacing;
            }
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        var height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        var iterator = property.Copy();
        var endProperty = iterator.GetEndProperty();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            enterChildren = false;
            height += iterator.name == "sideQuestsToUnlock"
                ? GetSideQuestUnlocksHeight(iterator) + VerticalSpacing
                : EditorGUI.GetPropertyHeight(iterator, true) + VerticalSpacing;
        }

        return height;
    }

    private static void DrawSideQuestUnlocks(Rect position, SerializedProperty property, QuestSystem questSystem)
    {
        var questNames = new List<string> { "None" };
        if (questSystem != null)
        {
            foreach (var sideQuest in questSystem.sideQuests)
            {
                if (sideQuest != null && !string.IsNullOrEmpty(sideQuest.text))
                {
                    questNames.Add(sideQuest.text);
                }
            }
        }

        EditorGUI.LabelField(position, "Side Quests To Unlock");
        position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;

        for (var index = 0; index < property.arraySize; index++)
        {
            var element = property.GetArrayElementAtIndex(index);
            var popupPosition = new Rect(position.x, position.y, position.width - 24f, EditorGUIUtility.singleLineHeight);
            var removePosition = new Rect(position.xMax - 20f, position.y, 20f, EditorGUIUtility.singleLineHeight);
            var selectedIndex = Mathf.Max(0, questNames.IndexOf(element.stringValue));

            selectedIndex = EditorGUI.Popup(popupPosition, selectedIndex, questNames.ToArray());
            element.stringValue = selectedIndex == 0 ? string.Empty : questNames[selectedIndex];
            if (GUI.Button(removePosition, "-"))
            {
                property.DeleteArrayElementAtIndex(index);
                break;
            }

            position.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        if (GUI.Button(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Add Side Quest"))
        {
            property.arraySize++;
            property.GetArrayElementAtIndex(property.arraySize - 1).stringValue = string.Empty;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private static float GetSideQuestUnlocksHeight(SerializedProperty property)
    {
        return (property.arraySize + 2) * (EditorGUIUtility.singleLineHeight + VerticalSpacing);
    }
}