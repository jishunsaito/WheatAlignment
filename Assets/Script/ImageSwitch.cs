using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageSwitch : MonoBehaviour
{
    // =========================================================
    // 左右別画像
    // =========================================================

    [Serializable]
    public class StereoImagePair
    {
        public string name;
        public Texture2D leftImage;
        public Texture2D rightImage;
    }


    // =========================================================
    // Display
    // =========================================================

    [Header("Display Quads")]
    [SerializeField] private Renderer leftDisplay;
    [SerializeField] private Renderer rightDisplay;


    // =========================================================
    // 左右別画像リスト
    // =========================================================

    [Header("Left / Right Image List")]
    [SerializeField] private StereoImagePair[] imagePairs;


    // =========================================================
    // SBS画像リスト
    // =========================================================

    [Header("SBS Image List")]
    [SerializeField] private Texture2D[] sbsImages;


    // =========================================================
    // Settings
    // =========================================================

    [Header("Settings")]

    [Tooltip("開始時に表示するPicture番号")]
    [SerializeField] private int startPicture = 0;

    [Tooltip("最後の画像から最初の画像へループする")]
    [SerializeField] private bool loop = true;

    [Tooltip("キーボード操作を有効にする")]
    [SerializeField] private bool enableKeyboard = true;


    // =========================================================
    // Material
    // =========================================================

    private Material leftMaterial;
    private Material rightMaterial;


    // =========================================================
    // Current State
    // =========================================================

    private int currentPicture = -1;

    /// <summary>
    /// 現在表示しているPicture番号
    /// </summary>
    public int CurrentPicture => currentPicture;


    /// <summary>
    /// 左右別画像の数
    /// </summary>
    public int SeparateCount =>
        imagePairs != null ? imagePairs.Length : 0;


    /// <summary>
    /// SBS画像の数
    /// </summary>
    public int SBSCount =>
        sbsImages != null ? sbsImages.Length : 0;


    /// <summary>
    /// 全Picture数
    /// </summary>
    public int PictureCount =>
        SeparateCount + SBSCount;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        // Display確認
        if (leftDisplay == null || rightDisplay == null)
        {
            Debug.LogError(
                "Left Display または Right Display が設定されていません。"
            );

            enabled = false;
            return;
        }


        // 左右で独立したMaterial Instanceを取得
        leftMaterial = leftDisplay.material;
        rightMaterial = rightDisplay.material;


        // Shaderが必要なプロパティを持っているか確認
        if (!leftMaterial.HasProperty("_Eye") ||
            !rightMaterial.HasProperty("_Eye"))
        {
            Debug.LogWarning(
                "Materialに _Eye プロパティがありません。" +
                "StereoDisplay Shaderを使用してください。"
            );
        }


        // 画像確認
        if (PictureCount == 0)
        {
            Debug.LogWarning(
                "表示する画像が1枚も登録されていません。"
            );

            return;
        }


        // 初期Picture表示
        ShowPicture(startPicture);
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (!enableKeyboard)
            return;

        if (Keyboard.current == null)
            return;


        // -----------------------------------------------------
        // → または Space
        // 次のPicture
        // -----------------------------------------------------

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextPicture();
        }


        // -----------------------------------------------------
        // ←
        // 前のPicture
        // -----------------------------------------------------

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            PreviousPicture();
        }


        // -----------------------------------------------------
        // B
        // 黒画面
        // -----------------------------------------------------

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ClearDisplay();
        }


        // -----------------------------------------------------
        // R
        // 現在のPictureを再表示
        // -----------------------------------------------------

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestoreDisplay();
        }
    }


    // =========================================================
    // Picture表示
    //
    // Picture番号
    //
    // 0 ～ SeparateCount-1
    //      → 左右別画像
    //
    // SeparateCount ～ PictureCount-1
    //      → SBS画像
    // =========================================================

    public void ShowPicture(int pictureNumber)
    {
        if (PictureCount == 0)
        {
            Debug.LogWarning(
                "表示するPictureがありません。"
            );

            return;
        }


        // Picture番号を有効範囲にする
        pictureNumber =
            GetValidPictureNumber(pictureNumber);


        // =====================================================
        // Separate画像
        // =====================================================

        if (pictureNumber < SeparateCount)
        {
            ShowSeparatePicture(pictureNumber);
        }

        // =====================================================
        // SBS画像
        // =====================================================

        else
        {
            // Picture番号からSBS配列の番号へ変換
            int sbsIndex =
                pictureNumber - SeparateCount;

            ShowSBSPicture(sbsIndex);
        }


        // 現在のPicture番号を保存
        currentPicture = pictureNumber;


        Debug.Log(
            $"Picture {currentPicture} / {PictureCount - 1}"
        );
    }


    // =========================================================
    // 左右別画像を表示
    // =========================================================

    private void ShowSeparatePicture(int index)
    {
        if (imagePairs == null ||
            index < 0 ||
            index >= imagePairs.Length)
        {
            Debug.LogWarning(
                $"Separate Image {index} が存在しません。"
            );

            return;
        }


        StereoImagePair pair =
            imagePairs[index];


        if (pair.leftImage == null ||
            pair.rightImage == null)
        {
            Debug.LogWarning(
                $"Separate Image {index} に画像が設定されていません。"
            );

            return;
        }


        // 左画像
        leftMaterial.mainTexture =
            pair.leftImage;


        // 右画像
        rightMaterial.mainTexture =
            pair.rightImage;


        // SBS分割をOFF
        //
        // _Eye = 0
        // → Texture全体を表示

        if (leftMaterial.HasProperty("_Eye"))
        {
            leftMaterial.SetFloat(
                "_Eye",
                0.0f
            );
        }

        if (rightMaterial.HasProperty("_Eye"))
        {
            rightMaterial.SetFloat(
                "_Eye",
                0.0f
            );
        }


        Debug.Log(
            $"Picture {index} : " +
            $"Separate [{index}] " +
            $"({pair.name})"
        );
    }


    // =========================================================
    // SBS画像を表示
    // =========================================================

    private void ShowSBSPicture(int sbsIndex)
    {
        if (sbsImages == null ||
            sbsIndex < 0 ||
            sbsIndex >= sbsImages.Length)
        {
            Debug.LogWarning(
                $"SBS Image {sbsIndex} が存在しません。"
            );

            return;
        }


        Texture2D sbsTexture =
            sbsImages[sbsIndex];


        if (sbsTexture == null)
        {
            Debug.LogWarning(
                $"SBS Image {sbsIndex} が設定されていません。"
            );

            return;
        }


        // 左右Quadに同じSBS画像を渡す

        leftMaterial.mainTexture =
            sbsTexture;

        rightMaterial.mainTexture =
            sbsTexture;


        // Shader側で左右を切り出す
        //
        // _Eye = 1
        // → SBS左半分
        //
        // _Eye = 2
        // → SBS右半分

        if (leftMaterial.HasProperty("_Eye"))
        {
            leftMaterial.SetFloat(
                "_Eye",
                1.0f
            );
        }

        if (rightMaterial.HasProperty("_Eye"))
        {
            rightMaterial.SetFloat(
                "_Eye",
                2.0f
            );
        }


        // 実際のPicture番号
        int pictureNumber =
            SeparateCount + sbsIndex;


        Debug.Log(
            $"Picture {pictureNumber} : " +
            $"SBS [{sbsIndex}] " +
            $"({sbsTexture.name})"
        );
    }


    // =========================================================
    // 次のPicture
    // =========================================================

    public void NextPicture()
    {
        if (PictureCount == 0)
            return;


        ShowPicture(
            currentPicture + 1
        );
    }


    // =========================================================
    // 前のPicture
    // =========================================================

    public void PreviousPicture()
    {
        if (PictureCount == 0)
            return;


        ShowPicture(
            currentPicture - 1
        );
    }


    // =========================================================
    // Picture番号を有効範囲にする
    // =========================================================

    private int GetValidPictureNumber(
        int pictureNumber
    )
    {
        if (PictureCount <= 0)
            return 0;


        // -----------------------------------------------------
        // Loop ON
        // -----------------------------------------------------

        if (loop)
        {
            return (
                (pictureNumber % PictureCount)
                + PictureCount
            ) % PictureCount;
        }


        // -----------------------------------------------------
        // Loop OFF
        // -----------------------------------------------------

        return Mathf.Clamp(
            pictureNumber,
            0,
            PictureCount - 1
        );
    }


    // =========================================================
    // 黒画面
    // =========================================================

    public void ClearDisplay()
    {
        if (leftMaterial == null ||
            rightMaterial == null)
        {
            return;
        }


        // SBS切り出し解除

        if (leftMaterial.HasProperty("_Eye"))
        {
            leftMaterial.SetFloat(
                "_Eye",
                0.0f
            );
        }

        if (rightMaterial.HasProperty("_Eye"))
        {
            rightMaterial.SetFloat(
                "_Eye",
                0.0f
            );
        }


        // 黒Texture

        leftMaterial.mainTexture =
            Texture2D.blackTexture;

        rightMaterial.mainTexture =
            Texture2D.blackTexture;


        Debug.Log(
            $"Display Cleared " +
            $"(Current Picture = {currentPicture})"
        );
    }


    // =========================================================
    // 現在のPictureを再表示
    // =========================================================

    public void RestoreDisplay()
    {
        if (PictureCount == 0)
            return;


        if (currentPicture < 0)
        {
            ShowPicture(0);
        }
        else
        {
            ShowPicture(
                currentPicture
            );
        }
    }


    // =========================================================
    // 左右反転
    // =========================================================

    public void SetHorizontalFlip(bool flip)
    {
        if (leftMaterial == null ||
            rightMaterial == null)
        {
            return;
        }


        float value =
            flip ? 1.0f : 0.0f;


        if (leftMaterial.HasProperty("_FlipX"))
        {
            leftMaterial.SetFloat(
                "_FlipX",
                value
            );
        }

        if (rightMaterial.HasProperty("_FlipX"))
        {
            rightMaterial.SetFloat(
                "_FlipX",
                value
            );
        }
    }


    // =========================================================
    // 左だけ反転
    // =========================================================

    public void SetLeftHorizontalFlip(bool flip)
    {
        if (leftMaterial == null)
            return;


        if (leftMaterial.HasProperty("_FlipX"))
        {
            leftMaterial.SetFloat(
                "_FlipX",
                flip ? 1.0f : 0.0f
            );
        }
    }


    // =========================================================
    // 右だけ反転
    // =========================================================

    public void SetRightHorizontalFlip(bool flip)
    {
        if (rightMaterial == null)
            return;


        if (rightMaterial.HasProperty("_FlipX"))
        {
            rightMaterial.SetFloat(
                "_FlipX",
                flip ? 1.0f : 0.0f
            );
        }
    }
}