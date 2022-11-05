// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Actor;
using Anamnesis.Serialization;
using Serilog;
using XivToolsWpf;
using System.Diagnostics;
using Anamnesis.Navigation;
using System.Collections.Generic;
using Anamnesis.Panels;
using Anamnesis.Libraries;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class ServiceManager
{
	private const int ServiceCount = 27;
	private readonly List<IService> services = new();
	private double initializedServiceSount = 0;
	private double startedServiceCount = 0;

	public double BootProgress { get; private set; } = 0;
	public bool BootComplete { get; private set; } = false;

	public LogService Logs { get; } = new();
	public SettingsService Settings { get; } = new();
	public NavigationService Navigation { get; } = new();
	public SerializerService Serializer { get; } = new();
	public LocalizationService Localization { get; } = new();
	public Updater.UpdateService Update { get; } = new();
	public MemoryService Memory { get; } = new();
	public AddressService Address { get; } = new();
	public ActorService Actor { get; } = new();
	public TargetService Target { get; } = new();
	public FileService FileService { get; } = new();
	public TerritoryService Territory { get; } = new();
	public GameService Game { get; } = new();
	public TimeService Time { get; } = new();
	public CameraService Camera { get; } = new();
	public GposeService Gpose { get; } = new();
	public GameDataService GameData { get; } = new();
	public PoseService Pose { get; } = new();
	public TipService Tips { get; } = new();
	public FavoritesService Favorites { get; } = new();
	public AnimationService Animation { get; } = new();
	public Keyboard.KeyboardService Hotkeys { get; } = new();
	public HistoryService History { get; } = new();
	public SceneService Scene { get; } = new();
	public PanelService Panels { get; } = new();
	public LibraryService Library { get; } = new();
	public DalamudIpcService DalamudIpc { get; } = new();

#if DEBUG
	public bool IsDebug => true;
#else
	public bool IsDebug => false;
#endif

	public async Task InitializeCriticalServices()
	{
		await this.InitializeService(this.Logs);
		await this.InitializeService(this.Settings);
		await this.InitializeService(this.Localization);
		await this.InitializeService(this.Memory);
	}

	public async Task InitializeServices()
	{
		await this.InitializeService(this.DalamudIpc);
		await this.InitializeService(this.Panels);
		await this.InitializeService(this.Scene);
		await this.InitializeService(this.Navigation);
		await this.InitializeService(this.Serializer);
		await this.InitializeService(this.Update);
		await this.InitializeService(this.Address);
		await this.InitializeService(this.Actor);
		await this.InitializeService(this.Target);
		await this.InitializeService(this.FileService);
		await this.InitializeService(this.Territory);
		await this.InitializeService(this.Game);
		await this.InitializeService(this.Time);
		await this.InitializeService(this.Camera);
		await this.InitializeService(this.Gpose);
		await this.InitializeService(this.GameData);
		await this.InitializeService(this.Pose);
		await this.InitializeService(this.Tips);
		await this.InitializeService(this.Favorites);
		await this.InitializeService(this.Animation);
		await this.InitializeService(this.Hotkeys);
		await this.InitializeService(this.History);
		await this.InitializeService(this.Library);

		await this.StartServices();

		this.BootComplete = true;
	}

	public async Task StartServices()
	{
		await App.Current.Dispatcher.MainThread();

		foreach (IService service in this.services)
		{
			Stopwatch sw = new();
			sw.Start();
			await service.Start();
			this.startedServiceCount++;
			this.UpdateBootProgress();
			Log.Information($"Started service: {service.GetType().Name} in {sw.ElapsedMilliseconds}ms");
		}

		if (this.initializedServiceSount != ServiceCount ||
			this.startedServiceCount != ServiceCount ||
			this.services.Count != ServiceCount)
		{
			throw new Exception("Service Count is incorrect. (Was a new service added without increasing the service count?)");
		}
	}

	public async Task ShutdownServices()
	{
		// shutdown services in reverse order
		this.services.Reverse();

		foreach (IService service in this.services)
		{
			try
			{
				// If this throws an exception we should keep trying to shut down the rest
				// not doing so can leave the game memory in a corrupt state
				await service.Shutdown();
				this.initializedServiceSount--;
				this.startedServiceCount--;
				this.UpdateBootProgress();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to shutdown service.");
			}
		}
	}

	private async Task InitializeService(IService service)
	{
		Stopwatch sw = new();
		sw.Start();
		await Dispatch.NonUiThread();
		await service.Initialize();
		this.services.Add(service);
		Log.Information($"Initialized service: {service.GetType().Name} in {sw.ElapsedMilliseconds}ms");

		this.initializedServiceSount++;
		this.UpdateBootProgress();
	}

	private void UpdateBootProgress()
	{
		this.BootProgress = ((this.initializedServiceSount / ServiceCount) / 2.0) + ((this.startedServiceCount / ServiceCount) / 2.0);
	}
}
