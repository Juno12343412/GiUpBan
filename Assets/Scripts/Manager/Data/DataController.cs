using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : MonoBehaviour{
    static GameObject _container;
    static GameObject Container {
        get {
            return _container;
        }
    }
    static DataController _instance;
    public static DataController Instance {
        get {
            if (!_instance) {
                _container = new GameObject();
                _container.name = "DataController";
                _instance = _container.AddComponent(typeof(DataController)) as DataController;
                DontDestroyOnLoad(_container);
            }
            return _instance;
        }
    }
    public string GameDataFileName = "DataInform.json";

    public PlayerStats.Player _gameData;
    public PlayerStats.Player gameData {
        get {
            if (_gameData == null) {
                LoadGameData();
                SaveGameData();
            }
            return _gameData;
        }
    }

    public void LoadGameData() {
        string filePath = Application.persistentDataPath + "/" + GameDataFileName;
        Debug.Log(filePath);
        if (File.Exists(filePath)) {
            string FromJsonData = File.ReadAllText(filePath);
            _gameData = JsonUtility.FromJson<PlayerStats.Player>(FromJsonData);
        } else {
            _gameData = new PlayerStats.Player();
        }
    }

    public void SaveGameData() {
        string ToJsonData = JsonUtility.ToJson(gameData);
        string filePath = Application.persistentDataPath + "/" + GameDataFileName;
        File.WriteAllText(filePath, ToJsonData);
    }

    private void OnApplicationPause(bool pause) {
        if (pause) {
            SaveGameData();
        }
    }
}
