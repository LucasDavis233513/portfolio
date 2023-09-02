using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour {
    [SerializeField] GameObject PauseMenu;
    public static bool isGamePaused = false;

    void Awake() {
        PauseMenu.SetActive(false);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame() {
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void loadMenu() {
        Time.timeScale = 1f;
        isGamePaused = false;
        SceneManager.LoadScene("Main Menu");
    }

    public void quit(){
        Debug.Log("QUIT");
        Application.Quit();
    }

    private void PauseGame() {
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isGamePaused = true;

        Cursor.lockState = CursorLockMode.Confined;
    }
}
