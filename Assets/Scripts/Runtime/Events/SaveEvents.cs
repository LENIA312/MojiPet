using System;
using Mojipet.Models;

namespace Mojipet.Events
{
    public readonly struct OnSaveLoaded
    {
        public readonly SaveData SaveData;

        public OnSaveLoaded(SaveData saveData)
        {
            SaveData = saveData;
        }
    }

    public readonly struct OnSaveCompleted
    {
        public readonly DateTime LastSaveUtc;

        public OnSaveCompleted(DateTime lastSaveUtc)
        {
            LastSaveUtc = lastSaveUtc;
        }
    }

    public readonly struct OnNewGameCreated
    {
    }

    public readonly struct OnMigrationCompleted
    {
        public readonly int OldVersion;
        public readonly int NewVersion;

        public OnMigrationCompleted(int oldVersion, int newVersion)
        {
            OldVersion = oldVersion;
            NewVersion = newVersion;
        }
    }
}
