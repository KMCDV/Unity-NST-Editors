using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PrefabSpawner : EditorWindow
{
    public float Radius = 1f;
    public int objectsToSpawnAmount = 1;
    public bool isRadiusUsingSlider = false;

    public List<Vector3> Points = new List<Vector3>();
    public Vector3 lastRaycastHitPoint;
    public Vector3 lastRaycastHitNormal;

    public PrefabSpawnerPrototype GameObjectToSpawn;
    public List<GameObject> CurrentGameObjects = new List<GameObject>();

    private PrefabSpawnerPrototype[] _prefabSpawnerPrototypes = Array.Empty<PrefabSpawnerPrototype>();
    
    [MenuItem("Window/Prefab Spawner")]
    public static void ShowExample()
    {
        PrefabSpawner wnd = GetWindow<PrefabSpawner>();
        wnd.titleContent = new GUIContent("PrefabSpawner");
    }
    private void OnBecameVisible()
    {
        
        SceneView.duringSceneGui += OnSceneGui;
    }

    private void OnSceneGui(SceneView obj)
    {
        bool isHit = Physics.Raycast(Camera.current.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2)),
            out RaycastHit hit);
        if(isHit == false)
            return;
        lastRaycastHitPoint = hit.point;
        lastRaycastHitNormal = hit.normal;
        Handles.DrawWireDisc(hit.point, Vector3.up, Radius);
        for (var index = 0; index < Points.Count; index++)
        {
            var po = Points[index];
            if (Physics.Raycast(hit.point + po + Vector3.up, Vector3.down, out RaycastHit newRaycast))
            {
                Handles.DrawLine(newRaycast.point, newRaycast.point + newRaycast.normal);
                if(CurrentGameObjects.Count <= index)
                    continue;
                CurrentGameObjects[index].transform.position = newRaycast.point;
                CurrentGameObjects[index].transform.up = newRaycast.normal;
            }
        }
    }

    private void OnBecameInvisible()
    {
        SceneView.duringSceneGui -= OnSceneGui;
    }

    private void GeneratePoints()
    {
        Points.Clear();
        for (int i = 0; i < objectsToSpawnAmount; i++)
        {
            Vector3 newPoint = Random.insideUnitSphere * Radius;
            newPoint.y = 0;
            Points.Add(newPoint);
        }
        SceneView.RepaintAll();
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        _prefabSpawnerPrototypes = AssetDatabase.FindAssets("t:PrefabSpawnerPrototype")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<PrefabSpawnerPrototype>)
            .ToArray();

        root.Clear();
        
        Toggle sliderValueRadiusToggle = new Toggle("Range Slider");
        sliderValueRadiusToggle.value = isRadiusUsingSlider;
        sliderValueRadiusToggle.RegisterValueChangedCallback(x =>
        {
            isRadiusUsingSlider = x.newValue;
            CreateGUI();
        });

        FloatField floatField = new FloatField(nameof(Radius));
        floatField.value = Radius;
        

        Slider slider = new Slider("Radius slider");
        slider.value = Radius;
        slider.highValue = 100f;
        slider.lowValue = 0f;
        
        floatField.RegisterValueChangedCallback(x =>
        {
            Radius = x.newValue;
            slider.value = Radius;
            SceneView.RepaintAll();
        });
        slider.RegisterValueChangedCallback(x =>
        {
            Radius = x.newValue;
            floatField.value = Radius;
            SceneView.RepaintAll();
        });

        SliderInt objectsAmountSlider = new SliderInt("Amount to spawn");
        objectsAmountSlider.value = objectsToSpawnAmount;
        objectsAmountSlider.RegisterValueChangedCallback(x =>
        {
            objectsToSpawnAmount = x.newValue;
            GeneratePoints();
        });
        
        DropdownField dropdownField = new DropdownField("Prototype to Spawn");
        dropdownField.value = GameObjectToSpawn != null ? GameObjectToSpawn.name : "";
        dropdownField.choices = _prefabSpawnerPrototypes.Select(x => x.name).ToList();

        dropdownField.RegisterValueChangedCallback(x =>
        {
            GameObjectToSpawn = _prefabSpawnerPrototypes.FirstOrDefault(y => y.name == x.newValue);
            CreateGUI();
        });

        /*
        ObjectField gameObjectToSpawn = new ObjectField("Prefab To Spawn");
        gameObjectToSpawn.objectType = typeof(GameObject);
        gameObjectToSpawn.RegisterValueChangedCallback(x =>
        {
            GameObjectToSpawn = x.newValue as GameObject;
            if(Points.Count <= 0)
                GeneratePoints();
            RenderNewObjectsOnPoints();
        });
        */
        
        root.Add(sliderValueRadiusToggle);
        if(sliderValueRadiusToggle.value)
            root.Add(slider);
        else
            root.Add(floatField);
        
        
        root.Add(objectsAmountSlider);
        root.Add(dropdownField);
        if (GameObjectToSpawn != null)
        {
            VisualElement soVE = Editor.CreateEditor(GameObjectToSpawn).CreateInspectorGUI();
            root.Add(soVE);
        }
        //root.Add(gameObjectToSpawn);
    }

    private void RenderNewObjectsOnPoints()
    {
        if(GameObjectToSpawn == null)
            return;

        foreach (GameObject currentGameObject in CurrentGameObjects)
        {
            DestroyImmediate(currentGameObject);
        }
        
        foreach (Vector3 vector3 in Points)
        {
            GameObject newGameObject = Instantiate(GameObjectToSpawn.PrefabToSpawn);
            newGameObject.transform.position = vector3;
            CurrentGameObjects.Add(newGameObject);
        }
    }
}
