namespace MembershipSystemAPI.Services;

/// <summary>
/// CDK服务接口，用于生成唯一的会员卡代码
/// </summary>
public interface ICdkService
{
    /// <summary>
    /// 根据金额生成CDK码
    /// </summary>
    /// <param name="amount">金额</param>
    /// <returns>生成的CDK码</returns>
    string GenerateCdk(decimal amount);
}
