using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRQueries;

// 用户相关的查询
public record GetUserByIdQuery(Guid UserId, bool IncludeDetails = false) : IRequest<UserDto?>;

public record GetUserByUsernameQuery(string Username, bool IncludeApiKey = false) : IRequest<UserDto?>;

public record GetAllUsersQuery(bool OnlyActive = false) : IRequest<IEnumerable<UserDto>>;

public record GetUsersWithPendingMembershipsQuery : IRequest<IEnumerable<UserDto>>;

public record ValidateUserCredentialsQuery(
    string Username,
    string Password
) : IRequest<ValidateUserResult>;

// query results
public record ValidateUserResult(
    User User,
    bool IsValid,
    string Message = "",
    bool IsLockedOut = false
);