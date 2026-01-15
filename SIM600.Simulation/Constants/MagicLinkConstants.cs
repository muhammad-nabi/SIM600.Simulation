namespace SIM600.Simulation.Constants;

public static class MagicLinkConstants
{
    public const string TokenPurpose = "MagicLinkLogin";
    public const int TokenLifespanMinutes = 15;
    public const int MaxRequestsPerWindow = 3;
    public const int RateLimitWindowMinutes = 15;
}
