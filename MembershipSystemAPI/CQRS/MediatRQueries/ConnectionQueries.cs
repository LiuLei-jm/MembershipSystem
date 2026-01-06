namespace MembershipSystemAPI.CQRS.MediatRQueries;

using ConnectionInfoEntity = MembershipSystemAPI.Domain.Entities.ConnectionInfo;

// 用户连接客户端相关的查询
public record GetUserConnectionClientsQuery(
    Guid CurrentUserId
) : IRequest<IEnumerable<ConnectionInfoEntity>>;

// 管理员获取所有连接客户端的查询
public record GetAllConnectionClientsQuery : IRequest<IEnumerable<ConnectionInfoEntity>>;