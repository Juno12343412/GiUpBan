using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Manager.View;
using Manager.Sound;
using BackEnd;

public enum Tear : byte
{
    브론즈,
    실버,
    골드,
    다이아몬드,
    마스터,
    NONE = 99
}

[System.Serializable]
public class Rank
{
    public Tear   tearEnum = Tear.NONE;
    public int    point    = 0;
    public string name   = ""; 

    public Rank(Tear _tear, int _point, string _name)
    {
        tearEnum = _tear;
        point    = _point;
        name     = _name;
    }
}

[System.Serializable]
public class RankUI
{
    public Image      tearImage      = null;
    public Text       nameAnTearText = null;
    public Text       pointText      = null;
    public Text       rankText       = null;
    public GameObject boardObject    = null;

    public RankUI(Image _tear = null, Text _nAt = null, Text _point = null, Text _rank = null)
    { 
        tearImage      = _tear  ?? null;
        nameAnTearText = _nAt   ?? null;
        pointText      = _point ?? null;
        rankText       = _rank  ?? null;
    }
}


// Rank
public partial class MainUI : BaseScreen<MainUI>
{
    [Header("Ranking")]
    public List<Rank>   rankList             = new List<Rank>();
    public List<RankUI> rankUIList           = new List<RankUI>();
    public GameObject   rankBoardObject      = null;
    public GameObject   rankBackGroundObject = null;
    public GameObject   rankNullText         = null;

    public Sprite[]     tearImages           = null;

    public int    basePosY  = 0;
    public int    rankCount = 0;
    public string rankUUID  = "";
    public GameObject rankParent = null;

    public override void ShowScreen()
    {
        Debug.Log("ShowScreen");
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        Debug.Log("HideScreen");
        base.HideScreen();
    }

    public void OpenRankUI()
    {
        SoundPlayer.instance.PlaySound("Click");

        rankObject.SetActive(true);
        SetRank();
    }

    public void CloseRankUI()
    {
        rankObject.SetActive(false);
    }

    public void SetRank()
    {
        rankBackGroundObject.SetActive(false);
        rankNullText.SetActive(false);

        Backend.Rank.RankList(bro => { 
            
            if (bro.IsSuccess())
            { 
                Debug.Log("Rank : " + bro); 
                rankUUID = bro.Rows()[0]["uuid"]["S"].ToString();

                if (rankUIList != null)
                {
                    for (int i = 0; i < rankUIList.Count; i++)
                        Destroy(rankUIList[i].boardObject);

                    rankUIList = null;
                }

                SendQueue.Enqueue(Backend.Rank.GetRankByUuid, rankUUID, 100, callback =>
                {
                    if (callback.IsSuccess() && callback.Rows().Count > 0)
                    {
                        Debug.Log("랭킹 변동 : " + callback);

                        rankUIList = new List<RankUI>();
                        rankList = new List<Rank>();
                        rankCount = callback.Rows().Count;

                        for (int i = 0; i < rankCount; i++)
                        {
                            rankList.Add(new Rank(
                                GetPointToRank(Convert.ToInt32(callback.Rows()[i]["score"]["N"].ToString())), // Tear
                                Convert.ToInt32(callback.Rows()[i]["score"]["N"].ToString()),                 // Score
                                callback.Rows()[i]["nickname"]["S"].ToString()                                // Name
                                ));

                            rankUIList.Add(new RankUI());
                            rankUIList[i].boardObject = Instantiate(rankBoardObject, new Vector3(rankBoardObject.transform.position.x, rankBoardObject.transform.position.y, rankBoardObject.transform.position.z), Quaternion.identity, rankParent.transform);

                            rankUIList[i] = new RankUI(
                                rankUIList[i].boardObject.transform.GetChild(1).GetChild(0).GetComponent<Image>(),
                                rankUIList[i].boardObject.transform.GetChild(0).GetComponent<Text>(),
                                rankUIList[i].boardObject.transform.GetChild(2).GetChild(0).GetComponent<Text>(),
                                rankUIList[i].boardObject.transform.GetChild(3).GetComponent<Text>()
                                );

                            rankUIList[i].boardObject = rankParent.transform.GetChild(i).gameObject;
                            rankUIList[i].boardObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(rankBoardObject.transform.position.x, basePosY - (i * 200f));

                            rankUIList[i].tearImage.sprite = tearImages[(int)rankList[i].tearEnum];
                            rankUIList[i].nameAnTearText.text = rankList[i].name;
                            rankUIList[i].pointText.text = rankList[i].point.ToString();
                            rankUIList[i].rankText.text = (i + 1).ToString();
                            rankUIList[i].boardObject.SetActive(true);
                        }

                        rankBackGroundObject.SetActive(true);
                    }
                    else
                    {
                        rankBackGroundObject.SetActive(true);
                        rankNullText.SetActive(true);
                    }
                });
            }
        });
    }

    public Tear GetPointToRank(int _point)
    {
        if (_point >= 0 && _point < 1000)
            return Tear.브론즈;
        else if (_point >= 1000 && _point < 2000)
            return Tear.실버;
        else if (_point >= 2000 && _point < 3000)
            return Tear.골드;
        else if (_point >= 3000 && _point < 4000)
            return Tear.다이아몬드;
        else if (_point >= 4000 && _point < 5000)
            return Tear.마스터;
        else
            return Tear.NONE;
    }
}
