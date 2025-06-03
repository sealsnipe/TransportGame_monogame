using System.ComponentModel.DataAnnotations;

namespace TransportGame.Game.Models
{
    /// <summary>
    /// Main settings container for the game.
    /// Contains all user-configurable settings organized by category.
    /// </summary>
    public class GameSettings
    {
        public DisplaySettings Display { get; set; } = new();
        public ControlSettings Controls { get; set; } = new();
        public AudioSettings Audio { get; set; } = new();
        public UISettings UI { get; set; } = new();

        /// <summary>
        /// Validates all settings and returns true if valid.
        /// </summary>
        public bool IsValid()
        {
            return Display.IsValid() && Controls.IsValid() && Audio.IsValid() && UI.IsValid();
        }

        /// <summary>
        /// Creates a deep copy of the settings.
        /// </summary>
        public GameSettings Clone()
        {
            return new GameSettings
            {
                Display = Display.Clone(),
                Controls = Controls.Clone(),
                Audio = Audio.Clone(),
                UI = UI.Clone()
            };
        }
    }

    /// <summary>
    /// Display and graphics related settings.
    /// </summary>
    public class DisplaySettings
    {
        [Range(800, 7680)] // Min 800x600, Max 8K
        public int ResolutionWidth { get; set; } = 1280;

        [Range(600, 4320)]
        public int ResolutionHeight { get; set; } = 720;

        public bool Fullscreen { get; set; } = false;

        public bool VSync { get; set; } = true;

        [Range(0.5f, 3.0f)] // 50% to 300% UI scaling
        public float UIScale { get; set; } = 1.0f;

        /// <summary>
        /// Validates display settings.
        /// </summary>
        public bool IsValid()
        {
            return ResolutionWidth >= 800 && ResolutionWidth <= 7680 &&
                   ResolutionHeight >= 600 && ResolutionHeight <= 4320 &&
                   UIScale >= 0.5f && UIScale <= 3.0f;
        }

        public DisplaySettings Clone()
        {
            return new DisplaySettings
            {
                ResolutionWidth = ResolutionWidth,
                ResolutionHeight = ResolutionHeight,
                Fullscreen = Fullscreen,
                VSync = VSync,
                UIScale = UIScale
            };
        }
    }

    /// <summary>
    /// Input and control related settings.
    /// </summary>
    public class ControlSettings
    {
        public string MenuKey { get; set; } = "Tab";
        public string MenuCloseKey { get; set; } = "Escape";

        [Range(0.1f, 20.0f)] // Very slow to very fast camera
        public float CameraSpeed { get; set; } = 5.0f;

        [Range(0.5f, 3.0f)] // 50% to 300% tooltip scaling
        public float TooltipScale { get; set; } = 1.5f;

        /// <summary>
        /// Validates control settings.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(MenuKey) &&
                   !string.IsNullOrWhiteSpace(MenuCloseKey) &&
                   CameraSpeed >= 0.1f && CameraSpeed <= 20.0f &&
                   TooltipScale >= 0.5f && TooltipScale <= 3.0f;
        }

        public ControlSettings Clone()
        {
            return new ControlSettings
            {
                MenuKey = MenuKey,
                MenuCloseKey = MenuCloseKey,
                CameraSpeed = CameraSpeed,
                TooltipScale = TooltipScale
            };
        }
    }

    /// <summary>
    /// Audio related settings.
    /// </summary>
    public class AudioSettings
    {
        [Range(0.0f, 1.0f)]
        public float MasterVolume { get; set; } = 0.8f;

        [Range(0.0f, 1.0f)]
        public float SfxVolume { get; set; } = 0.6f;

        [Range(0.0f, 1.0f)]
        public float MusicVolume { get; set; } = 0.4f;

        public bool Muted { get; set; } = false;

        /// <summary>
        /// Gets the effective volume for SFX (considers master volume and mute).
        /// </summary>
        public float EffectiveSfxVolume => Muted ? 0.0f : MasterVolume * SfxVolume;

        /// <summary>
        /// Gets the effective volume for music (considers master volume and mute).
        /// </summary>
        public float EffectiveMusicVolume => Muted ? 0.0f : MasterVolume * MusicVolume;

        /// <summary>
        /// Validates audio settings.
        /// </summary>
        public bool IsValid()
        {
            return MasterVolume >= 0.0f && MasterVolume <= 1.0f &&
                   SfxVolume >= 0.0f && SfxVolume <= 1.0f &&
                   MusicVolume >= 0.0f && MusicVolume <= 1.0f;
        }

        public AudioSettings Clone()
        {
            return new AudioSettings
            {
                MasterVolume = MasterVolume,
                SfxVolume = SfxVolume,
                MusicVolume = MusicVolume,
                Muted = Muted
            };
        }
    }

    /// <summary>
    /// User interface related settings.
    /// </summary>
    public class UISettings
    {
        public bool ShowHUD { get; set; } = true;
        public bool ShowDebugPanel { get; set; } = false;
        public string Language { get; set; } = "de";

        /// <summary>
        /// Validates UI settings.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Language);
        }

        public UISettings Clone()
        {
            return new UISettings
            {
                ShowHUD = ShowHUD,
                ShowDebugPanel = ShowDebugPanel,
                Language = Language
            };
        }
    }
}
