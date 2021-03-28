using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Life : MonoBehaviour
{
    public int distance = 1;
    public bool[] stayAlive = new bool[9];
    public bool[] reincarnate = new bool[9];
    public int size = 50;
    public bool wrap;
    public bool rainbow;
    public bool zoom;
    public Material material;
    public GameObject generationCounter;
    public GameObject sizeInput;
    public GameObject distanceInput;
    public GameObject settingsScreen;
    public GameObject neighbourToggle;
    public GameObject content;
    Texture2D texture;
    Color32[] fillColorArray;
    Color color1 = Color.white;
    Color color2 = Color.black;
    int generation;
    float time;
    // Start is called before the first frame update
    public void Start()
    {
        System.Array.Resize(ref stayAlive, (2 * distance + 1) * (2 * distance + 1));
        System.Array.Resize(ref reincarnate, stayAlive.Length);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, 75 * stayAlive.Length);
        for (int neighbours = 0; neighbours < stayAlive.Length; neighbours++)
        {
            int neighbour = neighbours;
            GameObject live = Instantiate(neighbourToggle, content.transform);
            live.GetComponent<RectTransform>().anchoredPosition = new Vector3(152.5f, -40 - neighbours * 75);
            live.GetComponent<Toggle>().onValueChanged.AddListener((value) => ToggleLife(neighbour, value));
            live.GetComponentInChildren<Text>().text = neighbours.ToString();
            live.GetComponent<Toggle>().isOn = stayAlive[neighbours];

            GameObject dead = Instantiate(neighbourToggle, content.transform);
            dead.GetComponent<RectTransform>().anchoredPosition = new Vector3(683, -40 - neighbours * 75);
            dead.GetComponent<Toggle>().onValueChanged.AddListener((value) => ToggleDead(neighbour, value));
            dead.GetComponentInChildren<Text>().text = neighbours.ToString();
            dead.GetComponent<Toggle>().isOn = reincarnate[neighbours];
        }
        generation = 0;
        generationCounter.GetComponent<Text>().text = "Generation: " + generation;
        texture = new Texture2D(size, size) { filterMode = FilterMode.Point };
        fillColorArray = texture.GetPixels32();
        material.mainTexture = texture;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                PixelOff(x, y);
        texture.SetPixels32(fillColorArray);
        texture.Apply();
    }
    public void ToggleLife(int neighbours, bool value)
    {
        stayAlive[neighbours] = value;
    }
    public void ToggleDead(int neighbours, bool value)
    {
        reincarnate[neighbours] = value;
    }
    public void RandomGrid()
    {
        if (rainbow)
        {
            color1 = (Color32) Random.ColorHSV();
            color2 = (Color32) Random.ColorHSV();
            Color color3 = (color1 - color2) * (color1 - color2);
            if (color3.r + color3.g + color3.b < 0.01)
                color2 += new Color(color2.r < .5f ? 1 : -1, color2.g < .5f ? 1 : -1, color2.g < .5f ? 1 : -1) / 3;
        }
        else
        {
            color1 = Color.white;
            color2 = Color.black;
        }
        generation = 0;
        generationCounter.GetComponent<Text>().text = "Generation: " + generation;
        fillColorArray = texture.GetPixels32();
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                SetPixel(x, y, Random.Range(0, 2) == 0 ? color1: color2);
        texture.SetPixels32(fillColorArray);
        texture.Apply();
    }
    // Update is called once per frame
    public void Size()
    {
        string text = sizeInput.GetComponent<InputField>().text;
        if (text != "") // No empty text
        {
            int newsize = int.Parse(text); // Save in new color
            if (newsize > 0 && newsize != size)
            {
                size = newsize;
                Start();
            }
        }
    }
    public void Distance()
    {
        string text = distanceInput.GetComponent<InputField>().text;
        if (text != "") // No empty text
        {
            distance = int.Parse(text);
            Start();
        }
    }
    public void Update()
    {
        if (Input.GetButtonDown("Fire1") && !settingsScreen.activeSelf && zoom && size > 19)
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (position.x > 0 && position.x < 1 && position.y > 0 && position.y < 1)
            {
                Camera.main.orthographicSize = 10f / size;
                transform.position = position -= Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }
        }
        if (Input.GetButtonUp("Fire1") && !settingsScreen.activeSelf)
        {
            Vector3 position = size * Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0,0,transform.position.z));
            if (position.x > 0 && position.x <= size && position.y > 0 && position.y <= size && Mathf.Abs(transform.position.y - position.y / size) / Camera.main.orthographicSize < 0.9f)
                Pixelchange((int)position.y, (int)(size - position.x));
            texture.SetPixels32(fillColorArray);
            texture.Apply();
        }
        if (time > 0)
            time += Time.deltaTime;
        if (time > 3)
            if (Input.GetButton("Fire1"))
                nextGeneration();
            else
                time = 0;
    }
    public void nextGeneration()
    {
        if (time == 0)
            time = 1;
        generation++;
        generationCounter.GetComponent<Text>().text = "Generation: " + generation;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                int neighbours = 0;
                for (int xDelta = -distance; xDelta <= distance; xDelta++)
                    for (int yDelta = -distance; yDelta <= distance; yDelta++)
                        neighbours += Pixeltest(x + xDelta, y + yDelta);
                if (Pixeltest(x, y) == 1)
                    fillColorArray[x + size * y] = stayAlive[neighbours - 1] ? color2 : color1;
                else
                    fillColorArray[x + size * y] = reincarnate[neighbours] ? color2 : color1;
            }
        texture.SetPixels32(fillColorArray);
        texture.Apply();
    }
    public void ToggleWrap()
    {
        wrap = !wrap;
    }
    public void ToggleZoom()
    {
        zoom = !zoom;
        Camera.main.orthographicSize = 0.59f;
        transform.position = new Vector3(0.5f, 0.5f, -1);
    }
    public void ToggleRainbow()
    {
        rainbow = !rainbow;
    }
    void SetPixel(int x, int y, Color32 color)
    {
        fillColorArray[size * x + y] = color;
    }
    void PixelOn(int x, int y)
    {
        fillColorArray[size*x+y] = color2;
    }
    void Pixelchange(int x, int y)
    {
        fillColorArray[size * x + y] = fillColorArray[size * x + y] == color2 ? color1 : color2;
    }
    int Pixeltest(int x, int y)
    {
        if (x < 0 || x >= size)
            if (wrap)
                x = size - Mathf.Abs(x);
            else
                return 0;
        if (y < 0 || y >= size)
            if (wrap)
                y = size - Mathf.Abs(y);
            else
                return 0;
        return texture.GetPixel(x,y) == color1 ? 0 : 1;
    }
    void Horizontal(int y)
    {
        for (int x = 0; x < size; x++)
            PixelOn(x, y);
    }
    void Vertical(int x)
    {
        for (int y = 0; y < size; y++)
            PixelOn(x, y);
    }
    void Line(int xStart, int yStart, int xEnd, int yEnd)
    {
        int deltaX = Mathf.Abs(xEnd - xStart);
        int signX = xStart < xEnd ? 1 : -1;
        int deltaY = Mathf.Abs(yEnd - yStart);
        int signY = yStart < yEnd ? 1 : -1;
        int error = (deltaX > deltaY ? deltaX : -deltaY) / 2;
        while (true)
        {
            PixelOn(xStart, yStart);
            if (xStart == xEnd && yStart == yEnd)
                break;
            int errorBackup = error;
            if (errorBackup > -deltaX)
            {
                error -= deltaY;
                xStart += signX;
            }
            if (errorBackup < deltaY)
            {
                error += deltaX;
                yStart += signY;
            }
        }
    }
    void PixelOff(int x, int y)
    {
        fillColorArray[size * x + y] = color1;
    }
}
