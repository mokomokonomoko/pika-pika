using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalGyaruMix
{
    public sealed class BuilderUI : MonoBehaviour
    {
        private enum Page { Builder, CardBook, Album, Achievements, Photo, SavedLook }
        private enum CameraPage { None, Scan, Photo }

        [SerializeField] private GameObject[] pages;
        [SerializeField] private Button[] tabs;
        [SerializeField] private CharacterPreview character;
        [SerializeField] private CharacterPreview portrait;
        [SerializeField] private Text[] slotLabels;
        [SerializeField] private Text filledCount;
        [SerializeField] private Text scanFeedback;
        [SerializeField] private Text rarityEffect;
        [SerializeField] private Button scanButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button finishButton;

        [SerializeField] private Button[] bookFilters;
        [SerializeField] private Text bookHeading;
        [SerializeField] private Text bookList;
        [SerializeField] private Text bookPage;
        [SerializeField] private Button bookPrevious;
        [SerializeField] private Button bookNext;

        [SerializeField] private Text albumSummary;
        [SerializeField] private Button[] albumRows;
        [SerializeField] private Text achievementList;
        [SerializeField] private Text photoHeading;
        [SerializeField] private Text photoStatus;
        [SerializeField] private InputField lookName;
        [SerializeField] private Button useCamera;
        [SerializeField] private Button saveLook;
        [SerializeField] private Button photoBack;
        [SerializeField] private CharacterPreview savedPortrait;
        [SerializeField] private Text savedName;
        [SerializeField] private Button editSaved;
        [SerializeField] private Button deleteSaved;
        [SerializeField] private Button savedBack;

        [SerializeField] private GameObject cameraPanel;
        [SerializeField] private Text cameraHeading;
        [SerializeField] private Text cameraStatus;
        [SerializeField] private Text sampleLabel;
        [SerializeField] private Button nextSample;
        [SerializeField] private Button useSample;
        [SerializeField] private Button cameraCapture;
        [SerializeField] private Button cameraBack;

        private static readonly Color Ink = new(0.17f, 0.10f, 0.20f);
        private static readonly Color Accent = new(0.56f, 0.20f, 0.39f);
        private static readonly Color Gold = new(0.56f, 0.29f, 0.10f);
        private static readonly Color SilverBlue = new(0.23f, 0.46f, 0.69f);
        private const int CardsPerPage = 6;
        private const int RareSparkleCount = 6;
        private const int SuperRareSparkleCount = 12;

        private GyaruBuilder game;
        private Page page;
        private CameraPage camera;
        private CardRarity filter;
        private int bookPageIndex;
        private int albumIndex;
        private int sampleIndex;
        private bool confirmDelete;
        private Coroutine rarityAnimation;
        private RectTransform sparkleLayer;
        private Image[] sparkles;

        public bool IsScanning => camera == CameraPage.Scan;
        public bool IsTakingPhoto => camera == CameraPage.Photo;
        public bool IsCameraOpen => camera != CameraPage.None;
        public bool AcceptsScans => IsScanning;

        public void Initialize(GyaruBuilder builder)
        {
            game = builder;
            for (int i = 0; i < tabs.Length; i++)
            {
                Page destination = (Page)i;
                tabs[i].onClick.AddListener(() => Navigate(destination));
            }

            for (int i = 0; i < bookFilters.Length; i++)
            {
                CardRarity rarity = (CardRarity)i;
                bookFilters[i].onClick.AddListener(() => SelectRarity(rarity));
            }

            for (int i = 0; i < albumRows.Length; i++)
            {
                int index = i;
                albumRows[i].onClick.AddListener(() => OpenAlbumLook(index));
            }

            scanButton.onClick.AddListener(game.OpenScanner);
            resetButton.onClick.AddListener(game.ClearLook);
            finishButton.onClick.AddListener(game.OpenPhoto);
            bookPrevious.onClick.AddListener(() => TurnBookPage(-1));
            bookNext.onClick.AddListener(() => TurnBookPage(1));
            useCamera.onClick.AddListener(game.OpenPhotoCamera);
            useCamera.gameObject.SetActive(game.UsesBrowserCamera);
            saveLook.onClick.AddListener(SaveCurrentLook);
            photoBack.onClick.AddListener(() => Navigate(Page.Builder));
            editSaved.onClick.AddListener(EditCurrentLook);
            deleteSaved.onClick.AddListener(DeleteCurrentLook);
            savedBack.onClick.AddListener(() => Navigate(Page.Album));
            cameraCapture.onClick.AddListener(game.TakePhoto);
            cameraBack.onClick.AddListener(game.CloseCamera);
            nextSample.onClick.AddListener(CycleSample);
            useSample.onClick.AddListener(EquipSample);
            lookName.characterLimit = GyaruBuilder.NameLimit;
            cameraPanel.SetActive(false);
            rarityEffect.gameObject.SetActive(false);
            Navigate(Page.Builder);
        }

        public void SetLook(CardDefinition[] cards, int filled)
        {
            character.SetLook(cards);
            for (int i = 0; i < slotLabels.Length; i++)
                slotLabels[i].text = cards[i]?.DisplayName ?? "Empty";

            filledCount.text = $"{filled} / {GyaruBuilder.SlotCount} equipped";
            finishButton.interactable = filled > 0;
            resetButton.interactable = filled > 0;
        }

        public void ShowEquipped(CardDefinition card, bool isNew)
        {
            string added = isNew ? " · Added to Card Book" : "";
            string rarity = card.Rarity == CardRarity.Common ? "" : card.RarityName + " · ";
            scanFeedback.text = $"{rarity}{card.DisplayName} equipped{added}";
            scanFeedback.color = card.Rarity == CardRarity.SuperRare ? Gold : Ink;
            StopRarityReveal();
            if (card.Rarity != CardRarity.Common)
                rarityAnimation = StartCoroutine(AnimateRarity(card.Rarity));
        }

        private IEnumerator AnimateRarity(CardRarity rarity)
        {
            if (sparkleLayer == null)
                CreateSparkleLayer();

            bool superRare = rarity == CardRarity.SuperRare;
            int count = superRare ? SuperRareSparkleCount : RareSparkleCount;
            float duration = superRare ? 1.8f : 0.9f;
            Color banner = superRare ? Gold : SilverBlue;
            rarityEffect.text = superRare ? "SUPER RARE" : "RARE";
            rarityEffect.gameObject.SetActive(true);
            sparkleLayer.gameObject.SetActive(true);
            for (int i = 0; i < sparkles.Length; i++)
                sparkles[i].enabled = i < count;

            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                float progress = time / duration;
                float pulse = Mathf.Sin(progress * Mathf.PI);
                banner.a = Mathf.Clamp01((1f - progress) * 4f);
                rarityEffect.color = banner;
                rarityEffect.rectTransform.localScale = Vector3.one * (1f + pulse * (superRare ? .20f : .09f));

                float radius = Mathf.Lerp(.18f, .43f, Mathf.Sqrt(progress));
                float unit = Mathf.Min(sparkleLayer.rect.width, sparkleLayer.rect.height);
                for (int i = 0; i < count; i++)
                {
                    float phase = (float)i / count;
                    float angle = (phase + (superRare ? progress * .22f : 0f)) * 2f * Mathf.PI;
                    var position = new Vector2(.5f + Mathf.Cos(angle) * radius, .48f + Mathf.Sin(angle) * radius * .85f);
                    Image sparkle = sparkles[i];
                    RectTransform diamond = sparkle.rectTransform;
                    diamond.anchorMin = position;
                    diamond.anchorMax = position;
                    float twinkle = .40f + .60f * Mathf.Abs(Mathf.Sin((progress * 2f + phase) * Mathf.PI));
                    diamond.sizeDelta = Vector2.one * (unit * (superRare ? .042f : .031f) * twinkle);
                    diamond.localRotation = Quaternion.Euler(0, 0, 45f + (superRare ? progress * 180f : 0f));

                    Color color = superRare
                        ? Color.Lerp(new Color(1f, .70f, .20f), new Color(.74f, .30f, .90f), Mathf.PingPong(phase * 2f + progress * 2f, 1f))
                        : Color.Lerp(SilverBlue, Color.white, twinkle * .75f);
                    color.a = pulse;
                    sparkle.color = color;
                }

                if (superRare)
                    character.SetRevealPose(Mathf.SmoothStep(0f, 1f, Mathf.Min(progress * 6f, (1f - progress) * 4f)));
                yield return null;
            }

            rarityAnimation = null;
            HideRarityReveal();
        }

        private void CreateSparkleLayer()
        {
            // Temporary art consists of tinted UI diamonds that can be replaced with sprites.
            var overlay = new GameObject("Rarity Sparkles", typeof(RectTransform));
            sparkleLayer = (RectTransform)overlay.transform;
            sparkleLayer.SetParent(character.transform, false);
            sparkleLayer.anchorMin = Vector2.zero;
            sparkleLayer.anchorMax = Vector2.one;
            sparkleLayer.sizeDelta = Vector2.zero;
            sparkles = new Image[SuperRareSparkleCount];

            for (int i = 0; i < sparkles.Length; i++)
            {
                var sparkle = new GameObject("Sparkle", typeof(RectTransform), typeof(Image));
                sparkle.transform.SetParent(sparkleLayer, false);
                sparkles[i] = sparkle.GetComponent<Image>();
                sparkles[i].raycastTarget = false;
            }
        }

        private void StopRarityReveal()
        {
            if (rarityAnimation != null)
            {
                StopCoroutine(rarityAnimation);
                rarityAnimation = null;
            }
            HideRarityReveal();
        }

        private void HideRarityReveal()
        {
            rarityEffect.gameObject.SetActive(false);
            if (sparkleLayer != null)
                sparkleLayer.gameObject.SetActive(false);
            character.SetRevealPose(0f);
        }

        public void ShowScanFeedback(string message)
        {
            StopRarityReveal();
            scanFeedback.color = Ink;
            scanFeedback.text = message;
            if (IsScanning)
                cameraStatus.text = message;
        }

        public void ShowPhoto(CardDefinition[] cards)
        {
            ShowPage(Page.Photo);
            photoHeading.text = "Name your gyaru";
            photoStatus.text = game.UsesBrowserCamera ? "Choose a camera backdrop or save this portrait." : "Save your illustrated portrait.";
            lookName.text = "";
            portrait.SetLook(cards);
            portrait.SetPhoto(null);
        }

        public void SetPhotoBackground(Texture2D photo)
        {
            portrait.SetPhoto(photo);
            photoStatus.text = "Camera backdrop added.";
        }

        public string ExportPhotoOverlay() => portrait.ExportOverlay();

        public void ShowSavedLook(string name, CardDefinition[] cards, Texture2D photo)
        {
            ShowPage(Page.SavedLook);
            savedName.text = name;
            savedPortrait.SetLook(cards);
            savedPortrait.SetPhoto(photo);
            confirmDelete = false;
            SetButtonLabel(deleteSaved, "Delete");
        }

        public void ShowCamera(bool photo)
        {
            StopRarityReveal();
            camera = photo ? CameraPage.Photo : CameraPage.Scan;
            cameraPanel.SetActive(true);
            cameraHeading.text = photo ? "Take a photo" : "Scan a card";
            cameraCapture.gameObject.SetActive(photo);
            cameraCapture.interactable = false;
            nextSample.gameObject.SetActive(!photo);
            useSample.gameObject.SetActive(!photo);
            sampleLabel.gameObject.SetActive(!photo);
            cameraStatus.text = game.UsesBrowserCamera
                ? "Allow camera access, then center your subject."
                : "Choose a sample card below.";
#if UNITY_EDITOR
            cameraStatus.text = "Starting your webcam…";
#endif
            if (!photo)
                RefreshSample();
        }

        public void CloseCamera()
        {
            camera = CameraPage.None;
            cameraPanel.SetActive(false);
        }

        public void ShowCameraStatus(string status)
        {
            if (status != "ready")
            {
                cameraStatus.text = status;
                return;
            }

            cameraStatus.text = IsTakingPhoto ? "Frame your backdrop and tap Capture." : "Hold the QR code inside the square.";
            cameraCapture.interactable = IsTakingPhoto;
        }

        private void Navigate(Page destination)
        {
            if (IsCameraOpen)
                game.CloseCamera();
            if (page == Page.Photo || page == Page.SavedLook)
                game.ReleasePhoto();

            ShowPage(destination);
            if (destination == Page.CardBook)
                RefreshBook();
            if (destination == Page.Album)
                RefreshAlbum();
            if (destination == Page.Achievements)
                RefreshAchievements();
        }

        private void ShowPage(Page destination)
        {
            if (destination != Page.Builder)
                StopRarityReveal();

            page = destination;
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == (int)destination);

            for (int i = 0; i < tabs.Length; i++)
                SetButtonColor(tabs[i], i == (int)destination ? Accent : Ink);
        }

        private void OnDisable()
        {
            if (game != null)
                StopRarityReveal();
        }

        private void SelectRarity(CardRarity rarity)
        {
            filter = rarity;
            bookPageIndex = 0;
            RefreshBook();
        }

        private void TurnBookPage(int direction)
        {
            bookPageIndex += direction;
            RefreshBook();
        }

        private void RefreshBook()
        {
            CardDefinition[] group = game.Catalog.Where(card => card.Rarity == filter).ToArray();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)group.Length / CardsPerPage));
            bookPageIndex = Mathf.Clamp(bookPageIndex, 0, totalPages - 1);
            bookHeading.text = $"{game.DiscoveredCount} / {game.Catalog.Count} discovered";
            bookPage.text = $"{bookPageIndex + 1} / {totalPages}";
            bookPrevious.interactable = bookPageIndex > 0;
            bookNext.interactable = bookPageIndex + 1 < totalPages;
            bookList.text = string.Join("\n\n", group.Skip(bookPageIndex * CardsPerPage).Take(CardsPerPage)
                .Select(card => game.IsDiscovered(card) ? $"{card.DisplayName}  ·  {card.Slot}" : $"Undiscovered  ·  {card.Slot}"));
            for (int i = 0; i < bookFilters.Length; i++)
                SetButtonColor(bookFilters[i], i == (int)filter ? Accent : Ink);
        }

        private void RefreshAlbum()
        {
            albumSummary.text = game.Album.Count == 0 ? "Your saved gyaru will appear here." : $"{game.Album.Count} / {GyaruBuilder.AlbumLimit} portraits saved";
            for (int i = 0; i < albumRows.Length; i++)
            {
                bool occupied = i < game.Album.Count;
                albumRows[i].gameObject.SetActive(occupied);
                if (occupied)
                    SetButtonLabel(albumRows[i], game.Album[i].name);
            }
        }

        private void RefreshAchievements()
        {
            achievementList.text = string.Join("\n\n",
                Achievement("Full Look", $"Fill all {GyaruBuilder.SlotCount} slots.", game.HasFullLook),
                Achievement("Animal Mix", "Mix three animals in the same look.", game.HasThreeAnimals),
                Achievement("Photo Debut", "Save a gyaru to the album.", game.HasSavedLook),
                Achievement("Card Collector", "Discover every card.", game.DiscoveredCount == game.Catalog.Count));
        }

        private static string Achievement(string title, string detail, bool complete) =>
            $"{title}  ·  {(complete ? "Unlocked" : "Locked")}\n{detail}";

        private void SaveCurrentLook()
        {
            if (string.IsNullOrWhiteSpace(lookName.text))
            {
                photoStatus.text = "Give your gyaru a name first.";
                return;
            }

            if (game.SaveLook(lookName.text))
                Navigate(Page.Album);
            else
                photoStatus.text = "The album is full. Delete a portrait to save this look.";
        }

        private void OpenAlbumLook(int index)
        {
            albumIndex = index;
            game.OpenSavedLook(index);
        }

        private void EditCurrentLook()
        {
            game.LoadSavedLook(albumIndex);
            Navigate(Page.Builder);
            ShowScanFeedback("Look loaded from your album.");
        }

        private void DeleteCurrentLook()
        {
            if (!confirmDelete)
            {
                confirmDelete = true;
                SetButtonLabel(deleteSaved, "Confirm delete");
                return;
            }

            game.DeleteSavedLook(albumIndex);
            Navigate(Page.Album);
        }

        private void CycleSample()
        {
            sampleIndex = (sampleIndex + 1) % game.Catalog.Count;
            RefreshSample();
        }

        private void RefreshSample()
        {
            CardDefinition card = game.Catalog[sampleIndex];
            sampleLabel.text = $"{card.DisplayName}  ·  {card.Id}";
        }

        private void EquipSample()
        {
            game.CloseCamera();
            game.Equip(game.Catalog[sampleIndex]);
            sampleIndex = (sampleIndex + 1) % game.Catalog.Count;
        }

        private static void SetButtonLabel(Button button, string value) =>
            button.GetComponentInChildren<Text>(true).text = value;

        private static void SetButtonColor(Button button, Color color) => button.targetGraphic.color = color;
    }
}
