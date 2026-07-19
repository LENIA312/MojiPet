using System;
using System.Collections.Generic;
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
        }

        private void OnDestroy()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.EventBus != null)
            {
                gameManager.EventBus.Unsubscribe<OnPetUnlocked>(HandlePetUnlocked);
            }
        }

        private void HandlePetUnlocked(OnPetUnlocked e)
        {
            AddToken(e.CharacterId);
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
