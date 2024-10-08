

using Org.BouncyCastle.Security;

public static class Utilities
{
    /// <returns>True if all the necessary env vars are found and not empty. False otherwise.</returns>
    public static bool AllEnvVarsExist()
    {
        string? baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        if (string.IsNullOrEmpty(baseApiUrl))
            return false;    

        return true;
    }
}

