using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mojipet.Core;
using Mojipet.Events;
using Mojipet.Master;
using Mojipet.UI.Components;
using UnityEngine;

namespace Mojipet.UI.Views
{
    public sealed class HomeWorldView : MonoBehaviour
    {
        // The garden's pannable area, larger than any single viewport so there's
        // actually something to scroll through. Characters wander within this;
        // the player swipes/drags to look around.
        private static readonly Vector2 GardenSize = new Vector2(2160f, 3320f);
        private const string BackgroundResourcePath = "GardenBackground";

        private RectTransform _worldRect;
        private readonly Dictionary<int, PetToken> _tokensByCharacterId = new Dictionary<int, PetToken>();
        private Action<int> _onPetClicked;

        public static HomeWorldView Create(Transform parent, Action<int> onPetClicked)
        {
            var go = new GameObject("World", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<HomeWorldView>();
            view.Initialize(onPetClicked);
            return view;
        }

        private void Initialize(Action<int> onPetClicked)
        {
            _onPetClicked = onPetClicked;

            // This object is the visible viewport window, clipped to sit strictly
            // between the header and footer bars (via Screen.safeArea, matching
            // HomeUIRoot's header/footer placement) so characters can never wander
            // underneath the UI chrome -- the mask makes it geometrically
            // impossible rather than relying on wander-bounds math alone.
            var viewportRect = (RectTransform)transform;
            var safeAreaMin = UiFactory.GetSafeAreaAnchorMin();
            var safeAreaMax = UiFactory.GetSafeAreaAnchorMax();
            viewportRect.anchorMin = new Vector2(0f, safeAreaMin.y);
            viewportRect.anchorMax = new Vector2(1f, safeAreaMax.y);
            viewportRect.offsetMin = new Vector2(0f, UiTheme.FooterHeight);
            viewportRect.offsetMax = new Vector2(0f, -UiTheme.HeaderHeight);

            // Static backdrop behind the scrollable garden (doesn't pan with it).
            // Drop a sprite at Assets/UI/Resources/GardenBackground.png (Texture
            // Type: Sprite) to fill this in -- until then it's simply absent.
            var backgroundSprite = Resources.Load<Sprite>(BackgroundResourcePath);
            if (backgroundSprite != null)
            {
                var background = UiFactory.CreateImage(viewportRect, backgroundSprite);
                background.raycastTarget = false;
                UiFactory.StretchFull((RectTransform)background.transform);
            }

            var scrollView = UiFactory.CreateFreeScrollArea(viewportRect, GardenSize, out _worldRect);
            UiFactory.StretchFull(scrollView);

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            foreach (var pet in gameManager.PetSystem.GetAllPets())
            {
                AddToken(pet.CharacterId);
            }

            gameManager.EventBus.Subscribe<OnPetUnlocked>(HandlePetUnlocked);
            gameManager.EventBus.Subscribe<OnResearchCompleted>(HandleResearchCompleted);
        }

        private void OnDestroy()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.EventBus != null)
            {
                gameManager.EventBus.Unsubscribe<OnPetUnlocked>(HandlePetUnlocked);
                gameManager.EventBus.Unsubscribe<OnResearchCompleted>(HandleResearchCompleted);
            }
        }

        private void HandlePetUnlocked(OnPetUnlocked e)
        {
            AddToken(e.CharacterId);
        }

        private void HandleResearchCompleted(OnResearchCompleted e)
        {
            GatherWordAsync(e.WordId).Forget();
        }

        // The one thing only a "letters are characters" game can do: when a
        // word finishes, physically walk its constituent letters together in
        // the garden for a moment before letting them wander off again.
        private async UniTaskVoid GatherWordAsync(int wordId)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            var characterIds = gameManager.WordSystem.GetCharacters(wordId);
            var seen = new HashSet<int>();
            var tokens = new List<PetToken>();

            foreach (var characterId in characterIds)
            {
                if (!seen.Add(characterId))
                {
                    continue; // word repeats a character (e.g. "ここ") -- don't gather the same token twice
                }

                if (_tokensByCharacterId.TryGetValue(characterId, out var token))
                {
                    tokens.Add(token);
                }
            }

            if (tokens.Count < 2)
            {
                return; // nothing to gather: single-character word, or other members not (yet) unlocked
            }

            var gatherPoint = Vector2.zero;
            foreach (var token in tokens)
            {
                gatherPoint += token.GetPosition();
            }

            gatherPoint /= tokens.Count;

            var gatherTasks = new List<UniTask>(tokens.Count);
            foreach (var token in tokens)
            {
                gatherTasks.Add(token.GatherAsync(gatherPoint, 1.5f));
            }

            await UniTask.WhenAll(gatherTasks);

            if (tokens.Count > 0)
            {
                tokens[0].ShowReactionAsync("✨").Forget();
            }
        }

        private void AddToken(int characterId)
        {
            if (_tokensByCharacterId.ContainsKey(characterId))
            {
                return;
            }

            var gameManager = GameManager.Instance;
            var petMasterEntry = FindPetMasterEntry(gameManager, characterId);
            var character = petMasterEntry != null ? petMasterEntry.Character : "?";

            var token = PetToken.Create(_worldRect, _worldRect, characterId, character, _onPetClicked);
            _tokensByCharacterId[characterId] = token;
        }

        private static PetMasterEntry FindPetMasterEntry(GameManager gameManager, int characterId)
        {
            foreach (var entry in gameManager.MasterManager.PetMaster.Entries)
            {
                if (entry.CharacterId == characterId)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
