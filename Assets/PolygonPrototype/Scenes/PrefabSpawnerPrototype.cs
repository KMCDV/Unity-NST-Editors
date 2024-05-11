using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Prefab Painter Prototype")]
public class PrefabSpawnerPrototype : ScriptableObject
{
    public GameObject PrefabToSpawn;
    public bool ShouldRandomizeScale = false;
    public Vector2 MinMaxScale = Vector2.one;
    public bool ShouldRandomizeRotation = false;

}

[CustomEditor(typeof(PrefabSpawnerPrototype))]
public class PrefabSpawnerPrototypeCustomInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {

        PrefabSpawnerPrototype prototype = (PrefabSpawnerPrototype)target;
        VisualElement root = new VisualElement();
        
        ObjectField prefabField = new ObjectField("Prefab");
        prefabField.objectType = typeof(GameObject);
        prefabField.bindingPath = nameof(prototype.PrefabToSpawn);
        root.Add(prefabField);

        Toggle shouldScaleToggle = new Toggle("Should Affect Scale");
        shouldScaleToggle.bindingPath = nameof(prototype.ShouldRandomizeScale);
        root.Add(shouldScaleToggle);
        
        
        MinMaxSlider minMaxScaleSlider = new MinMaxSlider("Scale Range", 1, 2f, 0, 100);
        minMaxScaleSlider.bindingPath = nameof(prototype.MinMaxScale);
        
        root.Add(minMaxScaleSlider);

        BindingExtensions.Bind(root, new SerializedObject(prototype));
        
        return root;
    }
}


