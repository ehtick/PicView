using PicView.Core.Config.ConfigFileManagement;

namespace PicView.Core.Keybindings;

/// <summary>
/// Represents the configuration for key bindings in the application.
/// This class extends <see cref="ConfigFile"/> and specifies "keybindings.json" as its configuration file name.
/// </summary>
public class KeyBindingsConfiguration() : ConfigFile("keybindings.json");