namespace SIM600.Simulation.Services;

public class AzureEmailOptions
{
    public const string SectionName = "AzureEmailSettings";

    /// <summary>
    /// Azure Communication Services connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The sender email address (must be verified in Azure portal)
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the sender
    /// </summary>
    public string SenderDisplayName { get; set; } = "SIM600 Simulation";
}
