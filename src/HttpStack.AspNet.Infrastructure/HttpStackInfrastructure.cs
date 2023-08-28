using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using HttpStack.AspNet;
using HttpStack.AspNet.Providers;
using HttpStack.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(HttpStackInfrastructure), nameof(HttpStackInfrastructure.Start))]

namespace HttpStack.AspNet;

public class HttpStackInfrastructure
{
    private static readonly Lazy<IHttpStackStartup?> LazyStartup = new(() =>
    {
        var typeName = WebConfigurationManager.AppSettings["httpstack:appStartup"];

        Type? startupType = null;

        if (!string.IsNullOrEmpty(typeName))
        {
            startupType = Type.GetType(typeName, false);

            if (startupType is null)
            {
                throw new InvalidOperationException($"The startup type configured in the httpstack:appStartup setting ({typeName}) could not be found.");
            }
        }

        startupType ??= AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetCustomAttributes<HttpStackStartupAttribute>())
            .SingleOrDefault()
            ?.Type;

        startupType ??= AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t is not null);
                }
            })
            .FirstOrDefault(i => i.IsClass && !i.IsAbstract && typeof(IHttpStackStartup).IsAssignableFrom(i));

        return startupType is not null
            ? (IHttpStackStartup)Activator.CreateInstance(startupType)
            : null;
    });

    public static IHttpStackStartup? Startup => LazyStartup.Value;

    private static IServiceProvider? _provider;

    public static void Start()
    {
        DynamicModuleUtility.RegisterModule(typeof(LifeCycleModule));
    }

    private sealed class LifeCycleModule : IHttpModule
    {
        private static readonly object InitLock = new();
        private static readonly SemaphoreSlim HostLock = new(1, 1);
        private static bool _isInitialized;
        private static int _initializedModuleCount;
        private static IHttpStack<HttpContext>? _stack;

        /// <summary>
        /// Initialize the service provider.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication? application)
        {
            if (application is null)
            {
                return;
            }

            var wrapper = new EventHandlerTaskAsyncHelper(ExecuteStackAsync);
            application.AddOnPostResolveRequestCacheAsync(wrapper.BeginEventHandler, wrapper.EndEventHandler);

            lock (InitLock)
            {
                _initializedModuleCount++;

                if (_initializedModuleCount != 1 || _isInitialized)
                {
                    return;
                }

                _isInitialized = true;
                
                var serviceProviderFactoryType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetCustomAttributes<HttpServiceProviderFactoryAttribute>())
                    .SingleOrDefault();

                var serviceProviderFactory = serviceProviderFactoryType != null
                    ? (IHttpServiceProviderFactory)Activator.CreateInstance(serviceProviderFactoryType.Type)
                    : new DefaultHttpServiceProviderFactory();

                var provider = serviceProviderFactory.CreateRootProvider(application, Startup);
                _provider = provider;
                StartHostedServicesInBackground(provider);

                if (Startup is {} startup)
                {
                    var builder = new HttpStackBuilder(provider);
                    startup.Configure(builder);
                    _stack = builder.CreateAspNetStack(
                        new AspNetContextScopeProvider(serviceProviderFactory, provider),
                        endRequest: false
                    );
                }
                else
                {
                    _stack = null;
                }
            }
        }

        /// <summary>
        /// Execute the stack.
        /// </summary>
        /// <param name="sender">The application.</param>
        /// <param name="e">The event arguments.</param>
        private static async Task ExecuteStackAsync(object sender, EventArgs e)
        {
            if (_stack is not {} stack || sender is not HttpApplication { Context: {} context })
            {
                return;
            }

            await stack.ProcessRequestAsync(context);

            if (ReferenceEquals(context.Items[Globals.DidFinishStackKey], Globals.BoxedFalse))
            {
                // Map a void handler to the request to prevent the default handler from running.
                context.RemapHandler(VoidHandler.Instance);
            }
        }

        /// <summary>
        /// Dispose the service provider and stop all hosted services.
        /// </summary>
        public void Dispose()
        {
            lock (InitLock)
            {
                _initializedModuleCount--;

                if (_initializedModuleCount != 0 || !_isInitialized)
                {
                    return;
                }

                _isInitialized = false;

                if (_provider is not { } provider)
                {
                    return;
                }

                _provider = null;

                HostingEnvironment.QueueBackgroundWorkItem(async _ =>
                {
                    await StopHostedServicesAsync(provider);

                    if (provider is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    else if (provider is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                });
            }
        }

        /// <summary>
        /// Start hosted services.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        private static void StartHostedServicesInBackground(IServiceProvider provider)
        {
            var hostedServices = provider.GetServices<IHostedService>().ToArray();

            if (hostedServices.Length == 0)
            {
                return;
            }

            HostingEnvironment.QueueBackgroundWorkItem(async token =>
            {
                await HostLock.WaitAsync(token);

                try
                {
                    foreach (var hostedService in hostedServices)
                    {
                        await hostedService.StartAsync(token);
                    }
                }
                finally
                {
                    HostLock.Release();
                }
            });
        }

        /// <summary>
        /// Stop hosted services.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        private static async Task StopHostedServicesAsync(IServiceProvider provider)
        {
            var hostedServices = provider.GetServices<IHostedService>().ToArray();

            if (hostedServices.Length == 0)
            {
                return;
            }

            await HostLock.WaitAsync();

            try
            {
                foreach (var hostedService in hostedServices)
                {
                    await hostedService.StopAsync(default);
                }
            }
            finally
            {
                HostLock.Release();
            }
        }
    }

}
