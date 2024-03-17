using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public partial class DomainReloadHelper
{
    public SerializableDictionary<guid, ISaved> SavingSystemsGUID = new();
    public SavingSystem.Save SavingLatestSave;
}

public static class SavingSystem
{
    static readonly uint LatestVersion = 1;
    static readonly Dictionary<guid, ISaved> _systemsByGuid = new();
    static readonly HashSet<Type> _validatedTypes = new();
    static Save _latestSave = new();
    static bool _loading;

    static SavingSystem()
    {
        // Restore data after domain reload
        DomainReloadHelper.BeforeReload += helper =>
        {
            helper.SavingSystemsGUID.Clear();
            foreach (var (guid, system) in _systemsByGuid)
                helper.SavingSystemsGUID.Add(guid, system);
            helper.SavingLatestSave = _latestSave;
        };
        DomainReloadHelper.AfterReload += helper =>
        {
            foreach (var (guid, system) in helper.SavingSystemsGUID)
                _systemsByGuid.Add(guid, system);
            helper.SavingSystemsGUID.Clear();
            _latestSave = helper.SavingLatestSave;
        };
    }

    /// <summary>
    /// Load save data for this component, may come from memory if a call to <see cref="Store{TSaved,THandler}"/> happened after <see cref="TrySaveToDisk"/>.
    /// </summary>
    /// <remarks>
    /// Also registers this instance for automatic saving to disk whenever <see cref="TrySaveToDisk"/> is called
    /// </remarks>
    public static bool TryRestore<TSaved, THandler>(TSaved source) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
    {
        RegisterAndValidate<TSaved, THandler>(source);

        if (_latestSave.Instances.TryGetValue(source.UniqueConstID, out var data))
        {
            source.RestoreFrom(data);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Save <typeparamref name="TSaved"/> to memory, will be saved to disk on the next call to <see cref="TrySaveToDisk"/>.
    /// You can call <see cref="Unregister{TSaved,THandler}"/> if you intend to destroy this system after this store.
    /// </summary>
    public static void Store<TSaved, THandler>(TSaved source) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
    {
        RegisterAndValidate<TSaved, THandler>(source);

        var newSaveData = new Save(_latestSave.Instances, SpawnPoint.LastSpawnUsed.Reference);
        source.StoreInto(newSaveData.Instances);
        _latestSave = newSaveData;
    }

    /// <summary>
    /// Capture <typeparamref name="TSaved"/>'s state to memory to be saved to disk later and remove it from the last-minute automatic save.
    /// </summary>
    /// <remarks>
    /// Useful if you need to destroy the component right now but still need to save its state.
    /// </remarks>
    public static void StoreAndUnregister<TSaved, THandler>(TSaved source) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
    {
        if (_loading) // When loading a new save we don't want existing components to write to the in-memory save
        {
            Unregister<TSaved, THandler>(source);
            return;
        }

        Store<TSaved, THandler>(source);
        Unregister<TSaved, THandler>(source);
    }

    /// <summary>
    /// Prevent this system from being automatically saved on the next call to <see cref="TrySaveToDisk"/>
    /// </summary>
    /// <remarks>
    /// If you need its data to be saved on the next call to <see cref="TrySaveToDisk"/>, but need to destroy it right now,
    /// call <see cref="StoreAndUnregister{TSaved,THandler}"/> instead.
    /// </remarks>
    public static void Unregister<TSaved, THandler>(TSaved source) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
    {
        if (_systemsByGuid.TryGetValue(source.UniqueConstID, out var system) && system == (ISaved)source)
            _systemsByGuid.Remove(source.UniqueConstID);
    }

    /// <summary>
    /// Will create a new file if <paramref name="fileToOverwrite"/> is null,
    /// otherwise overwrites the file on that path, but fails if <paramref name="fileToOverwrite"/> does not exist.
    /// </summary>
    /// <remarks>
    /// Returns false when it failed to save, a message box will be automatically shown in such a case.
    /// </remarks>
    public static bool TrySaveToDisk([CanBeNull] string fileToOverwrite = null)
    {
        try
        {
            var newSaveData = SaveToMemory();
            var json = JsonUtility.ToJson(newSaveData, true);

            Directory.CreateDirectory(GetSavesDirectory());

            bool forceNewFile = fileToOverwrite == null;
            // Explicit CreateNew to guarantee ownership when creating new files
            FileMode mode = forceNewFile ? FileMode.CreateNew : FileMode.Truncate;
            TRY_AGAIN:
            try
            {
                if (forceNewFile)
                    fileToOverwrite = Path.Combine(GetSavesDirectory(), $"Save_{DateTime.Now.Ticks}.{Extension}");

                using var stream = new FileStream(fileToOverwrite, mode, FileAccess.Write, FileShare.Write);
                using var writer = new StreamWriter(stream);
                writer.Write(json);
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 80 && forceNewFile)
            {
                goto TRY_AGAIN; // File already exists, try again
            }

            _latestSave = newSaveData;
            return true;
        }
        catch (Exception e)
        {
            MessageModal.Show("Failed to save", e.ToString(), MessageModal.Type.Error);
            return false;
        }
    }

    static Save SaveToMemory()
    {
        // Duplicate the current save to ensure we keep blocks/systems that have not registered
        var newSaveData = new Save(_latestSave.Instances, SpawnPoint.LastSpawnUsed.Reference);

        foreach (var (guid, system) in _systemsByGuid) // Could be done in parallel with minimal changes if necessary
        {
            if (system == null)
            {
                Debug.LogWarning($"System with GUID {guid} has been destroyed, but you didn't {nameof(Unregister)} it before hand. If you need to both destroy and save this system consider calling {nameof(StoreAndUnregister)} in that object's OnDestroy");
                continue;
            }

            system.StoreInto(newSaveData.Instances);
        }

        return newSaveData;
    }

    /// <summary>
    /// Attempts to load this file from disk and spawn the player at the scene this save file was at.
    /// May fail to do so if there are any IO issues, when the save is corrupted/invalid, or when the save does not contain the spawn point the player should start from.
    /// </summary>
    /// <remarks>
    /// A message box will be shown on screen with the appropriate information when it fails to load.
    /// </remarks>
    public static bool TryLoadFromDisk(string filePath)
    {
        try
        {
            _loading = true;
            SpawnPointReference spawn;
            Save newSave;
            try
            {
                string json = File.ReadAllText(filePath);
                newSave = JsonUtility.FromJson<Save>(json);
                if (newSave.Version != LatestVersion)
                    throw new NotImplementedException("Save version mismatch, version upgrading not implemented");

                if (_latestSave.SpawnPointReference == default)
                    throw new InvalidOperationException($"Empty id for spawn in save '{filePath}'");

                IdentifiableDatabase.TryGet(_latestSave.SpawnPointReference, out spawn);

                if (spawn == null)
                    throw new InvalidOperationException($"No spawn found in save '{filePath}'");
            }
            catch (Exception e)
            {
                Debug.LogException(new LoadingException($"Failed to Load {filePath}", e));
                MessageModal.Show($"Failed to Load {filePath}", e.Message, MessageModal.Type.Error);
                return false;
            }

            var beforeLoad = SaveToMemory(); // Get latest state and store it in case we need to rollback
            try
            {
                _latestSave = newSave;
                RestoreSystemsWith(newSave);
            }
            catch (Exception e)
            {
                Debug.LogException(new LoadingException($"Failed to Load {filePath}", e));
                MessageModal.Show($"Failed to Load {filePath}", e.Message, MessageModal.Type.Error);

                // Rollback saved states since we just failed to load the new one
                _latestSave = beforeLoad;
                RestoreSystemsWith(beforeLoad);
                return false;
            }

            spawn.SwapSceneToThisSpawnPoint(); // It is important for this call to destroy any existing systems that will be respawned
            return true;
        }
        finally
        {
            _loading = false;
        }

        static void RestoreSystemsWith(Save save)
        {
            foreach (var (guid, system) in _systemsByGuid)
            {
                if (system == null)
                {
                    Debug.LogWarning($"System with GUID {guid} has been destroyed, but you didn't {nameof(Unregister)} it before hand. If you need to both destroy and save this system consider calling {nameof(StoreAndUnregister)} in that object's OnDestroy");
                    continue;
                }

                if (save.Instances.TryGetValue(guid, out var data))
                    system.RestoreFrom(data);
            }
        }
    }

    public delegate void OnButtonPressed(string saveName);

    public static void FeedFileUI(UIElementList<SaveFileButton> buttons, OnButtonPressed OnButtonPressed)
    {
        // Interpret the Data on the read files
        buttons.Clear();
        foreach (var filePath in GetExistingSaveFilePaths())
        {
            buttons.Allocate(out var ui);

            #warning best to have the button already filled in that SaveFileButton class
            var button = ui.Button;
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(true);

            try
            {
                // Display name of the file
                ui.fileName.text = Path.GetFileNameWithoutExtension(filePath);

                button.onClick.AddListener(() => OnButtonPressed(filePath));

                var peeker = new SaveDataPeeking(filePath);

                // Set the Lineup of heroes
                if (peeker.TryPeek<GameManager, GameManager.SaveV1>(GameManager.Guid, out var managerData))
                {
                    for (int memberIndex = 0; memberIndex < managerData.Party.Length; memberIndex++)
                    {
                        if (PlayableCharacters.TryGet(managerData.Party[memberIndex], out var playable))
                            ui.characterPortraits[memberIndex].sprite = playable != null ? playable.Portrait : null;
                    }
                }

                if (peeker.TryPeek<GameManager, GameManager.SaveV1>(GameManager.Guid, out var clockData))
                    ui.timePlayed.text = clockData.TimeSpan.ToString(@"hh\:mm\:ss");

                if (peeker.TryPeek<InventoryManager, InventoryManager.SaveV1>(InventoryManager.Guid, out var inventoryData))
                    ui.moneyAcquired.text = $"{inventoryData.Credits} Credits";

                if (IdentifiableDatabase.TryGet(peeker.Save.SpawnPointReference, out SpawnPointReference spawn))
                {
                    ui.areaName.text = Path.GetFileNameWithoutExtension(spawn.Scene.Path);
                    ui.zoneName.text = spawn.SpawnName;
                }

            }
            catch (Exception e)
            {
                ui.fileName.text = $"Error:{e.Message}";
            }
        }
    }


    static string Extension => "json";
    static string[] GetExistingSaveFilePaths()
    {
        try
        {
            return Directory.GetFiles(GetSavesDirectory(), $"*.{Extension}");
        }
        catch(DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            MessageModal.Show("Failed to fetch save files", e.ToString(), MessageModal.Type.Error);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Not that this directory may not exist, you may have to create it if you intend to write to it
    /// </summary>
    static string GetSavesDirectory() => Path.Combine(Application.persistentDataPath, "Save Files");

    static void RegisterAndValidate<TSaved, THandler>(TSaved source) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
    {
        if (_systemsByGuid.TryGetValue(source.UniqueConstID, out var existing1) && ReferenceEquals(existing1, source))
            return; // Already valid, no need to check

        // ReSharper disable once EqualExpressionComparison
        if (new THandler().Version != new THandler().Version)
            throw new InvalidSerializationSetup($"{nameof(ISaveData.Version)} must always return the same value for a given type !");

        if (source is UnityEngine.Object == false && source.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length == 0)
            throw new InvalidSerializationSetup($"{source.GetType().Name} must be serializable");

        if (typeof(THandler).GetCustomAttributes(typeof(SerializableAttribute), true).Length == 0)
            throw new InvalidSerializationSetup($"{typeof(THandler).Name} must be serializable");

        if (_systemsByGuid.TryGetValue(source.UniqueConstID, out var existing) && ReferenceEquals(existing, source) == false)
        {
            if (existing is UnityEngine.Object uo && uo != null || existing != null)
                throw new InvalidSerializationSetup($"{source} is trying to register but another instance ({existing}) is using guid '{source.UniqueConstID}'");
        }

        if (_validatedTypes.Add(typeof(THandler)))
            new THandler().Validate();

        _systemsByGuid[source.UniqueConstID] = source;
    }

    [Serializable] public class Save
    {
        public uint Version = LatestVersion;
        public SerializableDictionary<guid, DataForInstance> Instances = new();
        public guid SpawnPointReference;

        public Save() { }

        public Save(SerializableDictionary<guid, DataForInstance> copy, SpawnPointReference spawn)
        {
            foreach (var instance in copy)
                Instances.Add(instance.Key, instance.Value);
            SpawnPointReference = spawn.Guid;
        }
    }

    [Serializable]
    public struct DataForInstance
    {
        public uint Version;
        // Could maybe use latest ISaveHandler instead of string, should take less memory but this shouldn't be that much of an issue right now.
        public string SerializedData;
    }

    public class SaveDataPeeking
    {
        public Save Save { get; private set; }

        public SaveDataPeeking(string filePath)
        {
            string json = File.ReadAllText(filePath);
            Save = new();
            JsonUtility.FromJsonOverwrite(json, Save);
        }

        public bool TryPeek<TSaved, THandler>(guid sourceGuid, out THandler handler) where TSaved : ISaved<TSaved, THandler> where THandler : ISaveHandler<TSaved>, new()
        {
            if (Save.Instances.TryGetValue(sourceGuid, out var data))
            {
                if (new THandler().TryDeserialize(data, out handler))
                    return true;

                throw new NoHandlerForVersionException($"Serialized data exist for '{sourceGuid}' but no handler could take care of {nameof(SavingSystem.Save.Version)} {data.Version}");
            }

            handler = default;
            return false;
        }
    }

    public class NoHandlerForVersionException : Exception
    {
        public NoHandlerForVersionException(string message) : base(message) { }
    }

    public class InvalidSerializationSetup : Exception
    {
        public InvalidSerializationSetup(string message) : base(message) { }
    }

    public class LoadingException : Exception
    {
        public LoadingException(string message, Exception e) : base(message, e) { }
    }

    public enum Transfer
    {
        /// <summary> Takes values stored in the source/game object, and assign them to the handler for saving </summary>
        PullFromSource,
        /// <summary> Takes values stored in the save/handler and assign them to the source </summary>
        PushToSource
    }
}

public interface ISaveData
{
    public uint Version { get; }

    /// <summary>
    /// May return false when this is not part of the save data, or when it's just this specific version, an anterior version may exist
    /// </summary>
    public bool TryDeserialize<T>(SavingSystem.DataForInstance saveData, out T deserialized) where T : ISaveData => TryDeserializeBase(saveData, out deserialized);

    protected bool TryDeserializeBase<T>(SavingSystem.DataForInstance saveData, out T deserialized) where T : ISaveData
    {
        if (saveData.Version == Version)
        {
            deserialized = JsonUtility.FromJson<T>(saveData.SerializedData);
            return true;
        }

        deserialized = default;
        return false;
    }

    void Validate() => ValidateBase();

    protected void ValidateBase()
    {
        foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsPublic == false && field.GetCustomAttribute<SerializeField>() is null || field.IsInitOnly)
                throw new SavingSystem.InvalidSerializationSetup($"{field.DeclaringType}.{field.Name}'s type is not Serializable");

            if (field.FieldType.IsPrimitive || field.FieldType.IsArray || field.FieldType.IsEnum || TypeWhitelist.Contains(field.FieldType))
                continue;

            if (field.FieldType.IsGenericType && typeof(List<>).IsAssignableFrom(field.FieldType.GetGenericTypeDefinition()))
                continue;

            if (field.FieldType.GetCustomAttribute<SerializableAttribute>() is null)
                throw new SavingSystem.InvalidSerializationSetup($"{field.DeclaringType}.{field.Name}'s type is not Serializable");

            if (field.FieldType.Assembly == typeof(TimeSpan).Assembly) // Even if they are marked as serializable, unity serializer can't handle those types
                throw new SavingSystem.InvalidSerializationSetup($"{field.DeclaringType}.{field.Name}'s type is not Serializable");
        }
    }

    static readonly HashSet<Type> TypeWhitelist = new()
    {
        typeof(string),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Vector2Int),
        typeof(Vector3Int),
        typeof(Color),
        typeof(Rect),
        typeof(Matrix4x4),
        typeof(AnimationCurve)
    };
}

public interface ISaveDataVersioned<in TPreviousVersion> : ISaveData where TPreviousVersion : ISaveData, new()
{
    public void UpgradeFromPrevious(TPreviousVersion old);

    bool ISaveData.TryDeserialize<T>(SavingSystem.DataForInstance saveData, out T deserialized)
    {
        // T should be this instance's actual type, this is just a workaround for the lack of c# 11

        if (TryDeserializeBase(saveData, out deserialized))
            return true;

        if (new TPreviousVersion().TryDeserialize<TPreviousVersion>(saveData, out var previous)) // Try with our previous version
        {
            UpgradeFromPrevious(previous);
            deserialized = (T)this;
            return true;
        }

        deserialized = default;
        return false;
    }

    void ISaveData.Validate()
    {
        var previous = new TPreviousVersion();
        if (Version <= previous.Version)
            throw new SavingSystem.InvalidSerializationSetup($"Invalid versioning setup, '{GetType().Name}' must be greater than '{new TPreviousVersion().GetType().Name}' ({Version} <= {new TPreviousVersion().Version})");
        ValidateBase();
        previous.Validate();
    }
}

public interface ISaved
{
    /// <summary>
    /// This GUID uniquely identifies a specific instance, e.g.: if this object is a specific character called 'Jim',
    /// every time the game runs 'Jim' should return the same exact GUID. On the other hand, another character named 'Bob' should not have the same GUID !
    /// </summary>
    public guid UniqueConstID { get; }
    void StoreInto(Dictionary<guid, SavingSystem.DataForInstance> saveData);
    void RestoreFrom(SavingSystem.DataForInstance saveData);
}

public interface ISaved<TSource, THandler> : ISaved where THandler : ISaveHandler<TSource>, new() where TSource : ISaved
{
    void ISaved.StoreInto(Dictionary<guid, SavingSystem.DataForInstance> saveData)
    {
        var handler = new THandler();
        handler.Transfer((TSource)this, SavingSystem.Transfer.PullFromSource);
        var json = JsonUtility.ToJson(handler);
        saveData[UniqueConstID] = new(){ Version = handler.Version, SerializedData = json };
    }

    void ISaved.RestoreFrom(SavingSystem.DataForInstance saveData)
    {
        if (new THandler().TryDeserialize<THandler>(saveData, out var deserialized))
        {
            deserialized.Transfer((TSource)this, SavingSystem.Transfer.PushToSource);
        }
        else
        {
            throw new SavingSystem.NoHandlerForVersionException($"Serialized data exist for '{this.GetType().Name}' with id '{this.UniqueConstID}' but no handler could take care of {nameof(SavingSystem.Save.Version)} {saveData.Version}");
        }
    }
}

public interface ISaveHandler : ISaveData
{
}

public interface ISaveHandler<in T> : ISaveHandler where T : ISaved
{
    void Transfer(T source, SavingSystem.Transfer transfer);
}
