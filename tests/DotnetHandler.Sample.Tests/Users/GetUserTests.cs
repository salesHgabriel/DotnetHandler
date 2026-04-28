using DotnetHandler.Sample.Tests.Helpers;
using Xunit;

namespace DotnetHandler.Sample.Tests.Users;

public class GetUserTests
{
    [Fact]
    public async Task Returns_user_by_id()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Carol", "carol@example.com"));

        var result = await dispatcher.Send(new GetUserQuery(created.Id));

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Carol", result.Name);
        Assert.Equal("carol@example.com", result.Email);
    }

    [Fact]
    public async Task Returns_null_for_unknown_id()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var result = await dispatcher.Send(new GetUserQuery(Guid.NewGuid()));

        Assert.Null(result);
    }

    [Fact]
    public async Task Returns_all_users()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        await dispatcher.Send(new CreateUserCommand("Dave", "dave@example.com"));
        await dispatcher.Send(new CreateUserCommand("Eve", "eve@example.com"));

        var users = await dispatcher.Send(new GetUsersQuery());

        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Name == "Dave");
        Assert.Contains(users, u => u.Name == "Eve");
    }

    [Fact]
    public async Task Returns_empty_list_when_no_users()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var users = await dispatcher.Send(new GetUsersQuery());

        Assert.Empty(users);
    }
}
