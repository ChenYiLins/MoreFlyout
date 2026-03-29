namespace MoreFlyout.Config;

public class ConfigManager
{
    private static readonly string ConfigPath = CommonHelper.ConfigPath;
    private static readonly YamlStorage<ConfigModel> Storage = new(ConfigPath);
    private static FileSystemWatcher? FileWatcher;
    private static Timer? DebounceTimer;
    private static readonly Lock DebounceLock = new();

    public static event EventHandler? ConfigChanged;

    public static ConfigModel Instance
    {
        get { return field ??= Storage.Load(); }
        private set;
    }

    /// <summary>
    /// Save the current configuration in memory to a file
    /// </summary>
    public static void Save()
    {
        Storage.Save(Instance);
    }

    /// <summary>
    /// Reload configuration from file (undo unsaved changes)
    /// </summary>
    public static void Reload()
    {
        Instance = Storage.Load();
    }

    /// <summary>
    /// Start watching the configuration file for changes
    /// </summary>
    public static void StartWatching()
    {
        if (FileWatcher is not null)
        {
            return;
        }

        var configDirectory = Path.GetDirectoryName(ConfigPath);
        var configFileName = Path.GetFileName(ConfigPath);

        if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(configFileName))
        {
            return;
        }

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        FileWatcher = new FileSystemWatcher(configDirectory, configFileName) { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size, EnableRaisingEvents = true };

        FileWatcher.Changed += OnConfigFileChanged;
    }

    /// <summary>
    /// Stop watching the configuration file
    /// </summary>
    public static void StopWatching()
    {
        if (FileWatcher is not null)
        {
            FileWatcher.Changed -= OnConfigFileChanged;
            FileWatcher.Dispose();
            FileWatcher = null;
        }

        DebounceTimer?.Dispose();
        DebounceTimer = null;
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (DebounceLock)
        {
            DebounceTimer?.Dispose();
            DebounceTimer = new Timer(
                _ =>
                {
                    try
                    {
                        Reload();
                        ConfigChanged?.Invoke(null, EventArgs.Empty);
                    }
                    catch
                    {
                        // Ignore errors during reload (file might be locked temporarily)
                    }
                },
                null,
                200,
                Timeout.Infinite
            );
        }
    }
}
