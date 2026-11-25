using System;
using System.Collections.Generic;
using System.IO;
using CardScanner;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnimalGyaruMix
{
    public static class MixSceneAuthor
    {
        private const string Root = "Assets/AnimalGyaruMix";
        private const string ScenePath = "Assets/Scenes/AnimalGyaruMix.unity";
        private static readonly Color Ink = Hex("#321E3C");
        private static readonly Color Accent = Hex("#A23768");
        private static readonly Color Paper = Hex("#FFF2E7");
        private static readonly Color Panel = Hex("#F9DEE8");
        private static readonly Color Lavender = Hex("#E8DCF7");
        private static readonly Color Cream = Hex("#FFFBF3");
        private static Font font;

        [MenuItem("Tools/Animal Gyaru Mix/Create Playable Scene")]
        public static void Generate()
        {
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var catalog = CreateCatalog();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.orthographic = true;
            camera.gameObject.AddComponent<AudioListener>();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var scanner = new GameObject("QRManager").AddComponent<QRManager>();
            Set(scanner, serialized =>
            {
                serialized.FindProperty("startCameraAutomatically").boolValue = false;
                serialized.FindProperty("cameraRect").rectValue = new Rect(.10f, .235f, .80f, .80f);
            });
            var game = new GameObject("Gyaru Builder").AddComponent<GyaruBuilder>();
            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.layer = 5;
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            var shell = Block(canvas.transform, "Game Screens", 0, 0, 1, 1, Paper);
            var ui = shell.gameObject.AddComponent<BuilderUI>();

            Label(shell, "Game Title", "Animal Gyaru Mix", .055f, .914f, .945f, .975f, 80, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Block(shell, "Header Divider", .055f, .902f, .945f, .905f, Hex("#DBA6C2"));
            var nav = Rect(shell, "Navigation", .055f, .035f, .945f, .105f);
            var tabs = new[]
            {
                Btn(nav, "Builder Tab", "Builder", 0, 0, .239f, 1, Accent),
                Btn(nav, "Book Tab", "Card Book", .254f, 0, .493f, 1, Ink),
                Btn(nav, "Album Tab", "Album", .508f, 0, .747f, 1, Ink),
                Btn(nav, "Goals Tab", "Goals", .762f, 0, 1, 1, Ink)
            };
            RectTransform Page(string name) => Rect(shell, name, .055f, .128f, .945f, .884f);

            var builder = Page("Builder Panel");
            var character = Preview(builder, "Character Preview", .04f, .478f, .96f, .962f, Lavender, false);
            var count = Label(builder, "Filled Slot Count", "0 / 6 equipped", .10f, .451f, .90f, .499f, 40, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            var feedback = Label(builder, "Scan Feedback", "Scan a card to style your gyaru.", 0, .385f, 1, .454f, 37, Ink);
            var rarityEffect = Label(builder, "Rarity Effect", "", .12f, .841f, .88f, .939f, 58, Accent, TextAnchor.MiddleCenter, FontStyle.Bold);
            var slots = Rect(builder, "Equipment Slots", 0, .157f, 1, .384f);
            var slotLabels = new Text[6];
            string[] slotNames = { "Ears", "Tail", "Markings", "Top", "Bottom", "Accessory" };
            for (int i = 0; i < slotNames.Length; i++)
            {
                float x = i < 3 ? 0 : .512f;
                float y = (2 - i % 3) / 3f;
                var tile = Block(slots, slotNames[i] + " Slot", x, y + .027f, x + .488f, y + .31f, i < 3 ? Lavender : Panel);
                Label(tile, "Slot Label", slotNames[i], .055f, .47f, .96f, .93f, 35, Accent, TextAnchor.MiddleLeft, FontStyle.Bold);
                slotLabels[i] = Label(tile, "Equipped Name", "Empty", .055f, .040f, .96f, .54f, 42, Ink, TextAnchor.MiddleLeft);
            }
            var scan = Btn(builder, "Open Scanner", "Scan a card", 0, .066f, .62f, .142f, Accent);
            var finish = Btn(builder, "Finish Look", "Name & save", .638f, .066f, 1, .142f, Ink);
            var reset = Btn(builder, "Reset Look", "Reset look", .36f, .002f, .64f, .055f, Ink);

            var book = Page("Card Book Panel");
            Label(book, "Title", "Card Book", 0, .862f, 1, .99f, 72, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var bookHeading = Label(book, "Discovered Count", "0 / 30 discovered", 0, .788f, 1, .862f, 46, Accent, TextAnchor.MiddleLeft);
            var filters = new[]
            {
                Btn(book, "Common Filter", "Common", 0, .694f, .32f, .774f, Accent),
                Btn(book, "Rare Filter", "Rare", .34f, .694f, .66f, .774f, Ink),
                Btn(book, "Super Rare Filter", "Super Rare", .68f, .694f, 1, .774f, Ink)
            };
            var bookPaper = Block(book, "Card List Background", 0, .127f, 1, .665f, Panel);
            var bookList = Label(bookPaper, "Card List", "", .054f, .035f, .955f, .965f, 45, Ink, TextAnchor.MiddleLeft);
            var bookPrevious = Btn(book, "Previous Page", "Previous", 0, .028f, .30f, .105f, Ink);
            var bookNext = Btn(book, "Next Page", "Next", .70f, .028f, 1, .105f, Ink);
            var bookNumber = Label(book, "Book Page", "", .32f, .028f, .68f, .105f, 44, Accent);

            var album = Page("Album Panel");
            Label(album, "Title", "Gyaru Album", 0, .862f, 1, .99f, 72, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var albumSummary = Label(album, "Album Summary", "Your saved gyaru will appear here.", 0, .774f, 1, .866f, 45, Accent, TextAnchor.MiddleLeft);
            var rows = new Button[GyaruBuilder.AlbumLimit];
            for (int i = 0; i < rows.Length; i++)
            {
                float y = .68f - .087f * i;
                rows[i] = Btn(album, "Saved Portrait " + (i + 1), "", .024f, y, .976f, y + .073f, i % 2 == 0 ? Ink : Accent);
            }

            var achievements = Page("Achievements Panel");
            Label(achievements, "Title", "Achievements", 0, .862f, 1, .99f, 72, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var goalPaper = Block(achievements, "Achievement Background", 0, .123f, 1, .815f, Lavender);
            var goalList = Label(goalPaper, "Achievement List", "", .057f, .04f, .95f, .95f, 47, Ink, TextAnchor.MiddleLeft);

            var photo = Page("Photo Panel");
            var photoHeading = Label(photo, "Title", "Name your gyaru", 0, .868f, 1, .994f, 66, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var portrait = Preview(photo, "Photo Portrait", .10f, .368f, .90f, .867f, Lavender, true);
            var status = Label(photo, "Photo Status", "", 0, .31f, 1, .367f, 36, Ink);
            var name = Input(photo, "Gyaru Name", 0, .216f, 1, .300f);
            var useCamera = Btn(photo, "Use Camera", "Camera backdrop", 0, .121f, 1, .198f, Ink);
            var save = Btn(photo, "Save to Album", "Save to album", .306f, .031f, 1, .107f, Accent);
            var photoBack = Btn(photo, "Back to Builder", "Back", 0, .031f, .286f, .107f, Ink);
            var saved = Page("Saved Portrait Panel");
            var savedName = Label(saved, "Portrait Name", "", 0, .867f, 1, .99f, 68, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var savedPortrait = Preview(saved, "Album Portrait", .07f, .280f, .93f, .841f, Lavender, true);
            var edit = Btn(saved, "Load in Builder", "Load in builder", 0, .155f, 1, .239f, Accent);
            var delete = Btn(saved, "Delete Portrait", "Delete", .489f, .046f, 1, .13f, Ink);
            var savedBack = Btn(saved, "Back to Album", "Album", 0, .046f, .47f, .13f, Ink);

            var cameraPanel = Block(shell, "Camera Panel", 0, 0, 1, 1, Ink);
            var cameraHeading = Label(cameraPanel, "Title", "Scan a card", .065f, .85f, .935f, .955f, 78, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            var cameraStatus = Label(cameraPanel, "Camera Status", "", .08f, .775f, .92f, .85f, 43, Cream);
            Block(cameraPanel, "Camera Frame Placeholder", .10f, .311f, .90f, .766f, Hex("#463153"));
            Label(cameraPanel, "Camera Frame Label", "Camera preview", .10f, .48f, .90f, .58f, 54, Cream);
            var sampleLabel = Label(cameraPanel, "Sample Card", "", .07f, .23f, .93f, .302f, 43, Cream);
            var sampleEquip = Btn(cameraPanel, "Use Sample Card", "Use sample card", .07f, .136f, .60f, .214f, Accent);
            var sampleNext = Btn(cameraPanel, "Next Sample Card", "Next sample", .62f, .136f, .93f, .214f, Hex("#674B76"));
            var capture = Btn(cameraPanel, "Capture Photo", "Capture", .07f, .136f, .93f, .214f, Accent);
            var cameraBack = Btn(cameraPanel, "Close Camera", "Back", .30f, .047f, .70f, .120f, Hex("#674B76"));

            Set(ui, data =>
            {
                SetArray(data, "pages", new[] { builder.gameObject, book.gameObject, album.gameObject, achievements.gameObject, photo.gameObject, saved.gameObject });
                SetArray(data, "tabs", tabs);
                SetArray(data, "slotLabels", slotLabels);
                SetArray(data, "bookFilters", filters);
                SetArray(data, "albumRows", rows);
                foreach (var item in new (string name, UnityEngine.Object target)[]
                {
                    ("character", character), ("portrait", portrait), ("filledCount", count), ("scanFeedback", feedback),
                    ("rarityEffect", rarityEffect), ("scanButton", scan), ("resetButton", reset), ("finishButton", finish),
                    ("bookHeading", bookHeading), ("bookList", bookList), ("bookPage", bookNumber), ("bookPrevious", bookPrevious),
                    ("bookNext", bookNext), ("albumSummary", albumSummary), ("achievementList", goalList), ("photoHeading", photoHeading),
                    ("photoStatus", status), ("lookName", name), ("useCamera", useCamera), ("saveLook", save), ("photoBack", photoBack),
                    ("savedPortrait", savedPortrait), ("savedName", savedName), ("editSaved", edit), ("deleteSaved", delete),
                    ("savedBack", savedBack), ("cameraPanel", cameraPanel.gameObject), ("cameraHeading", cameraHeading),
                    ("cameraStatus", cameraStatus), ("sampleLabel", sampleLabel), ("nextSample", sampleNext), ("useSample", sampleEquip),
                    ("cameraCapture", capture), ("cameraBack", cameraBack)
                })
                    data.FindProperty(item.name).objectReferenceValue = item.target;
            });
            Set(game, data =>
            {
                data.FindProperty("scanner").objectReferenceValue = scanner;
                data.FindProperty("view").objectReferenceValue = ui;
                SetArray(data, "catalog", catalog);
            });

            foreach (var page in new[] { book, album, achievements, photo, saved, cameraPanel })
                page.gameObject.SetActive(false);
            rarityEffect.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (previous.IsValid() && !previous.isDirty)
                EditorSceneManager.CloseScene(previous, true);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Animal Gyaru Mix authored with {catalog.Count} cards.");
        }

        private static List<CardDefinition> CreateCatalog()
        {
            const string art = Root + "/Art";
            foreach (string file in Directory.GetFiles(art, "*.png"))
            {
                string path = file.Replace('\\', '/');
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            const string output = Root + "/Cards";
            if (!AssetDatabase.IsValidFolder(output))
                AssetDatabase.CreateFolder(Root, "Cards");

            string[] entries = {
                "Ears0|Cat Ears|Common|Cat", "Ears1|Bunny Ears|Common|Bunny", "Tail0|Fox Tail|Rare|Fox",
                "Top0|Sequin Top|Rare", "Bottom0|Glitter Skirt|Rare", "Accessory0|Rhinestone Shades|Rare",
                "Markings0|Tiger Stripes|SuperRare|White Tiger", "Ears2|Fox Ears|Rare|Fox",
                "Ears3|White Tiger Ears|SuperRare|White Tiger", "Ears4|Axolotl Gills|SuperRare|Axolotl",
                "Tail1|Bunny Tail|Common|Bunny", "Tail2|Leopard Tail|Rare|Leopard", "Tail3|White Tiger Tail|SuperRare|White Tiger", "Tail4|Cat Tail|Common|Cat",
                "Markings1|Cat Whiskers|Common|Cat", "Markings2|Leopard Spots|Rare|Leopard", "Markings3|Fox Cheeks|Rare|Fox", "Markings4|Axolotl Stars|SuperRare|Axolotl",
                "Top1|Leopard Crop Top|Rare", "Top2|Lavender Cardigan|Common", "Top3|Midnight Jacket|Rare", "Top4|Blush Cami|Common",
                "Bottom1|Denim Flares|Common", "Bottom2|Violet Pleats|Common", "Bottom3|Rose Mini|Common", "Bottom4|Moon Trousers|SuperRare",
                "Accessory1|Star Choker|Common", "Accessory2|Heart Hair Clip|Common", "Accessory3|Pearl Pendant|Rare", "Accessory4|Golden Bow|SuperRare"
            };
            var catalog = new List<CardDefinition>(entries.Length);
            for (int i = 0; i < entries.Length; i++)
            {
                string[] info = entries[i].Split('|');
                string id = $"CARD:{i + 1:0000}";
                var card = ScriptableObject.CreateInstance<CardDefinition>();
                Set(card, data =>
                {
                    data.FindProperty("cardId").stringValue = id;
                    data.FindProperty("displayName").stringValue = info[1];
                    data.FindProperty("slot").enumValueIndex = (int)Enum.Parse<CardSlot>(info[0][..^1]);
                    data.FindProperty("rarity").enumValueIndex = (int)Enum.Parse<CardRarity>(info[2]);
                    data.FindProperty("animal").stringValue = info.Length > 3 ? info[3] : "";
                    data.FindProperty("sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{art}/{info[0]}.png");
                });
                string path = $"{output}/{i + 1:00} {info[1]}.asset";
                AssetDatabase.CreateAsset(card, path);
                catalog.Add(card);
            }
            return catalog;
        }

        private static CharacterPreview Preview(Transform parent, string name, float x1, float y1, float x2, float y2, Color color, bool photo)
        {
            var root = Block(parent, name, x1, y1, x2, y2, color);
            var holder = Rect(root, "Sprite Layers", .04f, .025f, .96f, .975f);
            var preview = root.gameObject.AddComponent<CharacterPreview>();
            RawImage backdrop = null;
            if (photo)
            {
                backdrop = Rect(root, "Camera Backdrop", 0, 0, 1, 1).gameObject.AddComponent<RawImage>();
                backdrop.raycastTarget = false;
                backdrop.enabled = false;
                backdrop.transform.SetAsFirstSibling();
                var fit = backdrop.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio = 1f;
            }

            Image Layer(string layer)
            {
                var img = Rect(holder, layer, 0, 0, 1, 1).gameObject.AddComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;
                return img;
            }
            var tail = Layer("Tail");
            var ears = Layer("Ears");
            var body = Layer("Base Body");
            body.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Base.png");
            var top = Layer("Top");
            var bottom = Layer("Bottom");
            var marks = Layer("Markings");
            var accessory = Layer("Accessory");
            Image[] slots = { ears, tail, marks, top, bottom, accessory };
            foreach (Image slot in slots)
                slot.enabled = false;
            Set(preview, so =>
            {
                SetArray(so, "slotImages", slots);
                so.FindProperty("photoBackground").objectReferenceValue = backdrop;
            });
            return preview;
        }

        private static InputField Input(Transform parent, string name, float x1, float y1, float x2, float y2)
        {
            var root = Block(parent, name, x1, y1, x2, y2, Cream);
            var graphic = root.GetComponent<Image>();
            graphic.raycastTarget = true;
            var input = root.gameObject.AddComponent<InputField>();
            var text = Label(root, "Input Text", "", .035f, .055f, .965f, .945f, 58, Ink, TextAnchor.MiddleLeft);
            text.supportRichText = false;
            var placeholder = Label(root, "Placeholder", "Enter a name", .035f, .055f, .965f, .945f, 58, Hex("#8D6C88"), TextAnchor.MiddleLeft);
            input.targetGraphic = graphic;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        private static RectTransform Rect(Transform parent, string name, float x1, float y1, float x2, float y2)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x1, y1);
            rect.anchorMax = new Vector2(x2, y2);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform Block(Transform parent, string name, float x1, float y1, float x2, float y2, Color color)
        {
            var rect = Rect(parent, name, x1, y1, x2, y2);
            var img = rect.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rect;
        }

        private static Text Label(Transform parent, string name, string content, float x1, float y1, float x2, float y2, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var text = Rect(parent, name, x1, y1, x2, y2).gameObject.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 17;
            text.resizeTextMaxSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private static Button Btn(Transform parent, string name, string content, float x1, float y1, float x2, float y2, Color color)
        {
            var rect = Block(parent, name, x1, y1, x2, y2, color);
            var img = rect.GetComponent<Image>();
            img.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(.93f, .87f, .91f);
            colors.pressedColor = new Color(.77f, .66f, .77f);
            colors.disabledColor = new Color(.74f, .64f, .70f, .66f);
            button.colors = colors;
            Label(rect, "Button Label", content, .045f, .06f, .955f, .94f, 46, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            return button;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        private static void Set(UnityEngine.Object target, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(target);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray<T>(SerializedObject data, string name, IReadOnlyList<T> values) where T : UnityEngine.Object
        {
            var array = data.FindProperty(name);
            array.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
