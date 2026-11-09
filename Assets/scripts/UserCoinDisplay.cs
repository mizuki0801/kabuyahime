
using System;
using System.IO;
using System.Collections;
using TMPro;
using UnityEngine;

public class UserCoinDisplay : MonoBehaviour
{
    [Header("コインを表示するTextMeshPro")]
    [SerializeField] private TMP_Text coinText;

    [Header("表示するコインの種類")]
    [SerializeField] private CoinType coinType = CoinType.Oshi;

    [Header("デバッグ設定")]
    [SerializeField] private bool debugMode = true;

    public enum CoinType
    {
        Investment,
        Oshi
    }

    [Serializable]
    private class UserData
    {
        public int investmentCoins;
        public int oshiCoins;
    }

    private int loadedCoins;
    private string jsonPath;

    private void Start()
    {
        LoadCoins();

        if (debugMode)
        {
            StartCoroutine(CheckTextOverwrite());
        }
    }

    public void LoadCoins()
    {
        jsonPath = Path.Combine(
            Application.streamingAssetsPath,
            "user.json"
        );

        Debug.Log(
            "【JSON読込先】" + jsonPath
        );

        if (!File.Exists(jsonPath))
        {
            Debug.LogError(
                "【JSONエラー】user.jsonが見つかりません: "
                + jsonPath
            );
            return;
        }

        try
        {
            string json = File.ReadAllText(jsonPath);

            Debug.Log(
                "【JSON内容】" + json
            );

            UserData data =
                JsonUtility.FromJson<UserData>(json);

            if (data == null)
            {
                Debug.LogError(
                    "【JSONエラー】データの変換に失敗しました"
                );
                return;
            }

            loadedCoins = coinType == CoinType.Investment
                ? data.investmentCoins
                : data.oshiCoins;

            Debug.Log(
                $"【コイン読込成功】{coinType}: {loadedCoins}"
            );

            UpdateCoinText();
        }
        catch (Exception e)
        {
            Debug.LogError(
                "【JSON読込エラー】" + e
            );
        }
    }

    private void UpdateCoinText()
    {
        if (coinText == null)
        {
            Debug.LogError(
                "【表示失敗】Coin Textが設定されていません！"
            );
            return;
        }

        coinText.text = loadedCoins.ToString("N0");

        Debug.Log(
            $"【表示更新成功】" +
            $"JSON値:{loadedCoins} / " +
            $"表示値:{coinText.text} / " +
            $"対象:{coinText.gameObject.name} / " +
            $"ファイル:{jsonPath}"
        );
    }

    private IEnumerator CheckTextOverwrite()
    {
        if (coinText == null)
        {
            yield break;
        }

        // 他スクリプトのStart処理後に確認
        yield return null;

        // 1秒後にも確認
        yield return new WaitForSeconds(1f);

        string expectedText = loadedCoins.ToString("N0");

        if (coinText.text != expectedText)
        {
            Debug.LogWarning(
                $"【上書き検出】" +
                $"期待値:{expectedText} / " +
                $"現在値:{coinText.text} / " +
                $"対象:{coinText.gameObject.name}"
            );
        }
        else
        {
            Debug.Log(
                $"【表示維持確認】{coinText.gameObject.name}: " +
                coinText.text
            );
        }
    }
}
