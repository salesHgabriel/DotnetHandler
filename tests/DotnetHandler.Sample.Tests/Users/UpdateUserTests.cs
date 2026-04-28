using DotnetHandler.Sample.Tests.Helpers;
using DotnetHandler.Validation;
using Xunit;

namespace DotnetHandler.Sample.Tests.Users;

public class UpdateUserTests
{
    [Fact]
    public async Task Updates_existing_user()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Frank", "frank@example.com"));

        var result = await dispatcher.Send(new UpdateUserCommand(created.Id, "Franklin", "franklin@example.com"));

        Assert.NotNull(result);
        Assert.Equal("Franklin", result.Name);
        Assert.Equal("franklin@example.com", result.Email);
    }

    [Fact]
    public async Task Persists_updated_values()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Grace", "grace@example.com"));

        await dispatcher.Send(new UpdateUserCommand(created.Id, "Gracie", "gracie@example.com"));
        var fetched = await dispatcher.Send(new GetUserQuery(created.Id));

        Assert.Equal("Gracie", fetched!.Name);
        Assert.Equal("gracie@example.com", fetched.Email);
    }

    [Fact]
    public async Task Returns_null_for_unknown_id()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var result = await dispatcher.Send(new UpdateUserCommand(Guid.NewGuid(), "X", "x@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Rejects_empty_name()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Hank", "hank@example.com"));

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new UpdateUserCommand(created.Id, "", "hank@example.com")));

        Assert.Contains("Name is required.", ex.Errors);
    }

    [Fact]
    public async Task Rejects_invalid_email()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var created = await dispatcher.Send(new CreateUserCommand("Iris", "iris@example.com"));

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new UpdateUserCommand(created.Id, "Iris", "not-valid")));

        Assert.Contains("A valid email address is required.", ex.Errors);
    }
}
