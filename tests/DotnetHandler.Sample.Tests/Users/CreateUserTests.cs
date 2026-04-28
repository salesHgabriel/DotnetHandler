using DotnetHandler.Sample.Tests.Helpers;
using DotnetHandler.Validation;
using Xunit;

namespace DotnetHandler.Sample.Tests.Users;

public class CreateUserTests
{
    [Fact]
    public async Task Creates_user_and_returns_response()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var result = await dispatcher.Send(new CreateUserCommand("Alice", "alice@example.com"));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task Created_user_is_persisted()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var created = await dispatcher.Send(new CreateUserCommand("Bob", "bob@example.com"));
        var fetched = await dispatcher.Send(new GetUserQuery(created.Id));

        Assert.NotNull(fetched);
        Assert.Equal("Bob", fetched.Name);
    }

    [Fact]
    public async Task Rejects_empty_name()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateUserCommand("", "alice@example.com")));

        Assert.Contains("Name is required.", ex.Errors);
    }

    [Fact]
    public async Task Rejects_invalid_email()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateUserCommand("Alice", "not-an-email")));

        Assert.Contains("A valid email address is required.", ex.Errors);
    }

    [Fact]
    public async Task Rejects_multiple_validation_errors()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateUserCommand("", "")));

        Assert.Contains("Name is required.", ex.Errors);
        Assert.Contains("Email is required.", ex.Errors);
    }

    [Fact]
    public async Task Rejects_name_exceeding_max_length()
    {
        var sp = SampleServiceFactory.Build();
        var dispatcher = SampleServiceFactory.Dispatcher(sp);
        var longName = new string('A', 101);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateUserCommand(longName, "alice@example.com")));

        Assert.Contains("Name must not exceed 100 characters.", ex.Errors);
    }
}
