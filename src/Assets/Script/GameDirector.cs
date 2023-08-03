using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameDirector : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject prefabMessage = default!;
    [SerializeField] GameObject gameObjectCanvas = default!;
    [SerializeField] PlayerDirector playDirector = default!;
    GameObject _message = null;

    
    void Start()
    {
        StartCoroutine("GameFlow");
    }

    private IEnumerator GameFlow()
    {
        CreateMessage("Ready?");
        yield return new WaitForSeconds(1.0f);
        Destroy(_message); _message = null;

        playDirector.EnableSpawn(true);

        while(!playDirector.IsGameOver())
        {
            yield return null;
        }
        CreateMessage("Game Over");

        while(!Input.anyKey)
        {
            yield return null;

        }
        
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("TitleScene");
    }
   
    void CreateMessage(string message)
    {
        Debug.Assert(_message == null);
        _message = Instantiate(prefabMessage, Vector3.zero, Quaternion.identity,
            gameObjectCanvas.transform);
        _message.transform.localPosition = new Vector3(0, 0, 0);

        _message.GetComponent<TextMeshProUGUI>().text = message;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
