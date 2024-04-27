using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionScenario;

public class Scenarios
{
    [Test]
    [Ignore("This one should fail.")]
    public void TypeMapping_Wrong()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<DisposableService>()
            .AddTransient<IDisposableService1, DisposableService>()
            .AddScoped<IDisposableService2>(provider => provider.GetRequiredService<DisposableService>());

        using var serviceProvider = services.BuildServiceProvider();

        var singleton = serviceProvider.GetRequiredService<DisposableService>();

        Assert.Multiple(() =>
        {
            using (var scoped = serviceProvider.CreateScope())
            {
                var interface1 = scoped.ServiceProvider.GetRequiredService<IDisposableService1>();
                Assert.That(interface1, Is.EqualTo(singleton), "[Failed]MS.DI registration is not service mapping.");

                var interface2 = scoped.ServiceProvider.GetRequiredService<IDisposableService2>();
                Assert.That(interface2, Is.EqualTo(singleton), "[Pass]");
            }

            Assert.That(singleton.Disposed, Is.Not.True,
                "[Failed]Mapping service's lifetime should be same to the mapped service.");
        });
    }

    [Test]
    public void TypeMapping_Correct()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<DisposableService>()
            .AddSingleton<IDisposableService1, DisposableService>(provider =>
                provider.GetRequiredService<DisposableService>())
            .AddSingleton<IDisposableService2>(provider => provider.GetRequiredService<DisposableService>());

        DisposableService singleton;
        using (var serviceProvider = services.BuildServiceProvider())
        {
            singleton = serviceProvider.GetRequiredService<DisposableService>();

            Assert.Multiple(() =>
            {
                using (var scoped = serviceProvider.CreateScope())
                {
                    var interface1 = scoped.ServiceProvider.GetRequiredService<IDisposableService1>();
                    Assert.That(interface1, Is.EqualTo(singleton));

                    var interface2 = scoped.ServiceProvider.GetRequiredService<IDisposableService2>();
                    Assert.That(interface2, Is.EqualTo(singleton));
                }

                Assert.That(singleton.Disposed, Is.Not.True);
            });
        }

        Assert.That(singleton.Disposed, Is.True);
    }

    interface IBooleanDisposable : IDisposable
    {
        public bool Disposed { get; }
    }

    interface IDisposableService1 : IBooleanDisposable
    {
    }

    interface IDisposableService2 : IBooleanDisposable
    {
    }

    class DisposableService : IDisposableService1, IDisposableService2
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}