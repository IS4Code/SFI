using IS4.SFI.Application.Plugins;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExecutionContext = NuGet.ProjectManagement.ExecutionContext;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NuGetLogLevel = NuGet.Common.LogLevel;

namespace IS4.SFI.Application.Tools.NuGet
{
    /// <summary>
    /// Provides resolution of plugins retrieved via NuGet.
    /// </summary>
    public class NuGetPluginResolver : NuGetProject, IPluginResolver, INuGetProjectContext, ISettings
    {
        readonly ILogger? logger;
        readonly ISettings? settings;
        readonly Dictionary<string, NuGetVersion> runtimeLibraries = new();

        readonly ConcurrentDictionary<PackageIdentity, PackageReference> installedPackages = new();

        static readonly SourceCacheContext sourceCacheContext = new();
        static readonly GatherCache gatherCache = new();

        readonly IEnumerable<SourceRepository> repositories;
        readonly NuGetPackageManager manager;

        /// <summary>
        /// The current framework used to load the package data.
        /// </summary>
        public NuGetFramework TargetFramework => GetMetadata<NuGetFramework>(NuGetProjectMetadataKeys.TargetFramework);

        /// <summary>
        /// All packages installed during retrieval.
        /// </summary>
        public ICollection<PackageReference> InstalledPackages => installedPackages.Values;

        NuGetPlugin? currentPlugin;

        /// <summary>
        /// Creates a new instance of the resolver.
        /// </summary>
        /// <param name="name">The display name of the project.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="settings">Additional NuGet settings.</param>
        /// <param name="dependencyContext">The <see cref="DependencyContext"/> instance describing the currently loaded packages.</param>
        /// <exception cref="InvalidOperationException">No <see cref="DependencyContext"/> instance could be loaded from the application.</exception>
        public NuGetPluginResolver(string name, ILogger? logger = null, ISettings? settings = null, DependencyContext? dependencyContext = null)
        {
            this.logger = logger;
            this.settings = settings;

            dependencyContext ??= DependencyContext.Default ?? throw new InvalidOperationException("Dependencies for the current application cannot be loaded.");
            var framework = NuGetFramework.Parse(dependencyContext.Target.Framework);

            InternalMetadata[NuGetProjectMetadataKeys.Name] = name;
            InternalMetadata[NuGetProjectMetadataKeys.TargetFramework] = framework;

            foreach(var library in dependencyContext.RuntimeLibraries)
            {
                var libraryName = library.Name;
                if(library.Type == "runtimepack" && libraryName.StartsWith("runtimepack.", StringComparison.Ordinal))
                {
                    libraryName = libraryName.Substring("runtimepack.".Length);
                }else if(library.Type is not ("package" or "project"))
                {
                    continue;
                }
                var version = NuGetVersion.Parse(library.Version);
                runtimeLibraries[libraryName] = version;
            }

            var sourceProvider = new PackageSourceProvider(this, new[]
            {
                new PackageSource(Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome), "packages")),
                new PackageSource(NuGetConstants.V3FeedUrl)
            });

            var resourceProviders = Repository.Provider.GetCoreV3();
            var repositoryProvider = new SourceRepositoryProvider(sourceProvider, resourceProviders);
            var folder = Path.Combine(AppContext.BaseDirectory, "packages");

            manager = new NuGetPackageManager(repositoryProvider, this, folder);
            repositories = repositoryProvider.GetRepositories();
        }

        static readonly char[] idVersionSeparators = { '/' };

        /// <inheritdoc/>
        public async ValueTask<Plugin> GetPluginAsync(string id, CancellationToken cancellationToken)
        {
            var split = id.Split(idVersionSeparators);
            var idName = split[0];

            NuGetVersion? version = null;
            if(split.Length <= 1)
            {
                LoggerAdapter? logger = null;
                foreach(var repository in repositories)
                {
                    var find = await repository.GetResourceAsync<FindPackageByIdResource>();
                    logger ??= new(this);
                    var allVersions = await find.GetAllVersionsAsync(idName, sourceCacheContext, logger, cancellationToken);
                    var relVersions = allVersions.Where(v => !v.IsPrerelease);
                    if(relVersions.Any())
                    {
                        version = relVersions.Max();
                    }else if(allVersions.Any())
                    {
                        version = allVersions.Max();
                    }
                }
                if(version == null)
                {
                    throw new ArgumentException($"The package {idName} could not be resolved.", nameof(id));
                }
            }else if(split.Length > 2)
            {
                throw new ArgumentException($"The package identifier '{id}' has unrecognized format.", nameof(id));
            }else{
                version = NuGetVersion.Parse(split[1]);
            }
            var identity = new PackageIdentity(idName, version);

            var resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest,
                true,
                true,
                VersionConstraints.None,
                gatherCache,
                sourceCacheContext
            );

            IEnumerable<NuGetProjectAction> actions;

            try{
                // Try just with the local repository
                actions = await manager.PreviewInstallPackageAsync(this, identity, resolutionContext, this, repositories.Take(1), Array.Empty<SourceRepository>(), cancellationToken);
            }catch(InvalidOperationException e)
            {
                Log(LogLevel.Information, e.Message);
                actions = await manager.PreviewInstallPackageAsync(this, identity, resolutionContext, this, repositories, Array.Empty<SourceRepository>(), cancellationToken);
            }

            actions = actions.Where(action => {
                if(action.NuGetProjectActionType != NuGetProjectActionType.Install)
                {
                    return true;
                }
                var identity = action.PackageIdentity;
                var id = identity.Id;
                if(runtimeLibraries.TryGetValue(id, out var existingVersion))
                {
                    const string sfiId = nameof(IS4) + "." + nameof(SFI);
                    if(
                        id.Equals(sfiId, StringComparison.OrdinalIgnoreCase) ||
                        id.StartsWith(sfiId + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    var versionRange = action.VersionRange;
                    if(versionRange != null && versionRange.Satisfies(existingVersion))
                    {
                        return false;
                    }
                    if(identity.HasVersion && identity.Version > existingVersion)
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            });

            currentPlugin = new(idName);
            await manager.ExecuteNuGetProjectActionsAsync(this, actions, this, sourceCacheContext, cancellationToken);
            return currentPlugin.Plugin;
        }

        /// <summary>
        /// Adds a package to <see cref="InstalledPackages"/>.
        /// </summary>
        /// <param name="package">The package to add.</param>
        /// <exception cref="ArgumentException">A package with the same identity is already present.</exception>
        public void AddPackage(PackageReference package)
        {
            var identity = package.PackageIdentity;
            if(!installedPackages.TryAdd(identity, package))
            {
                throw new ArgumentException($"Package {identity} is already present.", nameof(package));
            }
        }

        /// <summary>
        /// Retrieves a package by its identity from <see cref="InstalledPackages"/>.
        /// </summary>
        /// <param name="identity">The identity of the package.</param>
        /// <returns>The package with <paramref name="identity"/>, or <see langword="null"/>.</returns>
        public PackageReference? GetPackage(PackageIdentity identity)
        {
            return installedPackages.TryGetValue(identity, out var package) ? package : null;
        }

        /// <inheritdoc/>
        public async override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            return installedPackages.Values;
        }

        /// <inheritdoc/>
        public async override Task<bool> InstallPackageAsync(PackageIdentity identity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            if(downloadResourceResult.Status is DownloadResourceResultStatus.Available or DownloadResourceResultStatus.AvailableWithoutStream)
            {
                var package = downloadResourceResult.PackageReader;
                identity = await package.GetIdentityAsync(token);
                var libGroups = await package.GetLibItemsAsync(token);
                var libs = libGroups.GetNearest(TargetFramework);
                var plugin = currentPlugin;
                AddPackage(new(identity, libs?.TargetFramework ?? TargetFramework));
                if(runtimeLibraries.TryGetValue(identity.Id, out var runtimeVersion) && runtimeVersion >= identity.Version)
                {
                    return true;
                }
                if(plugin != null && libs != null)
                {
                    foreach(var file in libs.Items)
                    {
                        plugin.AddFile(new(file, package));
                    }
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async override Task<bool> UninstallPackageAsync(PackageIdentity identity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            return installedPackages.TryRemove(identity, out _);
        }

        #region INuGetProjectContext implementation
        void Log(LogLevel logLevel, string message, object[]? args = null)
        {
            if(String.IsNullOrWhiteSpace(message)) return;
            logger?.Log(logLevel, message, args ?? Array.Empty<object>());
        }

        void INuGetProjectContext.Log(MessageLevel level, string message, params object[] args)
        {
            Log(GetLogLevel(level), message, args);
        }

        void INuGetProjectContext.Log(ILogMessage message)
        {
            Log(GetLogLevel(message.Level), message.Message);
        }

        void INuGetProjectContext.ReportError(string message)
        {
            Log(LogLevel.Error, message);
        }

        void INuGetProjectContext.ReportError(ILogMessage message)
        {
            Log(GetLogLevel(message.Level), message.Message);
        }

        static LogLevel GetLogLevel(MessageLevel level)
        {
            switch(level)
            {
                case MessageLevel.Error:
                    return LogLevel.Error;
                case MessageLevel.Info:
                    return LogLevel.Information;
                case MessageLevel.Warning:
                    return LogLevel.Warning;
                case MessageLevel.Debug:
                default:
                    return LogLevel.Debug;
            }
        }

        static LogLevel GetLogLevel(NuGetLogLevel level)
        {
            switch(level)
            {
                case NuGetLogLevel.Error:
                    return LogLevel.Error;
                case NuGetLogLevel.Information:
                case NuGetLogLevel.Minimal:
                    return LogLevel.Information;
                case NuGetLogLevel.Verbose:
                    return LogLevel.Trace;
                case NuGetLogLevel.Warning:
                    return LogLevel.Warning;
                case NuGetLogLevel.Debug:
                default:
                    return LogLevel.Debug;
            }
        }

        FileConflictAction INuGetProjectContext.ResolveFileConflict(string message)
        {
            throw new NotImplementedException();
        }

        PackageExtractionContext? INuGetProjectContext.PackageExtractionContext { get; set; }

        ISourceControlManagerProvider INuGetProjectContext.SourceControlManagerProvider => throw new NotImplementedException();

        ExecutionContext INuGetProjectContext.ExecutionContext { get; } = new Context();

        class Context : ExecutionContext
        {
            public async override Task OpenFile(string fullPath)
            {
                throw new NotSupportedException();
            }
        }

        XDocument? INuGetProjectContext.OriginalPackagesConfig { get; set; }
        NuGetActionType INuGetProjectContext.ActionType { get; set; }
        Guid INuGetProjectContext.OperationId { get; set; }
        #endregion

        #region ISettings implementation
        event EventHandler ISettings.SettingsChanged {
            add {
                if(settings != null)
                {
                    settings.SettingsChanged += value;
                }
            }
            remove {
                if(settings != null)
                {
                    settings.SettingsChanged -= value;
                }
            }
        }

        SettingSection? ISettings.GetSection(string sectionName)
        {
            return settings?.GetSection(sectionName);
        }

        void ISettings.AddOrUpdate(string sectionName, SettingItem item)
        {
            settings?.AddOrUpdate(sectionName, item);
        }

        void ISettings.Remove(string sectionName, SettingItem item)
        {
            settings?.Remove(sectionName, item);
        }

        void ISettings.SaveToDisk()
        {
            settings?.SaveToDisk();
        }

        IList<string> ISettings.GetConfigFilePaths()
        {
            return settings?.GetConfigFilePaths() ?? Array.Empty<string>();
        }

        IList<string> ISettings.GetConfigRoots()
        {
            return settings?.GetConfigRoots() ?? Array.Empty<string>();
        }
        #endregion

    }
}
