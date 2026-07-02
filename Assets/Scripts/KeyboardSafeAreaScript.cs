using UnityEngine;

public class KeyboardSafeAreaScript : MonoBehaviour
{
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float additionalPadding = 10f;

    private Vector2 originalOffsetMin;

    void Awake()
    {
        Debug.Log("KeyboardSafeAreaScript: Awake called");

        if (safeArea == null) safeArea = transform as RectTransform;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        originalOffsetMin = safeArea.offsetMin;
    }

    public int GetKeyboardSize()
    {
        using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

            using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
            {
                View.Call("getWindowVisibleDisplayFrame", Rct);

                return Screen.height - Rct.Call<int>("height");
            }
        }
    }

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        float keyboardHeight = 0f;

        if (TouchScreenKeyboard.visible)
        {
            keyboardHeight = GetKeyboardSize() / canvas.scaleFactor;
            Debug.Log("KeyboardSafeAreaScript: Keyboard is visible. Area height: " + GetKeyboardSize() + ", Canvas scale factor: " + canvas.scaleFactor + ", Calculated keyboard height: " + keyboardHeight);
        }

        safeArea.offsetMin = new Vector2(
            originalOffsetMin.x,
            originalOffsetMin.y + keyboardHeight + (keyboardHeight > 0f ? additionalPadding : 0f)
        );
#else
        safeArea.offsetMin = originalOffsetMin;
#endif
    }
}
