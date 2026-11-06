using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Restyles the Pause and Game Over menus to match the game's inked
/// wood-and-brass look. Keeps the existing panel GameObjects, so references on
/// GameManager and Button onClick survive. Safe to run repeatedly.
/// </summary>
public static class MenuThemer
{
    const string UI = "Assets/UI/";

    static readonly Color Parchment = new Color32(236, 221, 188, 255);
    static readonly Color Ink       = new Color32(18, 16, 20, 255);
    static readonly Color Dim       = new Color32(10, 8, 12, 165);
    static readonly Color Brass     = new Color32(219, 172, 76, 255);

    const float PAD        = 34f;
    const float BANNER_H   = 108f;
    const float BUTTON_W   = 360f;
    const float BUTTON_H   = 80f;
    const float GAP_BUTTON = 18f;

    [MenuItem("Tiny Kingdom/Theme Pause and Game Over Menus")]
    public static void ThemeMenus()
    {
        Sprite wood   = Load("ui_panel_wood",     new Vector4(56, 56, 56, 56));
        Sprite stone  = Load("ui_panel_stone",    new Vector4(56, 56, 56, 56));
        Sprite banner = Load("ui_banner",         new Vector4(60, 20, 60, 20));
        Sprite btn    = Load("ui_button",         new Vector4(26, 26, 26, 26));
        Sprite btnHi  = Load("ui_button_hover",   new Vector4(26, 26, 26, 26));
        Sprite btnDn  = Load("ui_button_pressed", new Vector4(26, 26, 26, 26));

        if (wood == null || btn == null)
        {
            Debug.LogError("MenuThemer: sprites missing from Assets/UI.");
            return;
        }

        Transform canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogError("MenuThemer: no Canvas in the open scene.");
            return;
        }

        int done = 0;
        Transform pause = FindDeep(canvas, "PausePanel");
        if (pause != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(pause.gameObject, "Theme Pause");
            Style(pause, wood, banner, btn, btnHi, btnDn, "PAUSED", 620f);
            done++;
        }
        else Debug.LogWarning("MenuThemer: PausePanel not found.");

        Transform over = FindDeep(canvas, "GameOverPanel");
        if (over != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(over.gameObject, "Theme GameOver");
            Style(over, stone, banner, btn, btnHi, btnDn, "GAME OVER", 640f);
            done++;
        }
        else Debug.LogWarning("MenuThemer: GameOverPanel not found.");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"MenuThemer: styled {done} panel(s). Save the scene to keep it.");
    }

    // ---------------------------------------------------------------- assets

    static Sprite Load(string file, Vector4 border)
    {
        string path = UI + file + ".png";
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogError("MenuThemer: missing " + path); return null; }

        bool changed = false;
        if (imp.textureType != TextureImporterType.Sprite)
        { imp.textureType = TextureImporterType.Sprite; changed = true; }
        if (imp.spriteImportMode != SpriteImportMode.Single)
        { imp.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (imp.spriteBorder != border)
        { imp.spriteBorder = border; changed = true; }
        if (imp.mipmapEnabled) { imp.mipmapEnabled = false; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed)
        { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (imp.wrapMode != TextureWrapMode.Clamp)
        { imp.wrapMode = TextureWrapMode.Clamp; changed = true; }
        if (changed) imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Transform FindCanvas()
    {
        var canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
            if (c.transform.parent == null) return c.transform;
        return canvases.Length > 0 ? canvases[0].transform : null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform r = FindDeep(root.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    static bool IsButtonLabel(Transform t)
    {
        Transform p = t.parent;
        while (p != null)
        {
            if (p.GetComponent<Button>() != null) return true;
            p = p.parent;
        }
        return false;
    }

    // ---------------------------------------------------------------- layout

    static void Style(Transform panel, Sprite windowSprite, Sprite bannerSprite,
                      Sprite btn, Sprite btnHi, Sprite btnDn,
                      string fallbackTitle, float w)
    {
        var panelRect = (RectTransform)panel;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var dim = panel.GetComponent<Image>();
        if (dim == null) dim = panel.gameObject.AddComponent<Image>();
        dim.sprite = null;
        dim.type = Image.Type.Simple;
        dim.color = Dim;
        dim.raycastTarget = true;

        Transform window = panel.Find("Window");
        if (window == null)
        {
            var go = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window = go.transform;
            window.SetParent(panel, false);
        }
        var winRect = (RectTransform)window;
        winRect.anchorMin = winRect.anchorMax = new Vector2(0.5f, 0.5f);
        winRect.pivot = new Vector2(0.5f, 0.5f);
        winRect.anchoredPosition = Vector2.zero;

        var winImg = window.GetComponent<Image>();
        winImg.sprite = windowSprite;
        winImg.type = Image.Type.Sliced;
        winImg.color = Color.white;
        winImg.raycastTarget = true;

        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Transform c = panel.GetChild(i);
            if (c != window) c.SetParent(window, false);
        }

        // the banner must exist before collecting, and the title may already be
        // inside it from a previous run - so search the whole subtree
        Transform bannerTf = window.Find("TitleBanner");

        var buttons = new List<Button>(window.GetComponentsInChildren<Button>(true));

        TMP_Text title = null;
        var others = new List<TMP_Text>();
        foreach (var t in window.GetComponentsInChildren<TMP_Text>(true))
        {
            if (IsButtonLabel(t.transform)) continue;        // it's a button caption
            string n = t.name.ToLower();
            if (title == null && (n.Contains("paused") || n.Contains("over") ||
                                  n.Contains("title")))
                title = t;
            else
                others.Add(t);
        }

        float cursor = -PAD;

        if (title != null)
        {
            if (bannerTf == null)
            {
                var go = new GameObject("TitleBanner",
                    typeof(RectTransform), typeof(Image));
                bannerTf = go.transform;
                bannerTf.SetParent(window, false);
            }
            bannerTf.SetSiblingIndex(0);
            var br = (RectTransform)bannerTf;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
            br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = new Vector2(0f, cursor);
            br.sizeDelta = new Vector2(w - 56f, BANNER_H);

            var bi = bannerTf.GetComponent<Image>();
            bi.sprite = bannerSprite;
            bi.type = Image.Type.Sliced;
            bi.color = Color.white;
            bi.raycastTarget = false;

            title.transform.SetParent(bannerTf, false);
            var tr = (RectTransform)title.transform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(30f, 14f);
            tr.offsetMax = new Vector2(-30f, -12f);
            tr.anchoredPosition = Vector2.zero;

            if (string.IsNullOrWhiteSpace(title.text)) title.text = fallbackTitle;
            title.text = title.text.ToUpperInvariant();
            StyleText(title, 46f, Parchment, FontStyles.Bold, 0.22f, 24f);
            title.alignment = TextAlignmentOptions.Center;

            cursor -= BANNER_H + 22f;
        }
        else if (bannerTf != null)
        {
            Object.DestroyImmediate(bannerTf.gameObject);
        }

        foreach (var t in others)
        {
            t.transform.SetParent(window, false);
            var r = (RectTransform)t.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = new Vector2(0f, cursor);
            r.sizeDelta = new Vector2(w - 110f, 42f);
            StyleText(t, 28f, Brass, FontStyles.Normal, 0.14f, 18f);
            t.alignment = TextAlignmentOptions.Center;
            cursor -= 48f;
        }
        if (others.Count > 0) cursor -= 10f;

        foreach (var b in buttons)
        {
            b.transform.SetParent(window, false);
            var r = (RectTransform)b.transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.anchoredPosition = new Vector2(0f, cursor);
            r.sizeDelta = new Vector2(BUTTON_W, BUTTON_H);
            cursor -= BUTTON_H + GAP_BUTTON;

            var img = b.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = btn;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            b.transition = Selectable.Transition.SpriteSwap;
            var st = b.spriteState;
            st.highlightedSprite = btnHi;
            st.pressedSprite     = btnDn;
            st.selectedSprite    = btnHi;
            b.spriteState = st;

            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                var lr = (RectTransform)label.transform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = new Vector2(18f, 10f);
                lr.offsetMax = new Vector2(-18f, -12f);
                lr.anchoredPosition = Vector2.zero;
                StyleText(label, 30f, Parchment, FontStyles.Bold, 0.18f, 18f);
                label.alignment = TextAlignmentOptions.Center;
            }
        }
        if (buttons.Count > 0) cursor += GAP_BUTTON;

        float contentHeight = Mathf.Abs(cursor) + PAD;
        winRect.sizeDelta = new Vector2(w, Mathf.Max(230f, contentHeight));
    }

    static void StyleText(TMP_Text t, float size, Color col,
                          FontStyles style, float outline, float minSize)
    {
        t.enableAutoSizing = true;
        t.fontSizeMin = minSize;
        t.fontSizeMax = size;
        t.fontSize = size;
        t.color = col;
        t.fontStyle = style;
        t.characterSpacing = 2f;
        t.margin = Vector4.zero;

        Material m = t.fontMaterial;
        m.EnableKeyword("OUTLINE_ON");
        m.SetFloat(ShaderUtilities.ID_OutlineWidth, outline);
        m.SetColor(ShaderUtilities.ID_OutlineColor, Ink);
    }
}
