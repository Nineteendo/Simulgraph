using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
	public string version; // REAL version
	public int puzzle; // Current puzzle
	public string[] scenes; // Available scenes
	public GameObject[] puzzles; // Puzzle icons

	void Start()
	{
		puzzle = PlayerPrefs.GetInt("puzzle", puzzle); // Get last played puzzle
		SetPuzzle(puzzle); // Move and rescale puzzles
	}
	void Update()
	{
		if (Input.GetButtonDown("Cancel")) // Player is tired of the application ;(
			Quit();
	}
	public void SetPuzzle(int puz) // Set Puzzle
	{
		puzzles[puzzle].transform.localScale = new Vector3(.25f, .25f, .25f); // Rescale old puzzle
		puzzle = puz; // Save puzzle
		puzzles[puzzle].transform.parent.localPosition = new Vector3(0, 64, 0) - puzzles[puzzle].transform.localPosition; // Center selected puzzle
		puzzles[puzzle].transform.localScale = new Vector3(1, 1, 1); // Rescale new puzzle
		PlayerPrefs.SetInt("puzzle", puzzle); // Save new puzzle in player prefs
	}
	public void Play() // Start selected scene
	{
		SceneManager.LoadScene(scenes[puzzle]);
	}
	public void Credits() // Start selected scene
	{
		SceneManager.LoadScene("Credits");
	}
	public void Quit() // Quit the application, when will you play it again?
	{
		Application.Quit();
	}
	void OnApplicationQuit()
	{
		Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.MaximizedWindow);
	}
}
