using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only helper: Tools > Assignment 4 > Set Up Weapon Pickups
public static class Assignment4Setup
{
    [MenuItem("Tools/Assignment 4/Set Up Weapon Pickups")]
    public static void SetUpPickups()
    {
        GameObject rifle = SetUpPickup("Pickup_Rifle", WeaponType.Rifle, new Vector3(3f, 0.5f, -6f));
        SetUpPickup("Pickup_Shotgun", WeaponType.Shotgun, new Vector3(-3f, 0.5f, -6f));

        Selection.activeGameObject = rifle;
        EditorSceneManager.MarkSceneDirty(rifle.scene);
        Debug.Log("Assignment 4: Pickup_Rifle and Pickup_Shotgun are set up. Press Ctrl+S to save the scene.");
    }

    [MenuItem("Tools/Assignment 4/Set Up Goal Point")]
    public static void SetUpGoalPoint()
    {
        GameObject go = GameObject.Find("GoalPoint");
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "GoalPoint";
            Undo.RegisterCreatedObjectUndo(go, "Create GoalPoint");
        }

        Undo.RecordObject(go.transform, "Place GoalPoint");
        go.transform.position = new Vector3(0f, 1.5f, 12f);
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 3f;

        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null) box = Undo.AddComponent<BoxCollider>(go);
        Undo.RecordObject(box, "Set trigger");
        box.isTrigger = true;

        if (go.GetComponent<GoalPoint>() == null)
            Undo.AddComponent<GoalPoint>(go);

        // Make it red so it stands out
        Material red = AssetDatabase.LoadAssetAtPath<Material>("Assets/_TPS/_CustomMaterials/MAT_Red01.mat");
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (red != null && mr != null)
        {
            Undo.RecordObject(mr, "Set GoalPoint material");
            mr.sharedMaterial = red;
        }

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Assignment 4: GoalPoint is set up. Press Ctrl+S to save the scene.");
    }

    [MenuItem("Tools/Assignment 4/Fix AreaTrigger Size")]
    public static void FixAreaTriggerSize()
    {
        GameObject go = GameObject.Find("AreaTrigger");
        if (go == null)
        {
            Debug.LogWarning("Assignment 4: No object named AreaTrigger found in the open scene.");
            return;
        }

        BoxCollider box = go.GetComponent<BoxCollider>();
        Undo.RecordObject(box, "Fix AreaTrigger size");
        box.size = new Vector3(30f, 3f, 1f);
        box.isTrigger = true;

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Assignment 4: AreaTrigger size set to 30, 3, 1. Press Ctrl+S to save the scene.");
    }

    [MenuItem("Tools/Assignment 4/Add Gun Models To Pickups")]
    public static void AddGunModels()
    {
        AddGunModel("Pickup_Rifle", "Assets/_TPS/Weapons/Rifle_A.fbx");
        AddGunModel("Pickup_Shotgun", "Assets/_TPS/Weapons/ShotGun_A.fbx");
        Debug.Log("Assignment 4: Gun models added to the pickups. Press Ctrl+S to save the scene.");
    }

    static void AddGunModel(string pickupName, string modelPath)
    {
        GameObject pickup = GameObject.Find(pickupName);
        if (pickup == null)
        {
            Debug.LogWarning("Assignment 4: " + pickupName + " not found in the open scene.");
            return;
        }

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning("Assignment 4: Model not found at " + modelPath);
            return;
        }

        // Remove a model added by an earlier run so running it twice doesn't stack guns
        Transform old = pickup.transform.Find("GunModel");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        Undo.RegisterCreatedObjectUndo(model, "Add gun model");
        model.name = "GunModel";
        model.transform.SetParent(pickup.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // The model should only be visual - the pickup's own trigger does the detection
        foreach (Collider c in model.GetComponentsInChildren<Collider>())
            Undo.DestroyObjectImmediate(c);

        // Resize the model so its longest side is about 1 meter, then center it on the pickup
        Bounds b = GetBounds(model);
        float longest = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (longest > 0.0001f)
            model.transform.localScale *= 1f / longest;
        b = GetBounds(model);
        model.transform.position += pickup.transform.position - b.center;

        // Hide the grey cube
        MeshRenderer cube = pickup.GetComponent<MeshRenderer>();
        if (cube != null)
        {
            Undo.RecordObject(cube, "Hide cube");
            cube.enabled = false;
        }

        // Make the trigger cover the whole gun (with a bit of extra room)
        BoxCollider box = pickup.GetComponent<BoxCollider>();
        if (box != null)
        {
            b = GetBounds(model);
            Vector3 s = pickup.transform.lossyScale;
            Undo.RecordObject(box, "Fit trigger to gun");
            box.center = Vector3.zero;
            box.size = new Vector3(
                Mathf.Max(b.size.x / s.x, 1f) * 1.2f,
                Mathf.Max(b.size.y / s.y, 1f) * 1.2f,
                Mathf.Max(b.size.z / s.z, 1f) * 1.2f);
            box.isTrigger = true;
        }

        EditorSceneManager.MarkSceneDirty(pickup.scene);
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static GameObject SetUpPickup(string name, WeaponType type, Vector3 position)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        }

        Undo.RecordObject(go.transform, "Place " + name);
        go.transform.position = position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * 0.5f;

        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null) box = Undo.AddComponent<BoxCollider>(go);
        Undo.RecordObject(box, "Set trigger");
        box.isTrigger = true;

        WeaponPickup pickup = go.GetComponent<WeaponPickup>();
        if (pickup == null) pickup = Undo.AddComponent<WeaponPickup>(go);
        Undo.RecordObject(pickup, "Set pickup type");
        pickup.weaponType = type;
        pickup.ammoAmount = 10;

        return go;
    }
}
