using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mosaic : MonoBehaviour
{
	public int size = 49;
	public int coloramount = 2;
	public float deltaTime = 0.5f;
	public float move = 0.05f;
	public bool rainbow;
	
	public Material material;
	public Color32[] colors;
	
	public GameObject spacing1Input;
	public GameObject spacing2Input;
	public GameObject sizeInput;
	public GameObject colorInput;
	public GameObject timeInput;
	public GameObject moveInput;

	Texture2D texture;
	Color32[] fillColorArray;

	float time;
	float spacing1;
	float spacing2;

	// Start is called before the first frame update
	public void Start()
	{
		if (spacing1Input.GetComponent<InputField>().text == "")
			spacing1 = Random.Range(3f, 5f);
		if (spacing2Input.GetComponent<InputField>().text == "")
			spacing2 = Random.Range(3f, 5f);
		Clear();
	}
	void Clear()
	{
		GenerateColors();
		texture = new Texture2D(size, size) { filterMode = FilterMode.Point };
		material.mainTexture = texture;
		fillColorArray = texture.GetPixels32();
		Generate();
	}
	// Update is called once per frame
	void Update()
	{
		time += Time.deltaTime;
		if (time >= deltaTime)
		{
			time -= deltaTime;
			if (spacing1Input.GetComponent<InputField>().text == "")
				spacing1 = Mathf.Clamp(spacing1 + Random.Range(-move, move), 3f, 5f);
			if (spacing2Input.GetComponent<InputField>().text == "")
				spacing2 = Mathf.Clamp(spacing2 + Random.Range(-move, move), 3f, 5f);
			Generate();
		}
	}
	public void Generate()
	{
		spacing1Input.GetComponent<InputField>().placeholder.GetComponent<Text>().text = spacing1.ToString("0.000000");
		spacing2Input.GetComponent<InputField>().placeholder.GetComponent<Text>().text = spacing2.ToString("0.000000");
		int size2 = size - 1;
		for (int x = 0; x <= size2; x++)
			for (int y = 0; y <= size2; y++)
				SetPixel(x, y, 0);
		int index = 0;
		float diagonal1 = 0;
		for (; diagonal1 <= size2; diagonal1 += spacing1)
		{
			int diagonal = (int)diagonal1;
			index++;
			if (index == coloramount)
				index = 1;
			Line(0, diagonal, diagonal, 0, index);
			Line(size2, diagonal, size2 - diagonal, 0, index);
			Line(0, size2 - diagonal, diagonal, size2, index);
			Line(size2, size2 - diagonal, size2 - diagonal, size2, index);
		}
		for (diagonal1 -= size2; diagonal1 <= size2; diagonal1 += spacing2)
		{
			int diagonal = (int)(diagonal1);
			index++;
			if (index == coloramount)
				index = 1;
			Line(size2, diagonal, diagonal, size2, index);
			Line(0, diagonal, size2 - diagonal, size2, index);
			Line(size2, size2 - diagonal, diagonal, 0, index);
			Line(0, size2 - diagonal, size2 - diagonal, 0, index);
		}

		texture.SetPixels32(fillColorArray);
		texture.Apply();
	}
	public void ToggleRainbow()
	{
		rainbow = !rainbow;
	}
	public void Size()
	{
		string text = sizeInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			int newsize = Mathf.Clamp(int.Parse(text), 2, 1000); // Save in new color
			sizeInput.GetComponent<InputField>().text = newsize.ToString();
			if (newsize > 0 && newsize != size)
			{
				size = newsize;
				Start();
			}
		}
	}
	public void SetSpacing1()
	{
		time = 0;
		string text = spacing1Input.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			spacing1 = float.Parse(text);
			if (spacing1 < 1 || 5 < spacing1)
			{
				spacing1 = Mathf.Clamp(float.Parse(text), 1f, 10f); // Save in new color
				spacing1Input.GetComponent<InputField>().text = spacing1.ToString();
			}
		}
	}
	public void SetSpacing2()
	{
		time = 0;
		string text = spacing2Input.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			spacing2 = float.Parse(text);
			if (spacing2 < 1 || 5 < spacing2)
			{
				spacing2 = Mathf.Clamp(float.Parse(text), 1f, 10f); // Save in new color
				spacing2Input.GetComponent<InputField>().text = spacing2.ToString();
			}
		}
	}
	public void SetTime()
	{
		string text = timeInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			deltaTime = Mathf.Clamp(float.Parse(text), 0f, 10f); // Save in new color
			timeInput.GetComponent<InputField>().text = deltaTime.ToString();
		}
	}
	public void SetMove()
	{
		string text = moveInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			move = Mathf.Clamp(float.Parse(text), .01f, 10f); // Save in new color
			moveInput.GetComponent<InputField>().text = move.ToString();
		}
	}
	public void SetColors()
	{
		string text = colorInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			coloramount = Mathf.Clamp(int.Parse(text), 2, 1000);
			colorInput.GetComponent<InputField>().text = coloramount.ToString();
			GenerateColors();
		}
	}
	void GenerateColors()
	{
		int root = (int)Mathf.Pow(coloramount-1, 1/3f);
		colors = new Color32[(root+1)*(root+1)*(root+1)];
		float gridspacing = 1;
		if (root != 0)
			gridspacing = 1f/root;
		int index = 0;
		for (int x = 0; x <= root; x++)
			for (int y = 0; y <= root; y++)
				for (int z = 0; z <= root; z++)
				{
					colors[index] = new Color(x * gridspacing, y * gridspacing, z * gridspacing ,1);
					index++;
				}
		if (rainbow)
			colors = colors.OrderBy(color => System.Guid.NewGuid()).ToArray();
		else if (coloramount == 2)
		{
			colors[1] = Color.white;
			colors[7] = Color.blue;
		}
		else if (coloramount == 4)
		{
			colors[3] = Color.red;
			colors[4] = Color.cyan;
		}
	}
	void SetPixel(int x, int y, int index)
	{
		fillColorArray[x + size * y] = colors[index];
	}
	void Line(int xStart, int yStart, int xEnd, int yEnd, int index = 1)
	{
		int deltaX = Mathf.Abs(xEnd - xStart);
		int signX = xStart < xEnd ? 1 : -1;
		int deltaY = Mathf.Abs(yEnd - yStart);
		int signY = yStart < yEnd ? 1 : -1;
		int error = (deltaX > deltaY ? deltaX : -deltaY) / 2;
		while (true)
		{
			//if (0 <= xStart && xStart < size && 0 <= yStart && yStart < size)
			fillColorArray[xStart + size * yStart] = colors[index];
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
}