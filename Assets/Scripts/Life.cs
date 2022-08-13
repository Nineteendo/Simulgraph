using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Life : MonoBehaviour
{
	public int distance = 1;
	public int size = 50;
	public bool wrap;
	public bool rainbow;
	public bool random;
	public bool zoom;

	public bool[][] stayAlive = new bool[1][];
	public bool[][] reincarnate = new bool[1][];


	public GameObject sizeInput;
	public GameObject distanceInput;
	public GameObject colorInput;
	public GameObject neighbourToggle;
	public GameObject ColorCycler;
	
	public GameObject settingsScreen;
	public GameObject backgroundUI;
	
	public GameObject generationCounter;
	public GameObject content;
	
	public Material material;
	public Color32[] colors;
	public int coloramount = 2;
	public int selectedcolor = 1;
	
	Texture2D texture;
	Color32[] fillColorArray;
	int generation = 0;
	int[] indexes;
	int[] newindexes;
	float time;

	// Start is called before the first frame update
	public void Start()
	{
		UpdateToggles();
		GenerateColors();
		Clear();
	}
	public void Update()
	{
		if (Input.GetButtonDown("Fire2"))
			CycleColors();
		else if (!settingsScreen.activeSelf)
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				if (0 < position.x && position.x < 1 && 0 < position.y && position.y < 1 && Camera.main.orthographicSize == 0.59f && zoom && 25 < size)
				{
					Camera.main.orthographicSize = 15f / size;
					transform.position = position -= Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
				}
				else
				{
					position = size * Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0, 0, transform.position.z));
					if (0 < position.x && position.x <= size && 0 < position.y && position.y <= size && Mathf.Abs(transform.position.y - position.y / size) / Camera.main.orthographicSize < 0.9f)
					{
						Pixelchange((int)(size - position.x), (int)position.y);
						Apply();
					}
				}
			}
			else if (Input.GetButtonDown("Cancel"))
				Menu();

			if (1 <= time)
				if (Input.GetButton("Fire1") || Input.GetButton("Submit"))
					nextGeneration();
				else
					time = 0;
			else if (0 < time)
				time += Time.deltaTime;
		}
		else if (Input.GetButtonDown("Cancel"))
		{
			settingsScreen.SetActive(false);
			backgroundUI.SetActive(true);
		}
	}
	public void nextGeneration()
	{
		if (time == 0)
			time = Time.deltaTime;
		generation++;
		generationCounter.GetComponent<Text>().text = "Generation: " + generation;
		
		for (int x = 0; x < size; x++)
			for (int y = 0; y < size; y++)
			{
				int index = indexes[x + size * y];
				int[] neighbours = new int[coloramount];
				for (int yDelta = -distance; yDelta <= distance; yDelta++)
				{
					int RelativeY = y + yDelta;
					if (RelativeY < 0)
					{
						if (wrap)
							RelativeY = size + RelativeY % size;
						else
							continue;
					}
					else if (size <= RelativeY)
					{
						if (wrap)
							RelativeY = RelativeY % size;
						else
							continue;
					}
					for (int xDelta = -distance; xDelta <= distance; xDelta++)
					{
						int RelativeX = x + xDelta;
						if (RelativeX < 0)
						{
							if (wrap)
								RelativeX = size + RelativeX % size;
							else
								continue;
						}
						else if (size <= RelativeX)
						{
							if (wrap)
								RelativeX = RelativeX % size;
							else
								continue;
						}
						if (x != RelativeX || y != RelativeY)
							neighbours[indexes[RelativeX + RelativeY * size]]++;
					}
				}
				if (index != 0)
				{
					if (!stayAlive[index - 1][neighbours[index]])
					{
						newindexes[x + size * y] = 0;
						fillColorArray[x + size * y] = colors[0];
					}
				}
				else 
				{
					int newindex = 0;
					int success = 0;
					for (index = 1; index < coloramount; index++)
						if (reincarnate[index - 1][neighbours[index]])
						{
							newindex = index;
							success++;
						}
					if (success == 1)
					{
						newindexes[x + size * y] = newindex;
						fillColorArray[x + size * y] = colors[newindex];
					}
				}
			}

		Apply();
	}
	public void Clear() 
	{
		generation = 0;
		generationCounter.GetComponent<Text>().text = "Generation: " + generation;
		texture = new Texture2D(size, size) { filterMode = FilterMode.Point };
		material.mainTexture = texture;
		fillColorArray = texture.GetPixels32();
		newindexes = new int[size * size];
		for (int x = 0; x < size; x++)
			for (int y = 0; y < size; y++)
				SetPixel(x, y, 0);
		Apply();
	}
	public void RandomGrid()
	{
		GenerateColors();
		if (random)
			RandomToggles();
		generation = 0;
		generationCounter.GetComponent<Text>().text = "Generation: " + generation;
		fillColorArray = texture.GetPixels32();
		newindexes = new int[size * size];
		for (int x = 0; x < size; x++)
			for (int y = 0; y < size; y++)
				SetPixel(x, y, Random.Range(0, coloramount));
		Apply();
	}
	public void Size()
	{
		string text = sizeInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			int newsize = Mathf.Clamp(int.Parse(text), 1, 1024);
			sizeInput.GetComponent<InputField>().text = newsize.ToString();
			if (newsize != size)
			{
				size = newsize;
				Clear();
			}
		}
	}
	public void Distance()
	{
		string text = distanceInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			distance = int.Parse(text);
			UpdateToggles();
		}
	}
	public void SetColors()
	{
		string text = colorInput.GetComponent<InputField>().text;
		if (text != "") // No empty text
		{
			coloramount = Mathf.Clamp(int.Parse(text), 2, 27);
			colorInput.GetComponent<InputField>().text = coloramount.ToString();
			selectedcolor = Mathf.Clamp(selectedcolor, 1, coloramount - 1);
			UpdateToggles();
			GenerateColors();
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
				{
					int index = indexes[x + size * y];
					if (coloramount <= index)
					{
						indexes[x + size * y] = 0;
						index = 0;
					}
					fillColorArray[x + size * y] = colors[index];
				}
			texture.SetPixels32(fillColorArray);
			texture.Apply();
		}
	}
	public void ToggleZoom()
	{
		zoom = !zoom;
		Camera.main.orthographicSize = 0.59f;
		transform.position = new Vector3(0.5f, 0.5f, -1);
	}
	public void CycleColors()
	{
		selectedcolor++;
		if (selectedcolor == coloramount)
			selectedcolor = 1;
		ColorCycler.GetComponent<Image>().color = colors[selectedcolor];
		UpdateToggles();
	}
	void GenerateColors()
	{
		int root = (int)Mathf.Pow(coloramount - 1, 1 / 3f);
		colors = new Color32[(root + 1) * (root + 1) * (root + 1)];
		float gridspacing = 1;
		if (root != 0)
			gridspacing = 1f / root;
		int index = 0;
		for (int x = 0; x <= root; x++)
			for (int y = 0; y <= root; y++)
				for (int z = 0; z <= root; z++)
				{
					colors[index] = new Color(x * gridspacing, y * gridspacing, z * gridspacing, 1);
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
		ColorCycler.GetComponent<Image>().color = colors[selectedcolor];
	}
	void UpdateToggles()
	{
		System.Array.Resize(ref stayAlive, coloramount - 1);
		System.Array.Resize(ref reincarnate, coloramount - 1);
		for (int index = 0; index < stayAlive.Length; index++)
		{
			if (stayAlive[index] == null)
			{
				stayAlive[index] = new bool[] {false, false, true, true, false, false, false, false, false};
				reincarnate[index] = new bool[] {false, false, false, true, false, false, false, false, false};
			}
			System.Array.Resize(ref stayAlive[index], (2 * distance + 1) * (2 * distance + 1));
			System.Array.Resize(ref reincarnate[index], (2 * distance + 1) * (2 * distance + 1));
		}
		showToggles();
	}
	void RandomToggles()
	{
		for (int index = 0; index < stayAlive.Length; index++)
		{
			for (int neighbours = 0; neighbours < (2 * distance + 1) * (2 * distance + 1); neighbours++)
			{
				stayAlive[index][neighbours] = Random.value > 0.5f;
				reincarnate[index][neighbours] = Random.value > 0.5f;
			}
		}
		showToggles();
	}
	void showToggles()
	{
		content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, 50 * stayAlive[selectedcolor - 1].Length);
		foreach (Transform child in content.transform)
			Destroy(child.gameObject);
		for (int neighbours = 0; neighbours < stayAlive[selectedcolor-1].Length; neighbours++)
		{
			int neighbour = neighbours;
			GameObject live = Instantiate(neighbourToggle, content.transform);
			live.GetComponent<RectTransform>().anchoredPosition = new Vector3(152.5f, -25 - neighbours * 50);
			live.GetComponent<Toggle>().onValueChanged.AddListener((value) => ToggleLife(neighbour, value));
			live.GetComponentInChildren<Text>().text = neighbours.ToString();
			live.GetComponent<Toggle>().isOn = stayAlive[selectedcolor-1][neighbours];

			GameObject dead = Instantiate(neighbourToggle, content.transform);
			dead.GetComponent<RectTransform>().anchoredPosition = new Vector3(683, -25 - neighbours * 50);
			dead.GetComponent<Toggle>().onValueChanged.AddListener((value) => ToggleDead(neighbour, value));
			dead.GetComponentInChildren<Text>().text = neighbours.ToString();
			dead.GetComponent<Toggle>().isOn = reincarnate[selectedcolor-1][neighbours];
		}
	}

	void SetPixel(int x, int y, int index)
	{
		newindexes[x + size * y] = index;
		fillColorArray[x + size * y] = colors[index];
	}
	void Pixelchange(int x, int y)
	{
		int index = indexes[x + size * y] == selectedcolor ? 0 : selectedcolor;
		newindexes[x + size * y] = index;
		fillColorArray[x + size * y] = colors[index];
	}
	void Apply()
	{
		indexes = (int[])newindexes.Clone();
		texture.SetPixels32(fillColorArray);
		texture.Apply();
	}

	public void ToggleLife(int neighbours, bool value)
	{
		stayAlive[selectedcolor - 1][neighbours] = value;
	}
	public void ToggleDead(int neighbours, bool value)
	{
		reincarnate[selectedcolor - 1][neighbours] = value;
	}
	public void ToggleRainbow()
	{
		rainbow = !rainbow;
	}
	public void ToggleRandom()
	{
		random = !random;
	}
	public void ToggleWrap()
	{
		wrap = !wrap;
	}
	public void Menu()
	{
		SceneManager.LoadScene("Menu");
	}
}