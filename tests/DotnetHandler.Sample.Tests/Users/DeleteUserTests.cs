using DotnetHandler.Sample.Tests.Helpers;
using Xunit;

namespace DotnetHandler.Sample.Tests.Users;

public class DeleteUserTests
{
    [Fact]
    public async Task Deletes_existing_user_and_returns_true()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Jack", "jack@example.com"));

        var deleted = await dispatcher.Send(new DeleteUserCommand(created.Id));

        Assert.True(deleted);
    }

    [Fact]
    public async Task Deleted_user_is_no_longer_found()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Kate", "kate@example.com"));

        await dispatcher.Send(new DeleteUserCommand(created.Id));
        var fetched = await dispatcher.Send(new GetUserQuery(created.Id));

        Assert.Null(fetched);
    }

    [Fact]
    public async Task Returns_false_for_unknown_id()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var deleted = await dispatcher.Send(new DeleteUserCommand(Guid.NewGuid()));

        Assert.False(deleted);
    }

    [Fact]
    public async Task Delete_removes_only_the_target_user()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var u1 = await dispatcher.Send(new CreateUserCommand("Leo", "leo@example.com"));
        var u2 = await dispatcher.Send(new CreateUserCommand("Mia", "mia@example.com"));

        await dispatcher.Send(new DeleteUserCommand(u1.Id));

        var remaining = await dispatcher.Send(new GetUsersQuery());
        Assert.Single(remaining);
        Assert.Equal(u2.Id, remaining[0].Id);
    }
}
