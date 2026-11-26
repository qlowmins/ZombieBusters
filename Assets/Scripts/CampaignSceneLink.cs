using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CampaignSceneLink : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
