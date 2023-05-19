using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PixelManager : MonoBehaviour
{
    [SerializeField] public string pixelMaterial;
    [SerializeField] SpriteRenderer pixelRenderer;
    [SerializeField] BrushManager brushMan;
    [SerializeField] CanvasManager canvasMan;
    [SerializeField] Dictionary<int,Vector3> neighbors;
    float timewatercanmove = 0;
    Vector3 lastPos = Vector3.zero;
    int skip = 1;
    int stillframes = 0;
    // Start is called before the first frame update
    void Start()
    {
        brushMan = GameObject.FindGameObjectWithTag("BrushMan").GetComponent<BrushManager>();
        canvasMan = GameObject.FindGameObjectWithTag("CanvasMan").GetComponent<CanvasManager>();
    }
    void OnEnable()
    {
        canvasMan = GameObject.FindGameObjectWithTag("CanvasMan").GetComponent<CanvasManager>();
        brushMan = GameObject.FindGameObjectWithTag("BrushMan").GetComponent<BrushManager>();
        //Debug.Log("Enabled, new mat= "+pixelMaterial);
        switch (pixelMaterial)
            {
                case "sand":
                {
                    pixelRenderer.color = brushMan.SandColor;
                    break;
                }
                case "water":
                {
                    pixelRenderer.color = brushMan.WaterColor;
                    break;
                }
                case "cement":
                {
                    pixelRenderer.color = brushMan.CementColor;
                    break;
                }
                case "acid":
                {
                    pixelRenderer.color = brushMan.AcidColor;
                    break;
                }
                default:
                {
                    break;
                }
            }
    }
    void Update()
    {
        if (enabled)
        {
            if (lastPos != Vector3.zero && (gameObject.transform.position == lastPos) && skip>=1)
            {
                skip--;
                return;
            }
            bool shouldcheck = canvasMan.IsPointNearBorder(gameObject.transform.position,10);
            List<int> skipi = new List<int>();
            List<int> skipj = new List<int>();
            float[] distances = new float[]{11,11,11,11};
            if (shouldcheck)
            {
                distances = canvasMan.DistanceToBorder(gameObject.transform.position).ToArray();
            }
            if (!pixelMaterial.Equals("water") && !pixelMaterial.Equals("acid"))
            {
                skipj.Add(1);
            }
            //distances = {dx-xmin,dx-xmax,dy-ymin,dy-ymax}
            for (int i = 0;i<=3;i++)
            {
                if (distances[i] < 10)
                {
                    switch(i)
                    {
                        case 0:
                        {
                            skipi.Add(-1);
                            break;
                        }
                        case 1:
                        {
                            //Debug.Log(pixelMaterial+" skipping i=1");
                            skipi.Add(1);
                            break;
                        }
                        case 2:
                        {
                            skipj.Add(-1);
                            break;
                        }
                        case 3:
                        {
                            //Debug.Log(pixelMaterial+" skipping j=1");
                            skipj.Add(1);
                            break;
                        }
                    }
                }
            }
            //Debug.Log("shouldcheck?: "+shouldcheck);
            neighbors = new Dictionary<int, Vector3>();
            int index = 1;
            for(int i = -1;i<=1;i++)
            {
                if(skipi.Contains(i))
                {
                    index++;
                    if (i==-1)
                    {
                        index++;
                    }
                    continue;
                }
                for(int j = -1;j<=1;j++)
                {
                    if(skipj.Contains(j))
                    {
                        index++;
                        continue;
                    }
                    Vector3 point = new Vector3(gameObject.transform.position.x+(i*10),gameObject.transform.position.y+(j*10),0);
                    if (((i == 0) && (j == 0)) || (shouldcheck && !canvasMan.isWithinBounds(point,11)))
                    {
                        index++;
                        continue;
                    }
                    neighbors[index] = point;
                    index++;
                }
            }
            canvasMan.ActivePoints.Remove(gameObject.transform.position);
            switch (pixelMaterial)
                {
                    case "sand":
                    {
                        SandBehavior();
                        break;
                    }
                    case "water":
                    {
                        WaterBehavior();
                        break;
                    }
                    case "cement":
                    {
                        CementBehavior();
                        break;
                    }
                    case "acid":
                    {
                        AcidBehavior();
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            if (enabled)
            {
                if (Vector3.Distance(gameObject.transform.position,lastPos)<1)
                {
                    canvasMan.ActivePixels[gameObject] = gameObject.transform.position;
                    skip = 1;
                    stillframes++;
                }
                else
                {
                    stillframes = 0;
                }
                if (stillframes > 10)
                {
                    skip = 30;
                }
                canvasMan.ActivePoints[gameObject.transform.position] = gameObject;
            }
            lastPos = (Vector3)gameObject.transform.position;
        }
    }
    // Update is called once per frame
    void Update2()
    {
        
    }
    void SandBehavior()
    {
        if (neighbors.Count == 0)
        {
            return;
        }
        List<int> neighIndex = new List<int>{4,1,7};
        foreach (int index in neighbors.Keys.Where(k => neighIndex.Contains(k)).OrderBy(neighIndex.IndexOf))
        {
            Vector3 point = neighbors[index];
            if (IsPositionEmpty(point,false))
            {
                gameObject.transform.position = point;
                break;
            }
        }
        /*foreach (int index in neighIndex)
        {
            if (!neighbors.ContainsKey(index))
            {
                //Debug.Log(transform.position+" does not have n="+index);
                continue;
            }
            
        }*/
    }
    void WaterBehavior()
    {
        if (neighbors.Count == 0)
        {
            return;
        }
        if (Time.time>timewatercanmove)
        {
            List<int> neighIndex = new List<int>{4,1,7,2,8};
            foreach (int index in neighbors.Keys.Where(k => neighIndex.Contains(k)).OrderBy(neighIndex.IndexOf))
            {
                Vector3 point = neighbors[index];
                if (IsPositionEmpty(point,false))
                {
                    gameObject.transform.position = point;
                    break;
                }
            }
            float RandomDelayforWater = Random.Range(0f,0.05f);
            timewatercanmove = Time.time + RandomDelayforWater;
            //SWAP IF UNDER
            if (!neighbors.ContainsKey(6))
            {
                return;
            }
            Vector3 point2 = neighbors[6];
            if (!IsPositionEmpty(point2,false))
            {
                GameObject other = GetGOatPos(point2,2f);
                PixelManager otherpixelManager = other.GetComponent<PixelManager>();
                if (!otherpixelManager.pixelMaterial.Equals("water") && !otherpixelManager.pixelMaterial.Equals("acid"))
                {
                    //Debug.Log("Pass. Above = "+other.GetComponent<PixelManager>().pixelMaterial);
                    //get value otherpos
                    Vector3 otherpos = (Vector3)other.transform.position;
                    //other pos is now my pos
                    other.transform.position = gameObject.transform.position;
                    //my pos is now value otherpos
                    gameObject.transform.position = otherpos;
                    //Remove activepoint[] for other previous pos
                    canvasMan.ActivePoints.Remove(otherpos);
                    //assign new activepoint[] for other
                    canvasMan.ActivePoints[other.transform.position] = other;
                    //Activepx[other] is now its new pos;
                    canvasMan.ActivePixels[other] = other.transform.position;
                }
            }
        }
    }
    void CementBehavior()
    {
        if (neighbors.Count == 0)
        {
            return;
        }
        List<int> neighIndex = new List<int>{4};
        foreach (int index in neighbors.Keys.Where(k => neighIndex.Contains(k)).OrderBy(neighIndex.IndexOf))
        {
            Vector3 point = neighbors[index];
            if (IsPositionEmpty(point,false))
            {
                gameObject.transform.position = point;
                break;
            }
        }
    }
    void AcidBehavior()
    {
        if (neighbors.Count == 0)
        {
            return;
        }
        if (Time.time>timewatercanmove)
        {
            List<int> AcidNeighIndex = new List<int>{1,2,3,4,6,7,8,9};
            foreach (int index in neighbors.Keys.Where(k => AcidNeighIndex.Contains(k)).OrderBy(AcidNeighIndex.IndexOf))
            {
                if (!gameObject.activeInHierarchy)
                {
                    break;
                }
                IsPositionEmpty(neighbors[index],true);
            }
            List<int> neighIndex = new List<int>{4,1,7,2,8};
            foreach (int index in neighbors.Keys.Where(k => neighIndex.Contains(k)).OrderBy(neighIndex.IndexOf))
            {
                Vector3 point = neighbors[index];
                if (IsPositionEmpty(point,false))
                {
                    gameObject.transform.position = point;
                    break;
                }
            }
            float RandomDelayforWater = Random.Range(0f,0.05f);
            timewatercanmove = Time.time + RandomDelayforWater;
            //SWAP IF UNDER BUT NOT YET IMPLEMENTED
            return;
            /* TBI
            if (!neighbors.ContainsKey(6))
            {
                return;
            }   
            Vector3 point2 = neighbors[6];
            if (!IsPositionEmpty(point2,false))
            {
                GameObject other = GetGOatPos(point2,2f);
                if (other.GetComponent<PixelManager>().pixelMaterial.Equals("someacidresistmat"))
                {
                    //get value otherpos
                    Vector3 otherpos = (Vector3)other.transform.position;
                    //other pos is now my pos
                    other.transform.position = gameObject.transform.position;
                    //my pos is now value otherpos
                    gameObject.transform.position = otherpos;
                    //Activepx[other] is now its new pos;
                    canvasMan.ActivePixels[other] = other.transform.position;
                    //Remove activepoint[] for other previous pos
                    canvasMan.ActivePoints.Remove(otherpos);
                    //assign new activepoint[] for other
                    canvasMan.ActivePoints[other.transform.position] = other;
                }
            }
            */ //TBI
        }
    }
    public bool IsPositionEmpty(Vector3 position, bool isAcidEating)
    {
        if (isAcidEating)
        {
            //get other
            GameObject other = GetGOatPos(position,2f);
            if (other is not null && !other.GetComponent<PixelManager>().pixelMaterial.Equals("acid"))
            {
                if(canvasMan.ActivePixels.ContainsKey(other))
                {
                    canvasMan.ActivePoints.Remove(canvasMan.ActivePixels[other]);
                    canvasMan.ActivePixels.Remove(other);
                    other.SetActive(false);
                    canvasMan.ActivePoints.Remove(canvasMan.ActivePixels[gameObject]);
                    canvasMan.ActivePixels.Remove(gameObject);
                    gameObject.SetActive(false);
                }
            }
        }
        bool empty = !canvasMan.ActivePoints.ContainsKey(position);
        return empty;
    }
    public GameObject GetGOatPos(Vector3 position, float radius)
    {
        if (canvasMan.ActivePoints.ContainsKey(position))
        {
            return canvasMan.ActivePoints[position];
        }
        return null;
    }
}
