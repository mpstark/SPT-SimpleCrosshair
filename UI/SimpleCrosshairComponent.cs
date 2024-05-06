using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using EFT;
using EFT.UI;
using SimpleCrosshair.Config;
using SimpleCrosshair.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleCrosshair
{
    public class SimpleCrosshairComponent : MonoBehaviour
    {
        private const float TransitionTime = 0.15f;
        private readonly static string DefaultImagePath = Path.Combine(Plugin.Path, "crosshair.png");
        private readonly static string CustomImagePath = Path.Combine(Plugin.Path, "custom_crosshair.png");
        private readonly static HashSet<EPlayerState> PlayerStatesToHideWith = new HashSet<EPlayerState>()
        {
            EPlayerState.ProneMove,
            EPlayerState.Sprint,
            EPlayerState.BreachDoor,
        };
        private readonly static HashSet<Type> HandControllersToShowWith = new HashSet<Type>()
        {
            typeof(Player.FirearmController),
            typeof(Player.BaseKnifeController),
            typeof(Player.BaseGrenadeController),
        };

        private Image _crosshairImage;
        private bool _isVisible = true;
        private Dictionary<string, bool> _reasonsToHide = new Dictionary<string, bool>();

        public static SimpleCrosshairComponent AttachToBattleUIScreen(BattleUIScreen screen)
        {
            // setup container
            var containerGO = new GameObject("SimpleCrosshair", typeof(RectTransform), typeof(CanvasRenderer));
            containerGO.layer = screen.gameObject.layer;
            containerGO.transform.SetParent(screen.transform);
            containerGO.transform.localScale = Vector3.one;
            containerGO.GetRectTransform().anchoredPosition = Vector2.zero;
            containerGO.GetRectTransform().pivot = new Vector2(0.5f, 0.5f);
            containerGO.GetRectTransform().anchorMin = new Vector2(0.5f, 0.5f);
            containerGO.GetRectTransform().anchorMax = new Vector2(0.5f, 0.5f);

            // add our component
            var component = containerGO.AddComponent<SimpleCrosshairComponent>();
            return component;
        }

        private void Awake()
        {
            // if have custom crosshair image, load that one
            var imagePath = DefaultImagePath;
            if (File.Exists(CustomImagePath))
            {
                imagePath = CustomImagePath;
            }

            var texture = TextureUtils.LoadTexture2DFromPath(imagePath);
            _crosshairImage = gameObject.AddComponent<Image>();
            _crosshairImage.sprite = Sprite.Create(texture,
                                                   new Rect(0f, 0f, texture.width, texture.height),
                                                   new Vector2(texture.width / 2, texture.height / 2));
            _crosshairImage.type = Image.Type.Simple;

            ReadConfig();

            // we may have missed register player, so if a player exists, go ahead and call it
            if (GameUtils.Player != null)
            {
                OnRegisterMainPlayer();
            }
        }

        public void ReadConfig()
        {
            gameObject.GetRectTransform().sizeDelta = Settings.CrosshairSize.Value * Vector2.one;
            gameObject.GetRectTransform().anchoredPosition = Settings.CrosshairOffset.Value;
            _crosshairImage.color = Settings.CrosshairColor.Value;
        }

        public void SetReasonToHide(string reason, bool shouldHide)
        {
            _reasonsToHide[reason] = shouldHide;
            SetVisibility(!_reasonsToHide.Any((pair) => pair.Value));
        }

        private void SetVisibility(bool visible)
        {
            if (_isVisible == visible)
            {
                return;
            }

            var configColor = Settings.CrosshairColor.Value;
            var finalColor = new Color(configColor.r, configColor.g, configColor.b, visible ? configColor.a : 0);

            // tween fade the crosshair in/out
            DOTween.To(() => _crosshairImage.color,
                       color => _crosshairImage.color = color,
                       finalColor,
                       TransitionTime);

            _isVisible = visible;
        }

        internal void OnRegisterMainPlayer()
        {
            var player = GameUtils.Player;

            // register events on player
            player.OnHandsControllerChanged += OnHandControllerChanged;
            player.OnSenseChanged += OnSenseChanged;
            player.OnPlayerDead += OnPlayerDead;
            player.MovementContext.OnStateChanged += OnMovementStateChanged;
            player.PossibleInteractionsChanged += PossibleInteractionsChanged;

            // we may have missed initial event, so call it now
            OnHandControllerChanged(null, player.HandsController);

            // clear all reasons to hide, since we're resetting the player
            _reasonsToHide.Clear();
        }

        internal void OnUnregisterMainPlayer()
        {
            var player = GameUtils.Player;

            // unregister events on player
            player.OnHandsControllerChanged -= OnHandControllerChanged;
            player.OnSenseChanged -= OnSenseChanged;
            player.OnPlayerDead -= OnPlayerDead;
            player.MovementContext.OnStateChanged -= OnMovementStateChanged;
            player.PossibleInteractionsChanged += PossibleInteractionsChanged;

            // unregister from hands controller if there is one
            OnHandControllerChanged(player.HandsController, null);
        }

        private void OnHandControllerChanged(Player.AbstractHandsController oldController,
                                             Player.AbstractHandsController newController)
        {
            if (oldController != null)
            {
                oldController.OnAimingChanged -= OnAimingChanged;
            }

            if (newController != null)
            {
                newController.OnAimingChanged += OnAimingChanged;

                // check if controller is one that we want to show a crosshair for
                SetReasonToHide("handController", !HandControllersToShowWith.Any(c =>
                    c.IsAssignableFrom(newController.GetType())));
            }
        }

        private void OnAimingChanged(bool isAiming)
        {
            SetReasonToHide("aiming", isAiming);
        }

        private void OnSenseChanged(bool sensingItem)
        {
            SetReasonToHide("sensing", sensingItem);
        }

        private void OnPlayerDead(Player player, IPlayer lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            SetReasonToHide("dead", true);
        }

        private void OnMovementStateChanged(EPlayerState previousState, EPlayerState nextState)
        {
            SetReasonToHide("movementState", PlayerStatesToHideWith.Contains(nextState));
        }

        private void PossibleInteractionsChanged()
        {
            SetReasonToHide("interactableObject", GameUtils.Player.InteractableObject != null);
        }
    }
}
