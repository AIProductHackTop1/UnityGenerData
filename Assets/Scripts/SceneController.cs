using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {
    public ImageSynthesis synth;
    public int trainingImages;      
    public int valImages;           
    public bool save = false;         
    public string pathToSave;
    public bool grayscale = false;   
    public float prcntOfNumGenObj;
    public Camera[] captureCameras;   
    public GameObject[] prefabs;  
    public Material[] materials;   
    
    private ShapePool pool;    
    private int frameCount = 0;  

    // Start is called before the first frame update
    void Start() {
        pool = ShapePool.Create(prefabs); 
    }

    // Update is called once per frame
    void Update() {
        if (frameCount < trainingImages + valImages) {
            if (frameCount % 5 == 0) {
                GenerateRandom();
                Debug.Log($"FrameCount: {frameCount}");
            
            
            if (save) 
                if (frameCount < trainingImages) {
                    string filename = $"image_{frameCount.ToString().PadLeft(5, '0')}";
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

    // Функция генерации случайных объектов
    void GenerateRandom() {
        pool.ReclaimAll();  // Возврат всех объектов обратно в пул

        // Генерация случайного количества объектов
        int totalPrefabs = prefabs.Length; 
        int minObjects = Mathf.CeilToInt(totalPrefabs * prcntOfNumGenObj); 
        int maxObjects = totalPrefabs - 1;         
        int objectsThisTime = Random.Range(minObjects, maxObjects + 1);  // Генерация количества объектов

        // Перемешиваем массив префабов
        List<GameObject> shuffledPrefabs = new List<GameObject>(prefabs);
        Shuffle(shuffledPrefabs);  // Функция перемешивания

        // Выбираем случайный материал
        int materialIndx = Random.Range(0, materials.Length);
        var selectedMaterial = materials[materialIndx];

        // Генерируем объекты
        for (int i = 0; i < objectsThisTime; i++) {
            GameObject prefab = shuffledPrefabs[i];  // Берем уникальный префаб из перемешанного списка

            // Получаем объект из пула
            var shape = pool.Get((ShapeLabel)i);
            if (shape == null || shape.obj == null) {
                Debug.LogError("Failed to retrieve object from pool.");
                continue;
            }

            var newObj = shape.obj;

            // Присваиваем материал объекту
            var renderer = newObj.GetComponent<Renderer>();
            if (renderer == null) {
                Debug.LogError($"Object {newObj.name} does not have a Renderer component.");
                continue;
            }
            renderer.material = selectedMaterial;

            // Присваиваем позицию, масштаб и поворот из префаба
            newObj.transform.position = prefab.transform.position;
            newObj.transform.rotation = prefab.transform.rotation;
            newObj.transform.localScale = prefab.transform.localScale;
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
