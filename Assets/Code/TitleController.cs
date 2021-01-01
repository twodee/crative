using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNew() {
      PlayerPrefs.DeleteKey("level");
      SceneManager.LoadScene("BuilderScene");
    }
    
    public void LoadContinue() {
      SceneManager.LoadScene("BuilderScene");
    }
}
