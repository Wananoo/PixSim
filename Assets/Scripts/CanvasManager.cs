using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public struct ActPoint
{
    public Vector3 point;
    public GameObject pixel;
    public ActPoint(Vector2 po, GameObject pi)
    {
        point = po;
        pixel = pi;
    }
}
public class CanvasManager : MonoBehaviour
{
    [SerializeField] public Vector2 gridSize;
    [SerializeField] public float cellSize;
    [SerializeField] float cells;
    [SerializeField] GameObject background;
    [SerializeField] GameObject Pixel;
    [SerializeField] public Dictionary<GameObject,Vector3> ActivePixels;
    [SerializeField] public Dictionary<Vector3,GameObject> ActivePoints;
    [SerializeField] ActPoint[] ActivePointsList;
    [SerializeField] public List<GameObject> Pixels;
    [SerializeField] BoxCollider2D planeCollider;
    [SerializeField] BrushManager brushMan;
    List<Vector3> bounds;
    // Start is called before the first frame update
    void Start()
    {
        ActivePixels = new Dictionary<GameObject,Vector3>();
        ActivePoints = new Dictionary<Vector3,GameObject>();
        Pixels = new List<GameObject>();
        gridSize = new Vector2(background.transform.localScale.x,background.transform.localScale.y);
        cells = gridSize.x/10*gridSize.y/10;
        bounds = new List<Vector3>{planeCollider.bounds.min,planeCollider.bounds.max};
        Pool();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO phys
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ActivePointsList = new ActPoint[ActivePoints.Count];
            Debug.Log("Filling.");
            int index = 0;
            foreach(KeyValuePair<Vector3,GameObject> kvp in ActivePoints)
            {
                ActivePointsList[index] = (new ActPoint(kvp.Key,kvp.Value));
                index++;
            }
        }
        List<Vector3> pointstoremove = ActivePoints.Where(pair => !pair.Value.activeInHierarchy)
                                          .Select(pair => pair.Key)
                                          .ToList();
        foreach(Vector3 v3 in pointstoremove)
        {
            ActivePoints.Remove(v3);
        }
    }
    public void Clear()
    {
        foreach (GameObject go in ActivePixels.Keys)
        {
            go.SetActive(false);
        }
        ActivePixels.Clear();
        ActivePoints.Clear();
    }
    public void Paint(Vector3 point, bool draw)
    {
        if (!isWithinBounds(point,100))
        {
            return;
        }
        if (!ActivePoints.ContainsKey(point) && draw)
        {
            GameObject newPixel = GetPooledObject(Pixels);
            newPixel.GetComponent<PixelManager>().pixelMaterial = brushMan.currentMaterial;
            newPixel.transform.position = point;
            newPixel.transform.rotation = Quaternion.identity;
            ActivePixels.Add(newPixel,point);
            ActivePoints.Add(point,newPixel);
            newPixel.SetActive(true);
        }
        else if (ActivePoints.ContainsKey(point) && !draw)
        {
            List<GameObject> pixelsinarea = new List<GameObject>();
            foreach (Vector3 v3 in ActivePoints.Keys)
            {
                if (v3 == point)
                {
                    pixelsinarea.Add(ActivePoints[v3]);
                }
            }
            foreach (GameObject pixel in pixelsinarea)
            {
                pixel.SetActive(false);
                ActivePoints.Remove(ActivePixels[pixel]);
                ActivePixels.Remove(pixel);
            }
        }
    }
    public bool isWithinBounds(Vector3 point, float r)
    {
        if (IsPointNearBorder(point,r))
        {
            return planeCollider.bounds.Contains(point);
        }
        return true;
    }
    public bool IsPointNearBorder(Vector3 point, float distance)
    {
        float dx0 = point.x - bounds[0].x;
        float dx1 = bounds[1].x - point.x;
        float dy0 = point.y - bounds[0].y;
        float dy1 = bounds[1].y - point.y;
        float Dist = Mathf.Min(dx0,dx1, dy0, dy1);
        return Dist <= distance;
    }
    public List<float> DistanceToBorder(Vector3 point)
    {
        float dx0 = point.x - bounds[0].x;
        float dx1 = bounds[1].x - point.x;
        float dy0 = point.y - bounds[0].y;
        float dy1 = bounds[1].y - point.y;
        return new List<float>{dx0,dx1,dy0,dy1};
    }
    void Pool()
    {
        foreach (GameObject go in Pixels)
        {
            Destroy(go);
        }
        Pixels.Clear();
        for(int i = 0; i <= cells; i++)
        {
            GameObject newGo = Instantiate(Pixel);
            newGo.SetActive(false);
            Pixels.Add(newGo);
        }
    }
    GameObject GetPooledObject(List<GameObject> objectPool)
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            if (!objectPool[i].activeInHierarchy && !ActivePixels.ContainsKey(objectPool[i]))
            {
                return objectPool[i];
            }
        }
        return null;
    }
}
