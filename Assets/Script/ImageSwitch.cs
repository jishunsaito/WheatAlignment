
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageSwitch : MonoBehaviour
{
    [Serializable]
    public class StereoImagePair
    {
        public string name;
        public Texture2D leftImage;
        public Texture2D rightImage;
    }

    [Header("Display Quads")]
    [SerializeField] private Renderer leftDisplay;
    [SerializeField] private Renderer rightDisplay;

    [Header("Stereo Images")]
    [SerializeField] private StereoImagePair[] imagePairs;

    [Header("Settings")]
    [SerializeField] private int startIndex = 0;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool enableKeyboard = true;

    private Material leftMaterial;
    private Material rightMaterial;
    private int currentIndex = -1;

    public int CurrentIndex => currentIndex;
    public int ImageCount => imagePairs?.Length ?? 0;

    private void Start()
    {
        if (leftDisplay == null || rightDisplay == null)
        {
            Debug.LogError(
                "Left/Right Display が設定されていません。");
            enabled = false;
            return;
        }

        // Quadごとに独立したMaterialを使用
        leftMaterial = leftDisplay.material;
        rightMaterial = rightDisplay.material;

        if (ImageCount > 0)
        {
            ShowImage(startIndex);
        }
        else
        {
            Debug.LogWarning("画像が登録されていません。");
        }
    }

    private void Update()
    {
        if (!enableKeyboard) return;

        // キーボードが接続されていない場合
        if (Keyboard.current == null) return;

        // 次の画像
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextImage();
        }

        // 前の画像
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            PreviousImage();
        }
    }

    // 指定した番号の画像を表示
    public void ShowImage(int index)
    {
        if (ImageCount == 0) return;

        if (loop)
        {
            index = ((index % ImageCount)
                     + ImageCount) % ImageCount;
        }
        else
        {
            index = Mathf.Clamp(
                index, 0, ImageCount - 1);
        }

        StereoImagePair pair = imagePairs[index];

        if (pair.leftImage == null ||
            pair.rightImage == null)
        {
            Debug.LogWarning(
                $"Image Pair {index} に未設定の画像があります。");
            return;
        }

        // 左右のQuadへ画像を設定
        leftMaterial.mainTexture = pair.leftImage;
        rightMaterial.mainTexture = pair.rightImage;

        currentIndex = index;

        Debug.Log(
            $"Stereo Image: {currentIndex} / " +
            $"{ImageCount - 1} ({pair.name})");
    }

    public void NextImage()
    {
        ShowImage(currentIndex + 1);
    }

    public void PreviousImage()
    {
        ShowImage(currentIndex - 1);
    }

    // 画像を消す（黒画面）
    public void ClearDisplay()
    {
        if (leftMaterial == null ||
            rightMaterial == null) return;

        leftMaterial.mainTexture = Texture2D.blackTexture;
        rightMaterial.mainTexture = Texture2D.blackTexture;
    }

    // 現在の画像を再表示
    public void RestoreDisplay()
    {
        if (currentIndex >= 0)
        {
            ShowImage(currentIndex);
        }
    }
}
