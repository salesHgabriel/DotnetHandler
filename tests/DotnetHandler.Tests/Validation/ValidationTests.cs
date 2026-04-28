using DotnetHandler.Abstractions;
using DotnetHandler.Registration;
using DotnetHandler.Tests.Helpers;
using DotnetHandler.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotnetHandler.Tests.Validation;

public class ValidationTests
{
    private static IDispatcher BuildDispatcher() =>
        ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<CreateItemCommand, string>().HandledBy<CreateItemHandler>())))
        .GetRequiredService<IDispatcher>();

    [Fact]
    public async Task Send_PassesValidation_ExecutesHandler()
    {
        var dispatcher = BuildDispatcher();
        var result = await dispatcher.Send(new CreateItemCommand("Widget"));
        Assert.Equal("Created: Widget", result);
    }

    [Fact]
    public async Task Send_FailsValidation_ThrowsValidationException()
    {
        var dispatcher = BuildDispatcher();

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateItemCommand("")));

        Assert.Contains("Name is required", ex.Errors);
    }

    [Fact]
    public async Task Send_FailsValidation_HandlerIsNeverCalled()
    {
        CreateItemHandler.HandlerWasCalled = false;

        var dispatcher = BuildDispatcher();

        await Assert.ThrowsAsync<ValidationException>(
            () => dispatcher.Send(new CreateItemCommand("")));

        Assert.False(CreateItemHandler.HandlerWasCalled);
    }

    [Fact]
    public async Task Send_HandlerWithoutValidationInterface_ExecutesNormally()
    {
        var provider = ServiceProviderFactory.Build(s =>
            s.AddDotnetHandler(app =>
                app.Handlers(h =>
                    h.Register<PingRequest, string>().HandledBy<PingHandler>())));

        var dispatcher = provider.GetRequiredService<IDispatcher>();
        var result = await dispatcher.Send(new PingRequest("no-validation"));

        Assert.Equal("Pong: no-validation", result);
    }

    [Fact]
    public async Task ValidationResult_Success_HasNoErrors()
    {
        var result = ValidationResult.Success();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidationResult_Failure_HasErrors()
    {
        var result = ValidationResult.Failure("Error 1", "Error 2");
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ValidationException_ExposesErrors()
    {
        var ex = new ValidationException(new[] { "A", "B" });
        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal("Validation failed", ex.Message);
    }
}
