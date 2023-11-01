using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpStack;

public class HttpApplicationBuilder : IHttpApplicationBuilder, IServiceProviderFactory<IServiceCollection>
#if NET8_0_OR_GREATER
    , IHostApplicationBuilder
#endif
{
    private IHttpStackBuilder? _application;
    private Func<IServiceProvider> _createServiceProvider;
    private Action<object> _configureContainer = _ => { };
    private ConfigureHostBuilder? _hostBuilder;

    public HttpApplicationBuilder()
        : this(new ServiceCollection())
    {
    }

    public HttpApplicationBuilder(IServiceCollection services)
    {
        services.AddSingleton<ApplicationLifetime>();
        services.AddSingleton<IHostApplicationLifetime>(s => s.GetRequiredService<ApplicationLifetime>());
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddSingleton<IApplicationLifetime>(s => s.GetRequiredService<ApplicationLifetime>());
#pragma warning restore CS0618 // Type or member is obsolete

        Services = services;
        Logging = new LoggingBuilder(Services);
        Configuration = new ConfigurationManager();
        Environment = new HostEnvironment();
        _createServiceProvider = () =>
        {
            // Call _configureContainer in case anyone adds callbacks via HostBuilderAdapter.ConfigureContainer<IServiceCollection>() during build.
            // Otherwise, this no-ops.
            _configureContainer(Services);
            return Services.BuildServiceProvider();
        };
    }

    public ConfigureHostBuilder Host => _hostBuilder ??= new ConfigureHostBuilder(
        new HostBuilderContext(Properties)
        {
            Configuration = Configuration,
            HostingEnvironment = Environment
        },
        Configuration,
        Services);

    public IServiceCollection Services { get; }

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

    public ConfigurationManager Configuration { get; }

    public HostEnvironment Environment { get; }
    public ILoggingBuilder Logging { get; }

    public IHttpStackBuilder Build()
    {
        if (_application != null)
        {
            return _application;
        }

        _hostBuilder?.RunDeferredCallbacks(this);

        var hostBuilder = new HostBuilder();

        Services.AddSingleton<IHostEnvironment>(Environment);
        Services.AddSingleton<IConfiguration>(Configuration);

        hostBuilder.UseServiceProviderFactory(new HostBuilderAdapter(this));

        hostBuilder.ConfigureAppConfiguration(builder =>
        {
            builder.AddConfiguration(Configuration);
        });

        var host = hostBuilder.Build();

        var result = new HttpStackBuilder(host.Services, host);

        _application = result;
        return result;
    }

    private class HostBuilderAdapter : IServiceProviderFactory<IServiceCollection>
    {
        private readonly HttpApplicationBuilder _httpApplicationBuilder;

        public HostBuilderAdapter(HttpApplicationBuilder httpApplicationBuilder)
        {
            _httpApplicationBuilder = httpApplicationBuilder;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            foreach (var service in services)
            {
                _httpApplicationBuilder.Services.Add(service);
            }

            return _httpApplicationBuilder.Services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return _httpApplicationBuilder._createServiceProvider();
        }
    }

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _createServiceProvider = () =>
        {
            TContainerBuilder containerBuilder = factory.CreateBuilder(Services);
            // Call _configureContainer in case anyone adds more callbacks via HostBuilderAdapter.ConfigureContainer<TContainerBuilder>() during build.
            // Otherwise, this is equivalent to configure?.Invoke(containerBuilder).
            _configureContainer(containerBuilder);
            return factory.CreateServiceProvider(containerBuilder);
        };

        // Store _configureContainer separately so it can replaced individually by the HostBuilderAdapter.
        _configureContainer = containerBuilder => configure?.Invoke((TContainerBuilder)containerBuilder);
    }

    public void ConfigureContainer<TContainerBuilder>(Action<TContainerBuilder> configureDelegate)
    {
        _configureContainer = containerBuilder => configureDelegate((TContainerBuilder)containerBuilder);
    }

    IHostEnvironment IHttpApplicationBuilder.Environment => Environment;

#if NET8_0_OR_GREATER
    private Microsoft.Extensions.Diagnostics.Metrics.IMetricsBuilder? _metricsBuilder;

    IHostEnvironment IHostApplicationBuilder.Environment => Environment;

    IConfigurationManager IHostApplicationBuilder.Configuration => Configuration;

    Microsoft.Extensions.Diagnostics.Metrics.IMetricsBuilder IHostApplicationBuilder.Metrics => _metricsBuilder ??= new MetricsBuilder(Services);
#endif

    IServiceCollection IServiceProviderFactory<IServiceCollection>.CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    IServiceProvider IServiceProviderFactory<IServiceCollection>.CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return _createServiceProvider();
    }
}

public class ConfigureHostBuilder : IHostBuilder
{
    private readonly ConfigurationManager _configuration;
    private readonly IServiceCollection _services;
    private readonly HostBuilderContext _context;

    private readonly List<Action<HttpApplicationBuilder>> _operations = new();

    internal ConfigureHostBuilder(HostBuilderContext context, ConfigurationManager configuration, IServiceCollection services)
    {
        _configuration = configuration;
        _services = services;
        _context = context;
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        configureDelegate(_configuration);
        return this;
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        // Run these immediately so that they are observable by the imperative code
        configureDelegate(_context, _configuration);
        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        // Run these immediately so that they are observable by the imperative code
        configureDelegate(_context, _services);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        where TContainerBuilder : notnull
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _operations.Add(b => b.ConfigureContainer(factory));
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        where TContainerBuilder : notnull
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _operations.Add(b => b.ConfigureContainer(factory(_context)));
        return this;
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        if (configureDelegate is null)
        {
            throw new ArgumentNullException(nameof(configureDelegate));
        }

        _operations.Add(b => b.ConfigureContainer<TContainerBuilder>(builder => configureDelegate(_context, builder)));
        return this;
    }

    IHost IHostBuilder.Build()
    {
        throw new NotSupportedException($"Call {nameof(HttpApplicationBuilder)}.{nameof(HttpApplicationBuilder.Build)}() instead.");
    }

    internal void RunDeferredCallbacks(HttpApplicationBuilder builder)
    {
        foreach (var operation in _operations)
        {
            operation(builder);
        }
    }

    /// <inheritdoc />
    public IDictionary<object, object> Properties => _context.Properties;
}
