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

        // config to be used later
        private float _fadeInOutTime;
        private bool _useDynamicPosition;
        private float _dynamicPositionSmoothTime;
        private float _dynamicPositionAimDistance;

        // cached variables
        private Player _cachedPlayer;
        private Player.FirearmController _cachedFirearmController;
        private Canvas _cachedCanvas;
        private Camera _cachedCamera;

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

            // load crosshair texture from disk
            var texture = TextureUtils.LoadTexture2DFromPath(imagePath);
            _crosshairImage = gameObject.AddComponent<Image>();
            _crosshairImage.sprite = Sprite.Create(texture,
                                                   new Rect(0f, 0f, texture.width, texture.height),
                                                   new Vector2(texture.width / 2, texture.height / 2));
            _crosshairImage.type = Image.Type.Simple;

            // we may have missed register player, so if a player exists, go ahead and call it
            // TODO: investigate if this is needed or if the patch should be used instead
            if (GameUtils.Player != null)
            {
                OnRegisterMainPlayer();
            }

            ReadConfig();
        }

        public void FixedUpdate()
        {
            if (!_useDynamicPosition || !_isVisible)
            {
                return;
            }

            CalculateDynamicAimPoint();
        }

        public void ReadConfig()
        {
            gameObject.GetRectTransform().sizeDelta = Settings.Size.Value * Vector2.one;
            gameObject.GetRectTransform().anchoredPosition = Settings.Offset.Value;
            _crosshairImage.color = Settings.Color.Value;
            _fadeInOutTime = Settings.FadeInOutTime.Value;
            _dynamicPositionSmoothTime = Settings.DynamicPositionSmoothTime.Value;
            _dynamicPositionAimDistance = Settings.DynamicPositionAimDistance.Value;

            if (_useDynamicPosition != Settings.UseDynamicPosition.Value)
            {
                _useDynamicPosition = Settings.UseDynamicPosition.Value;
                if (!_useDynamicPosition)
                {
                    ResetToStaticAimPoint();
                }
            }
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

            var configColor = Settings.Color.Value;
            var finalColor = new Color(configColor.r, configColor.g, configColor.b, visible ? configColor.a : 0);

            // tween fade the crosshair in/out
            DOTween.To(() => _crosshairImage.color,
                       color => _crosshairImage.color = color,
                       finalColor,
                       _fadeInOutTime);

            _isVisible = visible;
        }

        private void CalculateDynamicAimPoint()
        {
            if (_cachedPlayer == null)
            {
                return;
            }

            var rayBegin = _cachedFirearmController != null
                ? _cachedFirearmController.CurrentFireport.position
                : _cachedPlayer.CameraPosition.position;

            var rayDirection = _cachedFirearmController != null
                ? _cachedFirearmController.WeaponDirection
                : _cachedPlayer.LookDirection;

            // do raycast, if it hits, use that as aim point, if not, use end of ray
            var ray = new Ray(rayBegin, rayDirection);
            var rayEnd = rayBegin + rayDirection.normalized * _dynamicPositionAimDistance;
			var didRayHit = Physics.Raycast(
                ray, out RaycastHit rayHit, _dynamicPositionAimDistance, LayerMaskClass.HighPolyWithTerrainMask);

            var worldAimPoint = didRayHit ? rayHit.point : rayEnd;
            var screenAimPoint = GetCanvasScreenPosition(worldAimPoint);

            // move the anchor position of the cursor to the aim point
            _crosshairImage.GetRectTransform().DOAnchorPos(screenAimPoint, _dynamicPositionSmoothTime);
        }

        private void ResetToStaticAimPoint()
        {
            DOTween.Kill(_crosshairImage.GetRectTransform());
            _crosshairImage.GetRectTransform().anchoredPosition = Vector2.zero;
        }

        private Vector2 GetCanvasScreenPosition(Vector3 worldPoint)
        {
            var canvasRect = _cachedCanvas.GetRectTransform();
            var viewportPoint = _cachedCamera.WorldToViewportPoint(worldPoint);
            return new Vector2(
                (viewportPoint.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                (viewportPoint.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
        }

        internal void OnRegisterMainPlayer()
        {
            var player = GameUtils.Player;

            _cachedPlayer = player;
            _cachedCamera = Camera.main;
            _cachedCanvas = gameObject.GetComponentInParent<Canvas>();

            // register events on player
            player.OnHandsControllerChanged += OnHandControllerChanged;
            player.OnSenseChanged += OnSenseChanged;
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
            player.PossibleInteractionsChanged += PossibleInteractionsChanged;

            // unregister from hands controller if there is one
            OnHandControllerChanged(player.HandsController, null);

            _cachedPlayer = null;
            _cachedCamera = null;
            _cachedCanvas = null;
        }

        private void OnHandControllerChanged(Player.AbstractHandsController oldController,
                                             Player.AbstractHandsController newController)
        {
            if (oldController != null)
            {
                oldController.OnAimingChanged -= OnAimingChanged;
            }

            if (newController == null)
            {
                return;
            }

            newController.OnAimingChanged += OnAimingChanged;

            // check if controller is one that we want to show a crosshair for
            SetReasonToHide("handController", !HandControllersToShowWith.Any(c =>
                c.IsAssignableFrom(newController.GetType())));

            _cachedFirearmController = (newController is Player.FirearmController)
                ? newController as Player.FirearmController
                : null;
        }

        private void OnAimingChanged(bool isAiming)
        {
            SetReasonToHide("aiming", isAiming);
        }

        private void OnSenseChanged(bool sensingItem)
        {
            SetReasonToHide("sensing", sensingItem);
        }

        internal void OnMovementStateChanged(EPlayerState state)
        {
            SetReasonToHide("movementState", PlayerStatesToHideWith.Contains(state));
        }

        private void PossibleInteractionsChanged()
        {
            SetReasonToHide("interactableObject", GameUtils.Player.InteractableObject != null);
        }
    }
}
