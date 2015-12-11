using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(RecastConfig))]
public class RecastLayerEditor : Editor
{
    public static ReorderableList layersList;
    public static ReorderableList filtersList;

    private void OnEnable()
    {
        parseLayersList();
        parseFiltersList();
    }

    private void parseLayersList()
    {
        layersList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Layers"),
                true, true, true, true);

        layersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = layersList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("LayerID"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 180, rect.y, rect.width - 180 - 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Cost"), GUIContent.none);
        };
        layersList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), "Layer ID");
            EditorGUI.LabelField(new Rect(rect.x + 190, rect.y, 20, EditorGUIUtility.singleLineHeight), "|");
            EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, rect.width - 180 - 30, EditorGUIUtility.singleLineHeight), "Cost");
        };
        /*
        list.onSelectCallback = (ReorderableList l) => {
            var prefab = l.serializedProperty.GetArrayElementAtIndex(l.index).FindPropertyRelative("Cost").objectReferenceValue as GameObject;
            if (prefab) EditorGUIUtility.PingObject(prefab.gameObject);
        };
        */
        layersList.onCanRemoveCallback = (ReorderableList l) => {
            return l.count > 1;
        };
        layersList.onRemoveCallback = (ReorderableList l) => {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the wave?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
        layersList.onAddCallback = (ReorderableList l) => {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("LayerID").stringValue = "";
            element.FindPropertyRelative("Cost").floatValue = 1;
        };
        /*
        list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();
            menu.ShowAsContext();
        };
        */
    }

    private void parseFiltersList()
    {
        filtersList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("Filters"),
                false, true, true, true);

        filtersList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Filters");
        };
        filtersList.drawElementCallback += (rect, index, active, focused) =>
        {
            rect.y += 2;
            var element = filtersList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none);

            filtersList.elementHeight = Math.Max(filtersList.elementHeight, EditorGUI.GetPropertyHeight(element) + 20);
        };
        filtersList.onAddCallback = (ReorderableList l) => {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Include").ClearArray();
            element.FindPropertyRelative("Exclude").ClearArray();
        };
        filtersList.onCanRemoveCallback = (ReorderableList l) => {
            return l.count > 1;
        };
        filtersList.onRemoveCallback = (ReorderableList l) => {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the wave?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        layersList.DoLayoutList();
        filtersList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Filter))]
public class FilterPropertyDrawer: PropertyDrawer
{
    private Dictionary<SerializedProperty, ReorderableList> includeList = new Dictionary<SerializedProperty, ReorderableList>();
    private Dictionary<SerializedProperty, ReorderableList> excludeList = new Dictionary<SerializedProperty, ReorderableList>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!includeList.ContainsKey(property)) initList(property, label);

        return includeList[property].GetHeight() + excludeList[property].GetHeight();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!includeList.ContainsKey(property)) initList(property, label);

        includeList[property].serializedProperty = property.FindPropertyRelative("Include");
        includeList[property].DoList(position);
        includeList[property].serializedProperty.serializedObject.ApplyModifiedProperties();

        position.y += includeList[property].GetHeight();
        excludeList[property].serializedProperty = property.FindPropertyRelative("Exclude");
        excludeList[property].DoList(position);
        includeList[property].serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    private void initList(SerializedProperty property, GUIContent label)
    {
        includeList.Add(property, new ReorderableList(property.serializedObject, property.FindPropertyRelative("Include"), false, true, true, true));
        includeList[property].drawHeaderCallback += rect => GUI.Label(rect, "Include Layers");
        includeList[property].onCanRemoveCallback += (ReorderableList l) =>
        {
            return true;
        };
        includeList[property].drawElementCallback += (rect, index, active, focused) =>
        {
            EditorGUI.PropertyField(rect, includeList[property].serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
        };
        includeList[property].onAddDropdownCallback += (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();
            var layers = RecastLayerEditor.layersList.serializedProperty;

            for (int i = 0; i < layers.arraySize; ++i)
            {
                bool found = false;
                string current = layers.GetArrayElementAtIndex(i).FindPropertyRelative("LayerID").stringValue;

                for (int j = 0; !found && j < l.serializedProperty.arraySize; ++j)
                {
                    if (l.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("Name").stringValue.Equals(current))
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    menu.AddItem(new GUIContent("Layers/" + current), false, () => clickHandler(l, current));
                }
            }

            menu.ShowAsContext();
        };

        excludeList.Add(property, new ReorderableList(property.serializedObject, property.FindPropertyRelative("Exclude"), false, true, true, true));
        excludeList[property].drawHeaderCallback += rect => GUI.Label(rect, "Exclude Layers");
        excludeList[property].onCanRemoveCallback += (ReorderableList l) =>
        {
            return true;
        };
        excludeList[property].drawElementCallback += (rect, index, active, focused) =>
        {
            EditorGUI.PropertyField(rect, excludeList[property].serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
        };
        excludeList[property].onAddDropdownCallback += (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();
            var layers = RecastLayerEditor.layersList.serializedProperty;

            for (int i = 0; i < layers.arraySize; ++i)
            {
                bool found = false;
                string current = layers.GetArrayElementAtIndex(i).FindPropertyRelative("LayerID").stringValue;

                for (int j = 0; !found && j < l.serializedProperty.arraySize; ++j)
                {
                    if (l.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("Name").stringValue.Equals(current))
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    menu.AddItem(new GUIContent("Layers/" + current), false, () => clickHandler(l, current));
                }
            }

            menu.ShowAsContext();
        };
    }

    private void clickHandler(ReorderableList list, string name)
    {        
        var index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = index;
        var element = list.serializedProperty.GetArrayElementAtIndex(index);        
        element.FindPropertyRelative("Name").stringValue = name;

        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(FilterData))]
public class NestedItemPropertyDrawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var someValueProperty = property.FindPropertyRelative("Name");
        someValueProperty.stringValue = EditorGUI.TextField(position, label, someValueProperty.stringValue);
    }

}
