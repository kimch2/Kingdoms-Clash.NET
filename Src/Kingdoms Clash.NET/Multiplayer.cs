﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClashEngine.NET;
using ClashEngine.NET.Interfaces.Net;
using ClashEngine.NET.Net;

namespace Kingdoms_Clash.NET
{
	using Interfaces;
	using Interfaces.Controllers;
	using Interfaces.Map;
	using Interfaces.Player;
	using Interfaces.Units;

	/// <summary>
	/// Stan-ekran gry wieloosobowej.
	/// </summary>
	public class Multiplayer
		: Screen, IMultiplayer, IGameStateScreen
	{
		private static NLog.Logger Logger = NLog.LogManager.GetLogger("KingdomsClash.NET");

		#region Private
		private uint UserId;
		private IClient Client;
		private IMultiplayerSettings MainSettings;
		private List<IPlayerData> PlayersInLobby;
		private List<string> AvailableNations;
		private bool InGame = false;
		private PlayerType PlayerType = PlayerType.Spectator;
		private List<IResourceOnMap> Resources = new List<IResourceOnMap>();

		private Dictionary<GameMessageType, Func<Message, bool>> InGameHandlers = new Dictionary<GameMessageType, Func<Message, bool>>();
		private Dictionary<GameMessageType, Func<Message, bool>> NonInGameHandlers = new Dictionary<GameMessageType, Func<Message, bool>>();
		private Dictionary<GameMessageType, Func<Message, bool>> OthersHandlers = new Dictionary<GameMessageType, Func<Message, bool>>();

		private UserData.ClientLoader UserData;
		#endregion

		#region IGameState Members
		#region Properties
		/// <summary>
		/// Ustawienia gry.
		/// </summary>
		public IGameplaySettings Settings { get; private set; }

		/// <summary>
		/// Tablica dwóch, aktualnie grających, graczy.
		/// </summary>
		public IPlayer[] Players { get; private set; }

		/// <summary>
		/// Mapa.
		/// </summary>
		public IMap Map { get; private set; }

		/// <summary>
		/// Kontroler(tryb) gry.
		/// </summary>
		public IGameController Controller { get; private set; }
		#endregion

		#region Methods
		//Nie używamy - kontroler tylko steruje "na bierząco" grą, ale to serwer o wszytkim decyduje
		//TODO: zastanowić się nad sensem używania kontrolera gry i reguł zwycięstwa w kliencie
		/// <summary>
		/// Resetuje stan gry(zaczyna od początku).
		/// </summary>
		public void Reset()
		{
		}

		/// <summary>
		/// Dodaje jednostkę do gry.
		/// </summary>
		/// <param name="unit">Jednostka.</param>
		public void Add(IUnit unit)
		{
		}

		/// <summary>
		/// Dodaje zasób do gry.
		/// </summary>
		/// <param name="resource">Zasób.</param>
		public void Add(IResourceOnMap resource)
		{
		}

		/// <summary>
		/// Usuwa jednostkę z gry.
		/// Musimy zapewnić poprawny przebieg encji, więc dodajemy do kolejki oczekujących na usunięcie.
		/// </summary>
		/// <param name="unit">Jednostka.</param>
		public void Kill(IUnit unit)
		{
		}

		/// <summary>
		/// Usuwa zasób z gry.
		/// Musimy zapewnić poprawny przebieg encji, więc dodajemy do kolejki oczekujących na usunięcie.
		/// </summary>
		/// <param name="resource">Zasób.</param>
		public void Gather(IResourceOnMap resource, IUnit by)
		{
		}
		#endregion
		#endregion

		#region IMultiplayer Members
		/// <summary>
		/// Inicjalizuje stan gry.
		/// </summary>
		/// <param name="settings">Ustawienia gry.</param>
		public void Initialize(IMultiplayerSettings settings)
		{
			this.MainSettings = settings;
		}
		#endregion

		#region Screen Members
		public override void OnInit()
		{
			this.Client = new TcpClient(new System.Net.IPEndPoint(this.MainSettings.Address, this.MainSettings.Port),
				System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

			this.Client.Open();
			if (this.Client.Status != ClientStatus.Ok)
			{
				throw new Exception("Cannot connect to server - " + this.Client.Status.ToString()); //TODO: ładne przedstawienie błędu
			}

			List<KeyValuePair<string, byte[]>> nations = (from nation in this.UserData.Nations let nationName = nation.Name
														 from checkSum in this.UserData.NationsCheckSums let cs = checkSum
														 select new KeyValuePair<string, byte[]>(nationName, cs)).ToList();

			Messages.PlayersFirstConfiguration firstMsg = new Messages.PlayersFirstConfiguration(this.MainSettings.PlayerNick, nations);
			this.Client.Send(firstMsg.ToMessage());

			Stopwatch wait = new Stopwatch();
			wait.Start();
			while (wait.Elapsed.TotalMinutes < 1 && !this.Client.Messages.Contains((MessageType)GameMessageType.PlayerAccepted)) ;
			wait.Stop();
			if (this.Client.Messages.Contains((MessageType)GameMessageType.PlayerAccepted))
			{
				var msg = new Messages.PlayerAccepted(this.Client.Messages.GetFirst((MessageType)GameMessageType.PlayerAccepted));
				this.UserId = msg.UserId;
				this.PlayersInLobby = msg.Players;
				this.AvailableNations = msg.AvailableNations;
				//TODO: tmp
				this.Client.Send(new Messages.PlayerChangedNick(this.UserId, this.MainSettings.PlayerNick + this.UserId).ToMessage());
				this.Client.Send(new Messages.PlayerChangedState(this.UserId).ToMessage());
			}
			else
			{
				throw new Exception("Cannot connect to server - " + (this.Client.Status == ClientStatus.Ok ? "not responding" : this.Client.Status.ToString())); //TODO: ładne przedstawienie błędu
			}
			Logger.Info("Players in lobby: {0}", this.PlayersInLobby.Count);
			foreach (var p in this.PlayersInLobby)
			{
				Logger.Info("\t{0}", p.Nick);
			}
		}

		public override void OnDeinit()
		{
			this.Client.Close();
		}

		public override void Update(double delta)
		{
			if (this.InGame)
				this.ProcessInGame();
			else
				this.ProcessNonInGame();
			this.ProcessOther();
			base.Update(delta);
		}
		#endregion

		#region Constructors
		internal Multiplayer(UserData.ClientLoader userData)
			: base("GameScreen", ClashEngine.NET.Interfaces.ScreenType.Fullscreen)
		{
			this.InGameHandlers.Add(GameMessageType.UnitQueued, this.UnitQueuedHandler);
			this.InGameHandlers.Add(GameMessageType.UnitCreated, this.UnitCreatedHandler);
			this.InGameHandlers.Add(GameMessageType.UnitDestroyed, this.UnitDestroyedHandler);
			this.InGameHandlers.Add(GameMessageType.ResourceAdded, this.ResourceAddedHandler);
			this.InGameHandlers.Add(GameMessageType.ResourceGathered, this.ResourceGatheredHandler);
			this.InGameHandlers.Add(GameMessageType.PlayerHurt, this.PlayerHurtHandler);
			this.InGameHandlers.Add(GameMessageType.GameEnded, this.GameEndedHandler);

			this.NonInGameHandlers.Add(GameMessageType.GameWillStartAfter, this.GameWillStartAfterHandler);
			this.NonInGameHandlers.Add(GameMessageType.GameStarted, this.GameStartedHandler);

			this.OthersHandlers.Add(GameMessageType.PlayerConnected, this.PlayerConnectedHandler);
			this.OthersHandlers.Add(GameMessageType.PlayerDisconnected, this.PlayerDisconnectedHandler);
			this.OthersHandlers.Add(GameMessageType.PlayerChangedNick, this.PlayerChangedNickHandler);
			this.OthersHandlers.Add(GameMessageType.PlayerChangedState, this.PlayerChangedStateHandler);

			this.UserData = userData;
		}
		#endregion

		#region Handling
		/// <summary>
		/// Obsługa w trakcie gry.
		/// </summary>
		private void ProcessInGame()
		{
			foreach (var msg in this.Client.Messages.GetUpgradeableEnumerable())
			{
				if (this.InGameHandlers.Call(msg))
					this.Client.Messages.RemoveAt(0);
				break;
			}
		}

		/// <summary>
		/// Obsługa poza grą.
		/// </summary>
		private void ProcessNonInGame()
		{
			foreach (var msg in this.Client.Messages.GetUpgradeableEnumerable())
			{
				if (this.NonInGameHandlers.Call(msg))
					this.Client.Messages.RemoveAt(0);
				break;
			}
		}

		/// <summary>
		/// Obsługa pozostałych.
		/// </summary>
		private void ProcessOther()
		{
			foreach (var msg in this.Client.Messages.GetUpgradeableEnumerable())
			{
				this.OthersHandlers.Call(msg);
				this.Client.Messages.RemoveAt(0);
				break;
			}
		}
		#endregion

		#region InGame Handlers
		private bool UnitQueuedHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			var unitQueued = new Messages.UnitQueued(msg);
			if (unitQueued.Accepted)
			{
				IUnitRequestToken token = null;
				if (this.PlayerType == PlayerType.First)
					token = this.Controller.Player1Queue.Request(unitQueued.UnitId);
				else
					token = this.Controller.Player2Queue.Request(unitQueued.UnitId);

				if (token == null)
					Logger.Warn("Something went wrong - player resources are desynchronized");
				else
					Logger.Trace("Unit {0} queued", unitQueued.UnitId);
			}
			return true;
		}

		private bool UnitCreatedHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			var unitCreated = new Messages.UnitCreated(msg);
			var player = this.Players[unitCreated.PlayerId];
			var unit = new Units.Unit(player.Nation.AvailableUnits[unitCreated.UnitId], player);
			unit.UnitId = unitCreated.NumericUnitId;
			unit.Position = unitCreated.Position;

			player.Units.Add(unit);
			this.Entities.Add(unit);

			Logger.Trace("Unit {0} created(player: {1})", unit.Id, player.Name);

			//TODO: zdarzenia?
			return true;
		}

		private bool UnitDestroyedHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			var unitDestroyed = new Messages.UnitDestroyed(msg);
			var player = this.Players[unitDestroyed.PlayerId];
			var unit = (from u in player.Units where u.UnitId == unitDestroyed.UnitId select u).Single();

			this.Entities.Remove(unit);
			player.Units.Remove(unit);

			Logger.Trace("Unit {0} destroyed(player: {1})", unit.Id, player.Name);

			return true;
		}

		private bool ResourceAddedHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			var resourceAdded = new Messages.ResourceAdded(msg);
			var res = new Maps.ResourceOnMap(resourceAdded.ResourceId, resourceAdded.Amount, resourceAdded.Position);
			res.GameState = this;
			res.ResourceId = resourceAdded.NumericResourceId;

			this.Entities.Add(res);
			this.Resources.Add(res);

			return true;
		}

		private bool ResourceGatheredHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			var resourceGathered = new Messages.ResourceGathered(msg);
			var player = this.Players[resourceGathered.PlayerId];
			var unit = (from u in player.Units where u.UnitId == resourceGathered.UnitId select u).Single();
			var res = this.Resources.Find(r => r.ResourceId == resourceGathered.ResourceId);
			player.Resources[res.Id] += res.Value;

			this.Resources.Remove(res);
			this.Entities.Remove(res);

			return true;
		}

		private bool PlayerHurtHandler(Message msg)
		{
			if (!this.InGame)
				return false;
			var playerHurt = new Messages.PlayerHurt(msg);
			this.Players[playerHurt.PlayerId].Health -= playerHurt.Value;
			return true;
		}

		private bool GameEndedHandler(Message msg)
		{
			if (!this.InGame)
				return false;

			this.Entities.Clear();

			return true;
		}
		#endregion

		#region Non InGame Handlers
		private bool GameWillStartAfterHandler(Message msg)
		{
			if (this.InGame)
				return false;
			var gameWillStartAfter = new Messages.GameWillStartAfter(msg);
			Logger.Info("Game will start after {0}", gameWillStartAfter.Time.ToString());
			return true;
		}

		private bool GameStartedHandler(Message msg)
		{
			if (this.InGame)
				return false;

			Messages.GameStarted gameStarted = new Messages.GameStarted(msg);

			var p1 = this.PlayersInLobby.Find(p => p.UserId == gameStarted.PlayerA);
			var p2 = this.PlayersInLobby.Find(p => p.UserId == gameStarted.PlayerB);
			this.Players = new Player.Player[]
			{
				new Player.Player(p1.Nick, p1.Nation, 100)
				{
					Type = Interfaces.Player.PlayerType.First
				},
				new Player.Player(p2.Nick, p2.Nation, 100)
				{
					Type = Interfaces.Player.PlayerType.Second
				}
			};

			this.Map = new Maps.DefaultMap();

			float h = NET.Settings.ScreenSize * (Configuration.Instance.WindowSize.Height / (float)Configuration.Instance.WindowSize.Width);
			var cam = new ClashEngine.NET.Graphics.Cameras.Movable2DCamera(new OpenTK.Vector2(NET.Settings.ScreenSize, h),
				new System.Drawing.RectangleF(0f, 0f, this.Map.Size.X, Math.Max(this.Map.Size.Y + NET.Settings.MapMargin, h)));
			this.Camera = cam;
			this.Entities.Add(cam.GetCameraEntity(Configuration.Instance.CameraSpeed));
			this.Entities.Add(this.Map);
			this.Entities.Add(new Player.PlayerEntity(this.Players[0], this));
			this.Entities.Add(new Player.PlayerEntity(this.Players[1], this));

			if (this.UserId == gameStarted.PlayerA)
			{
				//TODO: GUI dla gracza A
			}
			else if (this.UserId == gameStarted.PlayerB)
			{
				//TODO: GUI dla gracza B
			}
			else
			{
				//TODO: GUI obserwatora
			}

			return true;
		}
		#endregion

		#region Other Handlers
		private bool PlayerConnectedHandler(Message msg)
		{
			Messages.PlayerConnected newPlayer = new Messages.PlayerConnected(msg);
			this.PlayersInLobby.Add(new Player.PlayerData(newPlayer.UserId) { Nick = newPlayer.Nick });
			Logger.Info("Player {0} connected", newPlayer.Nick);
			return true;
		}

		private bool PlayerDisconnectedHandler(Message msg)
		{
			Messages.PlayerDisconnected disconnected = new Messages.PlayerDisconnected(msg);
			int idx = this.PlayersInLobby.FindIndex(p => p.UserId == disconnected.UserId);
			Logger.Info("Player {0} disconnected, reason: {1}", this.PlayersInLobby[idx].Nick, disconnected.Reason.ToString());
			return true;
		}

		private bool PlayerChangedNickHandler(Message msg)
		{
			Messages.PlayerChangedNick newNick = new Messages.PlayerChangedNick(msg);
			int idx = this.PlayersInLobby.FindIndex(p => p.UserId == newNick.UserId);
			Logger.Info("Player {0} changed the nick to {1}", this.PlayersInLobby[idx].Nick, newNick.NewNick);
			this.PlayersInLobby[idx].Nick = newNick.NewNick;
			return true;
		}

		private bool PlayerChangedStateHandler(Message msg)
		{
			Messages.PlayerChangedState newState = new Messages.PlayerChangedState(msg);
			int idx = this.PlayersInLobby.FindIndex(p => p.UserId == newState.UserId);
			this.PlayersInLobby[idx].ReadyToPlay = !this.PlayersInLobby[idx].ReadyToPlay;
			Logger.Info("Player {0} changed his state to {1}", this.PlayersInLobby[idx].Nick,
				(this.PlayersInLobby[idx].ReadyToPlay ? "ready-to-play" : "spectator"));
			return true;
		}

		private void RequestUnit(string id)
		{
			Messages.UnitQueueAction queue = new Messages.UnitQueueAction(id, true);
			this.Client.Send(queue.ToMessage());
		}
		#endregion
	}
}
