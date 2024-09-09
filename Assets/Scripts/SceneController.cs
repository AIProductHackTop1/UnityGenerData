using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {
    public ImageSynthesis synth;
    public GameObject[] prefabs;
    public Material[] materials; // Изменено: массив материалов
    public int minObjects = 10;
    public int maxObjects = 50;
    public int trainingImages;
    public int valImages;
    public bool grayscale = false;
    public string path_to_save;
    public bool save = false;
    

    private ShapePool pool;
    private int frameCount = 0;

    // Start is called before the first frame update
    void Start() {
        pool = ShapePool.Create(prefabs);
    }

    // Update is called once per frame
    void Update() {
        if (frameCount < trainingImages + valImages) {
            if (frameCount % 2 == 0) {
                GenerateRandom();
                Debug.Log($"FrameCount: {frameCount}");
                if (save)
                {
                    if (frameCount < trainingImages)
                    {
                        string filename = $"image_{frameCount.ToString().PadLeft(5, '0')}";
                        synth.Save(filename, 512, 512, path_to_save, 2);
                    }
                    else if (frameCount < trainingImages + valImages)
                    {
                        int valFrameCount = frameCount - trainingImages;
                        string filename = $"image_{valFrameCount.ToString().PadLeft(5, '0')}";
                        synth.Save(filename, 512, 512, "captures/val", 2);
                    }
                }
            }
            frameCount++;
        }
    }

    void GenerateRandom() {
        pool.ReclaimAll();
        int objectsThisTime = Random.Range(minObjects, maxObjects);
        for (int i = 0; i < objectsThisTime; i++) {
            // Pick out a prefab
            int prefabIndx = Random.Range(0, prefabs.Length);
            GameObject prefab = prefabs[prefabIndx];

            // Get a shape from the pool
            var shape = pool.Get((ShapeLabel)prefabIndx);
            var newObj = shape.obj;

            // Assign a random material
            int materialIndx = Random.Range(0, materials.Length);
            var newMaterial = materials[materialIndx];
            newObj.GetComponent<Renderer>().material = newMaterial;

            // Position and scale are now fixed
            newObj.transform.position = prefab.transform.position;
            // newObj.transform.rotation = prefab.transform.rotation;
            float zRotation = Random.Range(0, 2) == 0 ? 0f : 180f;
            newObj.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles.x, prefab.transform.rotation.eulerAngles.y, zRotation);
            newObj.transform.localScale = prefab.transform.localScale;
            
        }
        synth.OnSceneChange(grayscale);
    }
}
