
using FastEndpoints;
using FastEndpoints.Swagger;
using MembershipSystemAPI.Hubs;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();
bld.Services.AddSwaggerDocument();
bld.Services.AddSignalR();

var app = bld.Build();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.MapHub<CommandHub>("/commandHub");
app.Run();
