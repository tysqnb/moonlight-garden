using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the six Moonlight Garden screens and the WebGL build.
/// Menu: Moonlight Garden -> 1 Build all scenes / 2 Build WebGL.
/// Batch mode: -executeMethod SceneBuilder.BuildScenes / SceneBuilder.BuildWebGL
/// </summary>
public static class SceneBuilder
{
    const float W = 1080f;
    const float H = 1920f;

    static readonly Color Ink = new Color32(0x16, 0x21, 0x3E, 255);
    static readonly Color Cream = new Color32(0xFF, 0xF9, 0xEC, 255);
    static readonly Color Amber = new Color32(0xF4, 0xD0, 0x3F, 255);
    static readonly Color Sage = new Color32(0x77, 0xC9, 0xA3, 255);
    static readonly Color Soft = new Color32(0xE8, 0xEC, 0xF5, 255);
    static readonly Color LineGrey = new Color32(0xC9, 0xD2, 0xE4, 255);
    static readonly Color TextGrey = new Color32(0x64, 0x6F, 0x8A, 255);
    static readonly Color Sand = new Color32(0xC9, 0xB7, 0x9A, 255);
    static readonly Color White = Color.white;

    static Font _font;
    static Font BodyFont
    {
        get
        {
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _font;
        }
    }

    static Sprite _ui, _knob;
    static Sprite UISprite
    {
        get { if (_ui == null) _ui = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); return _ui; }
    }
    static Sprite KnobSprite
    {
        get { if (_knob == null) _knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); return _knob; }
    }

    static Sprite Art(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/{name}.png");
    }

    // 9-slice borders for the two rounded shapes (see make_sprites.py).
    static readonly Dictionary<string, Vector4> SliceBorders = new Dictionary<string, Vector4>
    {
        { "card.png", new Vector4(28f, 28f, 28f, 28f) },
        { "pill.png", new Vector4(40f, 40f, 40f, 40f) },
    };

    static Color Hex(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    // ------------------------------------------------------------------ import

    static void ImportSprites()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            string file = System.IO.Path.GetFileName(path);
            if (SliceBorders.ContainsKey(file))
                importer.spriteBorder = SliceBorders[file];
            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    // ----------------------------------------------------------------- helpers

    static RectTransform Node(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        return rt;
    }

    static RectTransform At(Transform parent, string name, float x, float y, float w, float h)
    {
        RectTransform rt = Node(parent, name);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static Image Img(RectTransform rt, Sprite sprite, Color color, Image.Type type = Image.Type.Simple)
    {
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = type;
        return image;
    }

    static Sprite Shape(bool pill)
    {
        Sprite s = Art(pill ? "pill" : "card");
        return s != null ? s : UISprite;
    }

    /// <summary>Rounded rectangle: optional border plus an inset fill. Returns the fill.</summary>
    static Image Rounded(Transform parent, string name, float x, float y, float w, float h,
                         Color fill, float border, Color borderColor, bool pill = false)
    {
        Sprite shape = Shape(pill);
        if (border > 0f)
        {
            RectTransform outer = At(parent, name + "_border", x, y, w, h);
            Img(outer, shape, borderColor, Image.Type.Sliced);
            RectTransform inner = At(outer, name, border, border, w - border * 2f, h - border * 2f);
            return Img(inner, shape, fill, Image.Type.Sliced);
        }
        return Img(At(parent, name, x, y, w, h), shape, fill, Image.Type.Sliced);
    }

    static Text Label(Transform parent, string name, float x, float y, float w, float h, string text,
                      int size, Color color, TextAnchor anchor = TextAnchor.UpperLeft, bool bold = false)
    {
        RectTransform rt = At(parent, name, x, y, w, h);
        Text label = rt.gameObject.AddComponent<Text>();
        label.font = BodyFont;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = anchor;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        // Labels must never swallow clicks aimed at the control underneath them
        // (this is what stopped "Start reading" and "Read again" from working).
        label.raycastTarget = false;
        return label;
    }

    static Text LabelCentred(Transform parent, string name, float y, float w, float h, string text,
                             int size, Color color, bool bold = false)
    {
        Text label = Label(parent, name, 0f, 0f, w, h, text, size, color, TextAnchor.UpperCenter, bold);
        RectTransform rt = label.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -y);
        return label;
    }

    static Image SpriteAt(Transform parent, string name, float x, float y, float w, float h, string art, Color tint)
    {
        RectTransform rt = At(parent, name, x, y, w, h);
        Image img = Img(rt, Art(art), tint);
        img.raycastTarget = false;
        return img;
    }

    static Button MakeButton(Image graphic)
    {
        Button button = graphic.gameObject.AddComponent<Button>();
        button.targetGraphic = graphic;
        return button;
    }

    /// <summary>White circle, ink outline, centred icon.</summary>
    static Button CircleButton(Transform parent, string name, float x, float y, float size, Sprite icon, out Image iconImage)
    {
        RectTransform outer = At(parent, name, x, y, size, size);
        Img(outer, KnobSprite, Ink);
        RectTransform inner = At(outer, "fill", 5f, 5f, size - 10f, size - 10f);
        Image fill = Img(inner, KnobSprite, White);
        float pad = (size - 10f) * 0.26f;
        RectTransform iconRt = At(inner, "icon", pad, pad, size - 10f - pad * 2f, size - 10f - pad * 2f);
        iconImage = Img(iconRt, icon, Ink);
        return MakeButton(fill);
    }

    static void Dashes(Transform parent, string name, float y, int count, float width, float gap, float height, Color color)
    {
        RectTransform row = At(parent, name, 0f, y, W, height);
        for (int i = 0; i < count; i++)
        {
            RectTransform dash = At(row, "dash" + i, i * (width + gap), 0f, width, height);
            Img(dash, UISprite, color, Image.Type.Sliced).raycastTarget = false;
        }
    }

    // ----------------------------------------------------------------- screens

    class Frame
    {
        public ScreenController controller;
        public RectTransform content;
        public Canvas canvas;
        public Scene scene;
        public string name;
    }

    static Frame NewFrame(string sceneName, int pageIndex)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera", typeof(Camera));
        Camera cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Cream;
        cam.orthographic = true;
        camGo.tag = "MainCamera";

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(W, H);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // Match the height so the portrait stage always fits the browser window;
        // the camera's cream background fills any side bars.
        scaler.matchWidthOrHeight = 1f;

        RectTransform bg = Node(canvasGo.transform, "Background_paper");
        bg.anchorMin = Vector2.zero;
        bg.anchorMax = Vector2.one;
        bg.offsetMin = Vector2.zero;
        bg.offsetMax = Vector2.zero;
        Img(bg, null, Cream).raycastTarget = false;

        Label(canvasGo.transform, "TopBar_title", 44f, 50f, 560f, 50f, "Moonlight Garden", 46, Ink, TextAnchor.MiddleLeft, true);

        Image narIcon;
        Button nar = CircleButton(canvasGo.transform, "Narration_toggle", 807f, 26f, 99f,
                                  Art(pageIndex == 0 ? "icon_play" : "icon_pause"), out narIcon);
        ToggleIcon narToggle = nar.gameObject.AddComponent<ToggleIcon>();
        narToggle.icon = narIcon;
        narToggle.iconOn = Art("icon_pause");
        narToggle.iconOff = Art("icon_play");
        narToggle.state = pageIndex != 0;

        Image sndIcon;
        Button snd = CircleButton(canvasGo.transform, "Sound_toggle", 936f, 26f, 99f, Art("icon_speaker"), out sndIcon);
        ToggleIcon sndToggle = snd.gameObject.AddComponent<ToggleIcon>();
        sndToggle.icon = sndIcon;
        sndToggle.iconOn = Art("icon_speaker");
        sndToggle.iconOff = Art("icon_speaker_off");

        Dashes(canvasGo.transform, "TopBar_separator", 154f, 12, 54f, 36f, 4f, LineGrey);
        Dashes(canvasGo.transform, "BottomBar_separator", 1712f, 12, 54f, 36f, 4f, LineGrey);

        Image backIcon;
        Button back = CircleButton(canvasGo.transform, "Back_button", 45f, 1767f, 99f, Art("icon_back"), out backIcon);
        Image nextIcon;
        Button next = CircleButton(canvasGo.transform, "Next_button", 936f, 1767f, 99f, Art("icon_next"), out nextIcon);

        RectTransform content = Node(canvasGo.transform, "Content");
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        ScreenController controller = canvasGo.AddComponent<ScreenController>();
        controller.pageIndex = pageIndex;
        controller.lanternOff = Art("lantern_off");
        controller.lanternOn = Art("lantern_on");
        controller.backButton = back;
        controller.nextButton = next;
        controller.backIcon = backIcon;
        controller.nextIcon = nextIcon;
        controller.lanterns = new Image[6];
        controller.rings = new Image[6];

        float[] centres = { 349f, 426f, 501f, 576f, 652f, 728f };
        for (int i = 0; i < 6; i++)
        {
            RectTransform ringRt = At(canvasGo.transform, "Lantern_ring_" + (i + 1), centres[i] - 43f, 1773f, 86f, 86f);
            Image ringImg = Img(ringRt, KnobSprite, Sage);
            ringImg.raycastTarget = false;
            ringImg.enabled = false;
            controller.rings[i] = ringImg;

            RectTransform lanternRt = At(canvasGo.transform, "Lantern_" + (i + 1), centres[i] - 34f, 1782f, 68f, 68f);
            Image lanternImg = Img(lanternRt, Art("lantern_off"), White);
            MakeButton(lanternImg);
            LanternButton lb = lanternRt.gameObject.AddComponent<LanternButton>();
            lb.controller = controller;
            lb.index = i;
            controller.lanterns[i] = lanternImg;
        }

        Frame frame = new Frame();
        frame.controller = controller;
        frame.content = content;
        frame.canvas = canvas;
        frame.scene = scene;
        frame.name = sceneName;
        return frame;
    }

    static void Save(Frame f)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.MarkSceneDirty(f.scene);
        EditorSceneManager.SaveScene(f.scene, $"Assets/Scenes/{f.name}.unity");
    }

    static void IllustrationWindow(Transform parent, float x, float y, float w, float h, string label)
    {
        Rounded(parent, "Illustration_window", x, y, w, h, Soft, 3f, LineGrey).raycastTarget = false;
        if (!string.IsNullOrEmpty(label))
            Label(parent, "Illustration_label", x + 25f, y + 22f, w - 50f, 70f, label, 25, Hex("#5C667F"));
    }

    static void HintPill(Transform parent, float x, float y, float w, string icon, string text)
    {
        Rounded(parent, "Hint_pill", x, y, w, 77f, Amber, 5f, Ink, true).raycastTarget = false;
        SpriteAt(parent, "Hint_icon", x + 34f, y + 23f, 32f, 32f, icon, Ink);
        Label(parent, "Hint_text", x + 80f, y + 12f, w - 110f, 54f, text, 30, Ink, TextAnchor.MiddleLeft, true);
    }

    static void ScriptCard(Transform parent, float x, float y, float w, string script)
    {
        Rounded(parent, "Script_card", x, y, w, 284f, White, 4f, Ink).raycastTarget = false;
        Label(parent, "Script_tag", x + 25f, y + 26f, 300f, 30f, "S C R I P T", 23, Ink, TextAnchor.UpperLeft, true);
        Label(parent, "Script_text", x + 25f, y + 75f, w - 60f, 110f, script, 38, Ink);
        SpriteAt(parent, "Script_speaker", x + 25f, y + 212f, 28f, 28f, "icon_speaker", Ink);
        Label(parent, "Script_replay", x + 60f, y + 214f, 460f, 30f, "tap to hear this line again", 25, Hex("#3C4A66"));
    }

    static void HeadingAndScript(Frame f, int page)
    {
        Label(f.content, "Scene_heading", 44f, 176f, 960f, 44f, Headings[page], 30, Hex("#5C667F"), TextAnchor.MiddleLeft, true);
        ScriptCard(f.content, 51f, 276f, 976f, Scripts[page]);
    }

    static readonly string[] Headings =
    {
        "", "SCENE 1 - A SMALL LIGHT WAKES", "SCENE 2 - A DROP FOR THE SEED",
        "SCENE 3 - THE BREEZE ANSWERS", "SCENE 4 - THE FLOWER OPENS", ""
    };

    static readonly string[] Scripts =
    {
        "",
        "Under the moon, a firefly wakes on a wide green leaf.",
        "A snail carries one cool drop of dew across the path.",
        "A little breeze slips between the reeds and lifts the new shoot.",
        "Moonlight, dew and breeze together wake the silver flower.",
        ""
    };

    static readonly string[] ProgressText =
    {
        "",
        "interaction progress: lantern 2 of 6 lights when this page is finished",
        "interaction progress: lantern 3 of 6 lights when this page is finished",
        "interaction progress: lantern 4 of 6 lights when this page is finished",
        "interaction progress: lantern 5 of 6 lights when this page is finished",
        ""
    };

    static readonly string[] AudioText =
    {
        "", "audio: pond at night, quiet water, crickets", "audio: pond at night, quiet water, crickets",
        "audio: wind in the reeds, pond at night", "audio: pond at night, a rising chord", ""
    };

    // ------------------------------------------------------------------ build

    [MenuItem("Moonlight Garden/1 Build all scenes")]
    public static void BuildScenes()
    {
        ImportSprites();

        BuildHome();
        BuildScene1();
        BuildScene2();
        BuildScene3();
        BuildScene4();
        BuildCredits();

        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();
        string[] names = { "Home", "Scene1_Firefly", "Scene2_Snail", "Scene3_Seed", "Scene4_Moonflower", "Credits" };
        foreach (string n in names)
            list.Add(new EditorBuildSettingsScene($"Assets/Scenes/{n}.unity", true));
        EditorBuildSettings.scenes = list.ToArray();

        PlayerSettings.companyName = "Zihao Wang";
        PlayerSettings.productName = "Moonlight Garden";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.colorSpace = ColorSpace.Gamma;

        AssetDatabase.SaveAssets();
        Debug.Log("SceneBuilder: six scenes built and added to the build list.");
    }

    static void BuildHome()
    {
        Frame f = NewFrame("Home", 0);
        Transform c = f.content;

        IllustrationWindow(c, 51f, 240f, 976f, 741f,
            "Illustration: paper-cut night garden, moonlight on the pond, one firefly asleep on a leaf");
        SpriteAt(c, "Home_star", 120f, 300f, 130f, 130f, "star", Amber);
        SpriteAt(c, "Home_leaf", 300f, 700f, 460f, 300f, "leaf_wide", White);
        SpriteAt(c, "Home_firefly", 430f, 470f, 250f, 250f, "firefly", White);

        LabelCentred(c, "Home_title", 1030f, 960f, 110f, "Moonlight Garden", 80, Ink, true);
        LabelCentred(c, "Home_subtitle", 1160f, 960f, 50f,
            "A lantern story about a small light and a sleepy flower", 40, TextGrey);

        Image startImg = Rounded(c, "Start_button", 365f, 1251f, 348f, 114f, Amber, 5f, Ink, true);
        Label(c, "Start_label", 365f, 1284f, 348f, 60f, "Start reading", 42, Ink, TextAnchor.MiddleCenter, true);
        Button start = MakeButton(startImg);
        NavButton startNav = start.gameObject.AddComponent<NavButton>();
        startNav.controller = f.controller;
        startNav.action = 1;

        Rounded(c, "Read_chip", 390f, 1402f, 298f, 63f, White, 3f, Ink, true).raycastTarget = false;
        SpriteAt(c, "Read_chip_icon", 414f, 1423f, 26f, 26f, "icon_play", Ink);
        Label(c, "Read_chip_text", 446f, 1416f, 240f, 36f, "Read to me: on", 27, Ink, TextAnchor.MiddleLeft);

        LabelCentred(c, "Home_tip", 1522f, 960f, 40f,
            "Tip: one tap starts the story, a second tap reads any line aloud", 24, TextGrey);
        Save(f);
    }

    static void BuildScene1()
    {
        Frame f = NewFrame("Scene1_Firefly", 1);
        Transform c = f.content;

        HeadingAndScript(f, 1);
        IllustrationWindow(c, 51f, 606f, 976f, 684f,
            "Illustration: moon low over the pond, firefly rubbing its eyes on a leaf");

        SpriteAt(c, "Scene1_star", 150f, 660f, 150f, 150f, "star", Amber);
        SpriteAt(c, "Scene1_leaf", 250f, 1010f, 440f, 290f, "leaf_wide", White);
        SpriteAt(c, "Scene1_reed", 760f, 950f, 240f, 240f, "reed", White);

        RectTransform glowRt = At(c, "Scene1_glow", 520f, 880f, 420f, 420f);
        Image glow = Img(glowRt, Art("glow"), new Color(1f, 1f, 1f, 0f));
        glow.raycastTarget = false;

        RectTransform fireflyRt = At(c, "Scene1_firefly", 610f, 970f, 250f, 250f);
        Image fireflyImg = Img(fireflyRt, Art("firefly"), White);
        MakeButton(fireflyImg);
        TapToWake wake = fireflyRt.gameObject.AddComponent<TapToWake>();
        wake.firefly = fireflyRt;
        wake.glow = glow;

        Text cheer = Label(c, "Scene1_cheer", 600f, 1290f, 260f, 40f, "good evening!", 28, Sage, TextAnchor.MiddleCenter, true);
        cheer.gameObject.SetActive(false);
        wake.cheerText = cheer.gameObject;

        HintPill(c, 206f, 1339f, 668f, "icon_tap", "Tap the firefly to say good evening");
        LabelCentred(c, "Scene1_progress", 1479f, 960f, 36f, ProgressText[1], 23, TextGrey);
        LabelCentred(c, "Scene1_audio", 1515f, 960f, 36f, AudioText[1], 23, Hex("#8A93A8"));
        Save(f);
    }

    static void BuildScene2()
    {
        Frame f = NewFrame("Scene2_Snail", 2);
        Transform c = f.content;

        HeadingAndScript(f, 2);
        IllustrationWindow(c, 51f, 606f, 976f, 684f,
            "Illustration: snail on a stone, seed asleep in dark soil, dew drop above");

        Image soil = Rounded(c, "Scene2_soil", 300f, 1030f, 300f, 140f, Sand, 6f, Ink);
        soil.raycastTarget = false;

        RectTransform seedRt = At(c, "Scene2_seed", 380f, 990f, 132f, 132f);
        Image seedImg = Img(seedRt, KnobSprite, Ink);
        seedImg.raycastTarget = false;

        SpriteAt(c, "Scene2_snail", 700f, 1120f, 220f, 220f, "snail", White);

        RectTransform shootRt = At(c, "Scene2_shoot", 380f, 880f, 240f, 240f);
        Image shootImg = Img(shootRt, Art("shoot"), White);
        shootImg.raycastTarget = false;

        RectTransform dropRt = At(c, "Scene2_dewdrop", 780f, 980f, 130f, 130f);
        Image dropImg = Img(dropRt, Art("dewdrop"), White);
        DragDrop drag = dropRt.gameObject.AddComponent<DragDrop>();
        drag.drop = dropRt;
        drag.target = seedRt;
        drag.canvas = f.canvas;
        drag.soil = soil;
        drag.shoot = shootImg.gameObject;

        HintPill(c, 236f, 1339f, 607f, "icon_drag", "Drag the dew drop to the seed");
        LabelCentred(c, "Scene2_progress", 1479f, 960f, 36f, ProgressText[2], 23, TextGrey);
        LabelCentred(c, "Scene2_audio", 1515f, 960f, 36f, AudioText[2], 23, Hex("#8A93A8"));
        Save(f);
    }

    static void BuildScene3()
    {
        Frame f = NewFrame("Scene3_Seed", 3);
        Transform c = f.content;

        HeadingAndScript(f, 3);
        IllustrationWindow(c, 51f, 606f, 976f, 684f,
            "Illustration: reeds bent by a breeze, young shoot swaying, leaves in the air");

        RectTransform reedsRt = At(c, "Scene3_reeds", 560f, 860f, 420f, 420f);
        Image reedsImg = Img(reedsRt, Art("reed"), White);
        reedsImg.raycastTarget = false;

        RectTransform shootRt = At(c, "Scene3_shoot", 240f, 940f, 280f, 280f);
        Image shootImg = Img(shootRt, Art("shoot"), White);
        shootImg.raycastTarget = false;

        RectTransform windRt = At(c, "Scene3_wind", 260f, 700f, 384f, 168f);
        Image windImg = Img(windRt, Art("wind"), new Color(LineGrey.r, LineGrey.g, LineGrey.b, 0.9f));
        windImg.raycastTarget = false;
        windImg.gameObject.SetActive(false);

        RectTransform catcher = At(c, "Scene3_swipe_area", 51f, 606f, 976f, 684f);
        Image catcherImg = Img(catcher, null, new Color(0f, 0f, 0f, 0f));
        SwipeDetect swipe = catcher.gameObject.AddComponent<SwipeDetect>();
        swipe.reeds = reedsRt;
        swipe.shoot = shootRt;
        swipe.wind = windImg;
        swipe.canvas = f.canvas;

        HintPill(c, 159f, 1339f, 762f, "icon_swipe", "Swipe across the reeds to call the breeze");
        LabelCentred(c, "Scene3_progress", 1479f, 960f, 36f, ProgressText[3], 23, TextGrey);
        LabelCentred(c, "Scene3_audio", 1515f, 960f, 36f, AudioText[3], 23, Hex("#8A93A8"));
        Save(f);
    }

    static void BuildScene4()
    {
        Frame f = NewFrame("Scene4_Moonflower", 4);
        Transform c = f.content;

        HeadingAndScript(f, 4);
        IllustrationWindow(c, 51f, 606f, 976f, 684f,
            "Illustration: closed moonflower bud with a soft glow, firefly and snail beside it");

        RectTransform glowRt = At(c, "Scene4_glow", 360f, 810f, 460f, 460f);
        Image glow = Img(glowRt, Art("glow"), new Color(1f, 1f, 1f, 0f));
        glow.raycastTarget = false;

        RectTransform trackRt = At(c, "Scene4_ring_track", 420f, 860f, 300f, 300f);
        Img(trackRt, Art("ring"), new Color(0.68f, 0.73f, 0.80f, 1f)).raycastTarget = false;

        RectTransform ringRt = At(c, "Scene4_ring_fill", 420f, 860f, 300f, 300f);
        Image ringFill = Img(ringRt, Art("ring"), Amber);
        ringFill.type = Image.Type.Filled;
        ringFill.fillMethod = Image.FillMethod.Radial360;
        ringFill.fillOrigin = (int)Image.Origin360.Bottom;
        ringFill.fillAmount = 0f;
        ringFill.raycastTarget = false;

        RectTransform budRt = At(c, "Scene4_bud", 470f, 900f, 200f, 200f);
        Image budImg = Img(budRt, Art("moonflower_bud"), White);
        MakeButton(budImg);

        SpriteAt(c, "Scene4_firefly", 170f, 1120f, 150f, 150f, "firefly", White);
        SpriteAt(c, "Scene4_snail", 800f, 1140f, 180f, 180f, "snail", White);

        PressHold hold = budRt.gameObject.AddComponent<PressHold>();
        hold.ringFill = ringFill;
        hold.budImage = budImg;
        hold.glow = glow;
        hold.budClosed = Art("moonflower_bud");
        hold.budOpen = Art("moonflower_open");

        HintPill(c, 187f, 1339f, 706f, "icon_hold", "Press and hold the bud until it opens");
        LabelCentred(c, "Scene4_progress", 1479f, 960f, 36f, ProgressText[4], 23, TextGrey);
        LabelCentred(c, "Scene4_audio", 1515f, 960f, 36f, AudioText[4], 23, Hex("#8A93A8"));
        Save(f);
    }

    static void BuildCredits()
    {
        Frame f = NewFrame("Credits", 5);
        Transform c = f.content;

        IllustrationWindow(c, 51f, 216f, 976f, 486f,
            "Illustration: the opened moonflower, six lanterns lit around the pond");
        SpriteAt(c, "Credits_flower", 430f, 300f, 220f, 220f, "moonflower_open", White);
        SpriteAt(c, "Credits_star", 140f, 260f, 120f, 120f, "star", Amber);

        LabelCentred(c, "Credits_title", 745f, 960f, 100f, "Credits", 70, Ink, true);

        Rounded(c, "Credits_card", 51f, 853f, 976f, 404f, White, 4f, Ink).raycastTarget = false;
        Label(c, "Credits_text", 76f, 886f, 920f, 350f,
            "Story, art direction and wireframes: Zihao Wang\n" +
            "Narration: recorded by the author\n" +
            "Music and ambience: garden recordings, edited by the author\n" +
            "Sound design: crickets, water, paper rustle\n" +
            "Special thanks: the night gardeners of the pond",
            34, Ink);

        Image againImg = Rounded(c, "Read_again", 390f, 1311f, 300f, 105f, Amber, 5f, Ink, true);
        Label(c, "Read_again_label", 390f, 1338f, 300f, 60f, "Read again", 42, Ink, TextAnchor.MiddleCenter, true);
        Button again = MakeButton(againImg);
        NavButton againNav = again.gameObject.AddComponent<NavButton>();
        againNav.controller = f.controller;
        againNav.action = 2;

        LabelCentred(c, "Credits_note", 1480f, 960f, 36f,
            "prefers-reduced-motion: lanterns rest instead of glow", 23, TextGrey);
        Save(f);
    }

    // ------------------------------------------------------------------ webgl

    [MenuItem("Moonlight Garden/2 Build WebGL")]
    public static void BuildWebGL()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.template = "PROJECT:MoonlightGarden";
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.runInBackground = true;

        string outDir = @"D:\UnityProjects\MoonlightGardenWebGL";
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new[]
        {
            "Assets/Scenes/Home.unity",
            "Assets/Scenes/Scene1_Firefly.unity",
            "Assets/Scenes/Scene2_Snail.unity",
            "Assets/Scenes/Scene3_Seed.unity",
            "Assets/Scenes/Scene4_Moonflower.unity",
            "Assets/Scenes/Credits.unity"
        };
        options.locationPathName = outDir;
        options.target = BuildTarget.WebGL;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"SceneBuilder: WebGL build {report.summary.result}, {report.summary.totalSize} bytes -> {outDir}");
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
