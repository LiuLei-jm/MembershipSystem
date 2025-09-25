using System.Text;

namespace MembershipSystemAPI.Services;

public class CdkService : ICdkService
{
    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public string GenerateCdk(decimal amount)
    {
        var randomPart = new StringBuilder(16);
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[1];
        for (int i = 0; i < 16; i++)
        {
            rng.GetBytes(buffer);
            var idx = buffer[0] % _chars.Length;
            randomPart.Append(_chars[idx]);
        }
        var amountPart = ((int)(amount)).ToString("D4");
        return $"{randomPart}{amountPart}";
    }
}
