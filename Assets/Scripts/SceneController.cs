using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {
    public ImageSynthesis synth;
    public int trainingImages;      
    public int valImages;           
    public bool save = false;         
    public string pathToSave;
    public float prcntOfNumGenObj; 
    public bool generateAllObjects; 
    public bool randomRotation; 
    public float prcntOfRotation;
    public Vector3 rotationRange; 
    public Camera[] captureCameras;   
    public GameObject[] prefabs;  
    public Material[] materials;

    public float prcntOfNumNails;  // процент генерации nails
    public bool generateAllNails;  // флаг генерации всех nails
    public float prcntOfRotationNails;  // процент для вращения nails
    public Vector3 rotationNailsRange;  // диапазон вращения nails
    public GameObject[] nails;  // массив nails
    public int a;
    public bool grayscale = false;   

    private ShapePool pool;
    private ShapePool nailspool;
    private int frameCount = 0;  

    // Start is called before the first frame update
    void Start() {
        pool = ShapePool.Create(prefabs);
        nailspool = ShapePool.Create(nails);
    }

    // Update is called once per frame
    void Update() {
        if (frameCount < trainingImages + valImages) {
            if (frameCount % 1 == 0) {
                GenerateRandom();
                GenerateRandomNails();  // генерация nails
                Debug.Log($"FrameCount: {frameCount}");
            
                if (save) 
                    if (frameCount < trainingImages) {
                        string filename = $"{a}_image_{frameCount.ToString().PadLeft(5, '0')}";
                        synth.SaveMultipleCameras(filename, captureCameras, 2048, 2048, pathToSave);
                    } else if (frameCount < trainingImages + valImages) {
                        int valFrameCount = frameCount - trainingImages;
                        string filename = $"image_{valFrameCount.ToString().PadLeft(5, '0')}";
                        synth.SaveMultipleCameras(filename, captureCameras, 512, 512, "captures/val");
                    }
            }
            frameCount++;
        }
    }

    // Генерация объектов из массива prefabs
    void GenerateRandom() {
        pool.ReclaimAll();  

        int totalPrefabs = prefabs.Length; 
        int minObjects = Mathf.CeilToInt(totalPrefabs * prcntOfNumGenObj); 
        int maxObjects = totalPrefabs - 1;         
        int objectsThisTime = generateAllObjects ? totalPrefabs : Random.Range(minObjects, maxObjects + 1);

        List<GameObject> shuffledPrefabs = new List<GameObject>(prefabs);
        Shuffle(shuffledPrefabs);  

        int totalRotationObjects = Mathf.CeilToInt(objectsThisTime * prcntOfRotation);
        List<int> rotationIndices = new List<int>(); 

        while (rotationIndices.Count < totalRotationObjects) {
            int randomIndex = Random.Range(0, objectsThisTime);
            if (!rotationIndices.Contains(randomIndex)) {
                rotationIndices.Add(randomIndex);
            }
        }

        int materialIndx = Random.Range(0, materials.Length);
        var selectedMaterial = materials[materialIndx];

        for (int i = 0; i < objectsThisTime; i++) {
            GameObject prefab = shuffledPrefabs[i];

            var shape = pool.Get((ShapeLabel)i);
            if (shape == null || shape.obj == null) {
                Debug.LogError("Failed to retrieve object from pool.");
                continue;
            }

            var newObj = shape.obj;

            var renderer = newObj.GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError($"Object {newObj.name} does not have a Renderer component.");
                continue;
            }
            renderer.material = selectedMaterial;

            newObj.transform.position = prefab.transform.position;
            newObj.transform.localScale = prefab.transform.localScale;

            if (randomRotation && rotationIndices.Contains(i)) {
                Vector3 originalRotation = prefab.transform.rotation.eulerAngles;
                float randomY = Random.Range(-rotationRange.y, rotationRange.y);
                float randomZ = Random.Range(-rotationRange.z, rotationRange.z);
                newObj.transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y + randomY, originalRotation.z + randomZ);
            } else {
                newObj.transform.rotation = prefab.transform.rotation;
            }
        }

        synth.OnSceneChange(grayscale);
    }

    // Генерация объектов из массива nails
    void GenerateRandomNails() {
        nailspool.ReclaimAll();  

        int totalNails = nails.Length; 
        int minNails = Mathf.CeilToInt(totalNails * prcntOfNumNails); 
        int maxNails = totalNails - 1;         
        int nailsThisTime = generateAllNails ? totalNails : Random.Range(minNails, minNails);

        List<GameObject> shuffledNails = new List<GameObject>(nails);
        Shuffle(shuffledNails);  

        int totalRotationNails = Mathf.CeilToInt(nailsThisTime * prcntOfRotationNails);
        List<int> rotationNailsIndices = new List<int>(); 

        while (rotationNailsIndices.Count < totalRotationNails) {
            int randomIndex = Random.Range(0, nailsThisTime);
            if (!rotationNailsIndices.Contains(randomIndex)) {
                rotationNailsIndices.Add(randomIndex);
            }
        }

        for (int i = 0; i < nailsThisTime; i++) {
            GameObject nail = shuffledNails[i];

            var shape = nailspool.Get((ShapeLabel)i);
            if (shape == null || shape.obj == null) {
                Debug.LogError("Failed to retrieve object from pool.");
                continue;
            }

            var newObj = shape.obj;

            var renderer = newObj.GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError($"Object {newObj.name} does not have a Renderer component.");
                continue;
            }

            newObj.transform.position = nail.transform.position;
            newObj.transform.localScale = nail.transform.localScale;

            if (randomRotation && rotationNailsIndices.Contains(i)) {
                Vector3 originalRotation = nail.transform.rotation.eulerAngles;
                float randomY = Random.Range(-rotationNailsRange.y, rotationNailsRange.y);
                float randomZ = Random.Range(-rotationNailsRange.z, rotationNailsRange.z);
                newObj.transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y + randomY, originalRotation.z + randomZ);
            } else {
                newObj.transform.rotation = nail.transform.rotation;
            }
        }

        synth.OnSceneChange(grayscale);
    }

    // Функция для перемешивания списка 
    void Shuffle<T>(List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
