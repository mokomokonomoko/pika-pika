using System;
using System.Collections.Generic;
using System.Linq;
using CardScanner;
using UnityEngine;

namespace AnimalGyaruMix
{
    public sealed class GyaruBuilder : MonoBehaviour
    {
        public static readonly int SlotCount = Enum.GetValues(typeof(CardSlot)).Length;
        public const int AlbumLimit = 8;
        public const int NameLimit = 20;
        private const string SaveKey = "AnimalGyaruMix.Save.v1";

        [SerializeField] private CardDefinition[] catalog;
        [SerializeField] private QRManager scanner;
        [SerializeField] private BuilderUI view;

        private readonly Dictionary<string, CardDefinition> cards = new(StringComparer.OrdinalIgnoreCase);
        private readonly CardDefinition[] equipped = new CardDefinition[SlotCount];
        private readonly HashSet<string> discovered = new(StringComparer.OrdinalIgnoreCase);
        private SaveData save;
        private string cameraPhoto;
        private Texture2D photoTexture;
        private bool photoPending;

        public IReadOnlyList<CardDefinition> Catalog => catalog;
        public IReadOnlyList<SavedLook> Album => save.album;
        public bool UsesBrowserCamera => Application.platform == RuntimePlatform.WebGLPlayer;
        public int DiscoveredCount => discovered.Count;
        public int FilledCount => equipped.Count(card => card != null);
        public bool HasFullLook => save.fullLook;
        public bool HasThreeAnimals => save.threeAnimals;
        public bool HasSavedLook => save.firstSave;

        private void Awake()
        {
            if (scanner == null || view == null)
            {
                enabled = false;
                return;
            }

            foreach (CardDefinition card in catalog)
                cards.Add(card.Id, card);

            save = Load();
            discovered.UnionWith(save.discovered.Where(cards.ContainsKey));
            scanner.CodeScanned += ReceiveScan;
            scanner.CameraStatus += ReceiveCameraStatus;
            scanner.PhotoReceived += ReceivePhoto;
        }

        private void Start()
        {
            view.Initialize(this);
            RefreshLook();
        }

        private void OnDestroy()
        {
            if (scanner == null || view == null)
                return;

            scanner.CodeScanned -= ReceiveScan;
            scanner.CameraStatus -= ReceiveCameraStatus;
            scanner.PhotoReceived -= ReceivePhoto;
            ReleasePhoto();
        }

        public bool IsDiscovered(CardDefinition card) => discovered.Contains(card.Id);

        public void ReceiveScan(string payload)
        {
            if (!view.AcceptsScans || string.IsNullOrWhiteSpace(payload))
                return;

            if (!cards.TryGetValue(payload.Trim(), out CardDefinition card))
            {
                view.ShowScanFeedback("Unknown card. Try another QR code.");
                return;
            }

            if (view.IsScanning)
                CloseCamera();

            Equip(card);
        }

        public void Equip(CardDefinition card)
        {
            equipped[(int)card.Slot] = card;
            bool isNew = discovered.Add(card.Id);
            save.fullLook |= FilledCount == SlotCount;
            save.threeAnimals |= equipped.Where(part => part != null && !string.IsNullOrWhiteSpace(part.Animal))
                .Select(part => part.Animal.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 3;

            Persist();
            RefreshLook();
            view.ShowEquipped(card, isNew);
        }

        public void ClearLook()
        {
            Array.Clear(equipped, 0, SlotCount);
            Persist();
            RefreshLook();
            view.ShowScanFeedback("Look reset. Scan a card to start again.");
        }

        public void OpenScanner()
        {
            view.ShowCamera(false);
            scanner.SetCameraMode(1);
        }

        public void OpenPhoto()
        {
            if (FilledCount == 0)
                return;

            ReleasePhoto();
            view.ShowPhoto(equipped);
        }

        public void OpenPhotoCamera()
        {
            QRScannerBridge.SetPortraitOverlay(view.ExportPhotoOverlay());
            view.ShowCamera(true);
            scanner.SetCameraMode(2);
        }

        public void TakePhoto()
        {
            if (!view.IsTakingPhoto || photoPending)
                return;

            photoPending = true;
            scanner.CapturePhoto();
        }

        public void CloseCamera()
        {
            photoPending = false;
            scanner.SetCameraMode(0);
            view.CloseCamera();
        }

        private void ReceiveCameraStatus(string status)
        {
            if (view.IsCameraOpen)
                view.ShowCameraStatus(status);
        }

        private void ReceivePhoto(string jpeg)
        {
            if (!photoPending || !view.IsTakingPhoto)
                return;

            photoPending = false;
            Texture2D captured = DecodePhoto(jpeg);
            if (captured == null)
            {
                view.ShowCameraStatus("Camera is still getting ready. Try again.");
                return;
            }

            ReleasePhoto();
            cameraPhoto = jpeg;
            photoTexture = captured;
            CloseCamera();
            view.SetPhotoBackground(photoTexture);
        }

        public bool SaveLook(string name)
        {
            name = name.Trim();
            if (name.Length == 0 || save.album.Count >= AlbumLimit || FilledCount == 0)
                return false;

            save.album.Insert(0, new SavedLook
            {
                name = name.Length > NameLimit ? name.Substring(0, NameLimit) : name,
                cards = CurrentIds(),
                photo = cameraPhoto
            });
            save.firstSave = true;
            Persist();
            ReleasePhoto();
            return true;
        }

        public void OpenSavedLook(int index)
        {
            SavedLook look = save.album[index];
            ReleasePhoto();
            photoTexture = DecodePhoto(look.photo);
            view.ShowSavedLook(look.name, ResolveLook(look.cards), photoTexture);
        }

        public void LoadSavedLook(int index)
        {
            Array.Copy(ResolveLook(save.album[index].cards), equipped, SlotCount);
            ReleasePhoto();
            Persist();
            RefreshLook();
        }

        public void DeleteSavedLook(int index)
        {
            save.album.RemoveAt(index);
            ReleasePhoto();
            Persist();
        }

        public void ReleasePhoto()
        {
            if (photoTexture != null)
                Destroy(photoTexture);
            photoTexture = null;
            cameraPhoto = null;
        }

        private CardDefinition[] ResolveLook(string[] ids)
        {
            var result = new CardDefinition[SlotCount];
            if (ids == null)
                return result;

            foreach (string id in ids)
                if (!string.IsNullOrEmpty(id) && cards.TryGetValue(id, out CardDefinition card))
                    result[(int)card.Slot] = card;
            return result;
        }

        private string[] CurrentIds() => equipped.Select(card => card?.Id ?? "").ToArray();

        private void RefreshLook() => view.SetLook(equipped, FilledCount);

        private void Persist()
        {
            save.discovered = discovered.OrderBy(id => id).ToList();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        private static SaveData Load()
        {
            string json = PlayerPrefs.GetString(SaveKey);
            try
            {
                SaveData data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
                data ??= new SaveData();
                data.album ??= new List<SavedLook>();
                data.discovered ??= new List<string>();
                return data;
            }
            catch (ArgumentException)
            {
                return new SaveData();
            }
        }

        private static Texture2D DecodePhoto(string jpeg)
        {
            if (string.IsNullOrEmpty(jpeg) || jpeg.Length > 65000)
                return null;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(jpeg);
            }
            catch (FormatException)
            {
                return null;
            }

            var texture = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(texture, bytes, true))
                return texture;

            Destroy(texture);
            return null;
        }

        [Serializable]
        private sealed class SaveData
        {
            public List<string> discovered = new();
            public List<SavedLook> album = new();
            public bool fullLook;
            public bool threeAnimals;
            public bool firstSave;
        }

        [Serializable]
        public sealed class SavedLook
        {
            public string name;
            public string[] cards;
            public string photo;
        }
    }
}
