using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BrushManager : MonoBehaviour
{
    [SerializeField] float brushSize;
    [SerializeField] Vector3 mousePos;
    [SerializeField] Vector3 worldPosition;
    [SerializeField] CanvasManager canvasMan;
    [SerializeField] public string currentMaterial;
    [SerializeField] Button SandButton;
    [SerializeField] public Color SandColor;
    [SerializeField] Color SandColor2;
    [SerializeField] Button WaterButton;
    [SerializeField] public Color WaterColor;
    [SerializeField] Color WaterColor2;
    [SerializeField] Button CementButton;
    [SerializeField] public Color CementColor;
    [SerializeField] Color CementColor2;
    [SerializeField] Button AcidButton;
    [SerializeField] public Color AcidColor;
    [SerializeField] Color AcidColor2;
    [SerializeField] Slider sizeSlider;
    [SerializeField] TextMeshProUGUI sizeText;
    //bool holdingM1 = false;
    //bool holdingM2 = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BrushCR());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeBrushSize()
    {
        float size = sizeSlider.value;
        brushSize = size;
        sizeText.text = size.ToString("0");
    }
    IEnumerator BrushCR()
    {
        while (true)
        {
            if (Input.GetMouseButton(0)||Input.GetMouseButton(1))
            {
                mousePos = Input.mousePosition;
            worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            float gridSize = canvasMan.cellSize;
            float roundedX = Mathf.Round(worldPosition.x / gridSize) * gridSize;
            float roundedY = Mathf.Round(worldPosition.y / gridSize) * gridSize;
            worldPosition = new Vector3(roundedX, roundedY, 0);
            if (Input.GetMouseButton(0))
            {
                for (int x = (int)-brushSize; x < brushSize; x++) 
                {
                    for (int y = (int)-brushSize; y < brushSize; y++) 
                    {
                        //if (x == 0 && y == 0) continue; // Exclude the central cell
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= brushSize) {
                        canvasMan.Paint(new Vector3(worldPosition.x + (x * 10), worldPosition.y + (y * 10), worldPosition.z),true);
                        }
                    }
                }
            }
            if (Input.GetMouseButton(1))
            {
                canvasMan.Paint(worldPosition,false);
                for (int x = (int)-brushSize; x < brushSize+1; x++) 
                {
                    for (int y = (int)-brushSize; y < brushSize+1; y++) 
                    {
                        if (x == 0 && y == 0) continue; // Exclude the central cell
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= brushSize) {
                        canvasMan.Paint(new Vector3(worldPosition.x + (x * 10), worldPosition.y + (y * 10), worldPosition.z),false);
                        }
                    }
                }
            }
            }
            yield return new WaitForSeconds(0.001f); // Wait for 0.1 seconds before checking again
        }
    }
    public void ChangeMaterial(string mat)
    {
        currentMaterial = mat;
        switch (mat)
            {
                case "sand":
                {
                    SandButton.GetComponent<Image>().color = SandColor;
                    WaterButton.GetComponent<Image>().color = WaterColor2;
                    CementButton.GetComponent<Image>().color = CementColor2;
                    AcidButton.GetComponent<Image>().color = AcidColor2;
                    break;
                }
                case "water":
                {
                    SandButton.GetComponent<Image>().color = SandColor2;
                    WaterButton.GetComponent<Image>().color = WaterColor;
                    CementButton.GetComponent<Image>().color = CementColor2;
                    AcidButton.GetComponent<Image>().color = AcidColor2;
                    break;
                }
                case "cement":
                {
                    SandButton.GetComponent<Image>().color = SandColor2;
                    WaterButton.GetComponent<Image>().color = WaterColor2;
                    CementButton.GetComponent<Image>().color = CementColor;
                    AcidButton.GetComponent<Image>().color = AcidColor2;
                    break;
                }
                case "acid":
                {
                    SandButton.GetComponent<Image>().color = SandColor2;
                    WaterButton.GetComponent<Image>().color = WaterColor2;
                    CementButton.GetComponent<Image>().color = CementColor2;
                    AcidButton.GetComponent<Image>().color = AcidColor;
                    break;
                }
            }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(worldPosition, 0.1f);
    }
}
