using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMenu : MonoBehaviour
{
	// Update is called once per frame
	void Update()
	{
	if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
		Menu(); // Return to menu 
	}
	public void Menu()
	{
		SceneManager.LoadScene("Menu"); // Return to menu
	}
}
