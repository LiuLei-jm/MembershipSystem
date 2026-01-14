using System.Security.Cryptography;
using System.Text;

namespace MembershipSystemAPI.Services;

/// <summary>
/// CDK服务实现类，用于生成唯一的会员卡代码
/// </summary>
public class CdkService : ICdkService
{
    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// 根据金额生成CDK码
    /// </summary>
    /// <param name="amount">金额</param>
    /// <returns>生成的CDK码</returns>
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
