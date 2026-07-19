using Mojipet.Systems;
using UnityEngine;

namespace Mojipet.UI.Presenters
{
    public sealed class SettingsPresenter
    {
        private readonly SaveSystem _saveSystem;

        public SettingsPresenter(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        public float BgmVolume => _saveSystem.Data.Settings.BgmVolume;
        public float SeVolume => _saveSystem.Data.Settings.SeVolume;
        public int Quality => _saveSystem.Data.Settings.Quality;
        public string[] QualityNames => QualitySettings.names;

        public void SetBgmVolume(float value)
        {
            _saveSystem.Data.Settings.BgmVolume = value;
            _saveSystem.Save();
        }

        public void SetSeVolume(float value)
        {
            _saveSystem.Data.Settings.SeVolume = value;
            _saveSystem.Save();
        }

        public void SetQuality(int qualityIndex)
        {
            _saveSystem.Data.Settings.Quality = qualityIndex;
            QualitySettings.SetQualityLevel(qualityIndex, true);
            _saveSystem.Save();
        }
    }
}
