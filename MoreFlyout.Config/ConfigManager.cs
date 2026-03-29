namespace MoreFlyout.Config;

public class ConfigManager
{
    private static readonly string _ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MoreFlyout", "config.yaml");
    private static readonly YamlStorage<ConfigModel> _Storage = new(_ConfigPath);
    private static FileSystemWatcher? _FileWatcher;
    private static Timer? _DebounceTimer;
    private static readonly Lock _DebounceLock = new();

    public static event EventHandler? ConfigChanged;

    public static ConfigModel Instance
    {
        get { return field ??= _Storage.Load(); }
        private set;
    }

    /// <summary>
    /// Save the current configuration in memory to a file
    /// </summary>
    public static void Save()
    {
        _Storage.Save(Instance);
    }

    /// <summary>
    /// Reload configuration from file (undo unsaved changes)
    /// </summary>
    public static void Reload()
    {
        Instance = _Storage.Load();
    }

    /// <summary>
    /// Start watching the configuration file for changes
    /// </summary>
    public static void StartWatching()
    {
        if (_FileWatcher is not null)
        {
            return;
        }

        var configDirectory = Path.GetDirectoryName(_ConfigPath);
        var configFileName = Path.GetFileName(_ConfigPath);

        if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(configFileName))
        {
            return;
        }

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        _FileWatcher = new FileSystemWatcher(configDirectory, configFileName) { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size, EnableRaisingEvents = true };

        _FileWatcher.Changed += OnConfigFileChanged;
    }

    /// <summary>
    /// Stop watching the configuration file
    /// </summary>
    public static void StopWatching()
    {
        if (_FileWatcher is not null)
        {
            _FileWatcher.Changed -= OnConfigFileChanged;
            _FileWatcher.Dispose();
            _FileWatcher = null;
        }

        _DebounceTimer?.Dispose();
        _DebounceTimer = null;
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (_DebounceLock)
        {
            _DebounceTimer?.Dispose();
            _DebounceTimer = new Timer(
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
