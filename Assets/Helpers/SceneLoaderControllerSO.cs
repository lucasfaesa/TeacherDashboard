using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helpers
{
    [CreateAssetMenu(fileName = "LevelsLoaderController",
        menuName = "ScriptableObjects/LobbyScene/LevelSelector/LevelsLoaderController")]
    public class SceneLoaderControllerSO : ScriptableObject
    {
        public void GoToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}