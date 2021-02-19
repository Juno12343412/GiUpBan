using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager.Scene
{
    public enum Scene
    {
        Login,
        Tutorial,
        Main,
        Loading,
        Game,
        NONE = 99
    }

    public static class Loader
    {
        private class  LoadingMonoBehaviour : MonoBehaviour { }
        private static AsyncOperation asyncOperation;
        private static Action onLoaderCallback;

        public static void Load(Scene scene)
        {
            onLoaderCallback = () =>
            {
                GameObject loadingGameObject = new GameObject("Loading Game Object");
                loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
            };
            SceneManager.LoadScene(Scene.Loading.ToString());
        }

        private static IEnumerator LoadSceneAsync(Scene scene)
        {
            asyncOperation = SceneManager.LoadSceneAsync(scene.ToString());

            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            Debug.Log("isDone");
        }

        public static float GetLoadingProgress()
        {
            if (asyncOperation != null)
                return asyncOperation.progress;
            else
                return 1f;
        }

        public static void LoaderCallback()
        {
            if (onLoaderCallback != null)
            {
                onLoaderCallback();
                onLoaderCallback = null;
            }
        }
    }
}