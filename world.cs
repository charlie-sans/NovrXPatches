using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using CodeX;
using FrooxEngine.LogiX;
using FrooxEngine.Undo;
using NetX;
using FrooxEngine;
namespace NovrX
{

    public class World 
    {
        public enum WorldFocus
        {
            Background,
            Focused,
            Overlay,
            publicOverlay
        }

        public enum WorldState
        {
            Initializing,
            Running,
            Failed
        }

        public enum InitializationState
        {
            Created,
            InitializingNetwork,
            WaitingForJoinGrant,
            InitializingDataModel,
            Finished,
            Failed
        }

        public enum FailReason
        {
            None,
            NetworkError,
            JoinRejected,
            AuthenticationError
        }

        public enum WorldEvent
        {
            OnFocusChanged,
            OnUserJoined,
            OnUserSpawn,
            OnUserLeft,
            OnWorldSaved,
            OnWorldDestroy,
            END
        }


        public struct SynchronousAction
        {
            public readonly IUpdatable updatable;

            public readonly Action action;

            public readonly bool evenDisposed;

            public SynchronousAction(Action action, IUpdatable updatable, bool evenDisposed)
            {
                this.action = action;
                this.updatable = updatable;
                this.evenDisposed = evenDisposed;
            }
        }

        public enum RefreshStage
        {
            RefreshBegin,
            UpdatingStreams,
            PhysicsSync,
            PhysicsMoved,
            PhysicsUpdate,
            RunningStartups,
            WorldEvents,
            RunningEvents,
            Input,
            Coroutines,
            Updates,
            LogixUpdates,
            Changes,
            Destructions,
            PhysicsSchleduleRefine,
            MovedSlots,
            Connectors,
            ValidatingPermissions,
            Finished,
            SynchronousActions
        }

        public const string FIXED_SALT = "599f0e72-c606-483b-8ddd-44e8b5a27515";

        public const int UPDATE_TIMES_HISTORY = 90;

        public object stateLock = new object();

        public static Type __connectorType;

        public readonly WorldManager WorldManager;

        internal readonly WorldConfiguration Configuration;

        public ushort[][] _randomizationTables;

        public bool ForceFullUpdateCycle;

        public readonly bool UnsafeMode;

        public bool ForceAnnounceOnWAN;

        public bool SaveOnExit;

        public string AllowUserCloudVariable;

        public string DenyUserCloudVariable;

        public string RequiredUserJoinCloudVariable;

        public string RequiredUserJoinCloudVariableDenyMessage;

        public HashSet<string> _allowedUsers = new HashSet<string>();

        public SpinQueue<SynchronousAction> synchronousActions = new SpinQueue<SynchronousAction>();

        public WorldFocus _focus;

        public List<Uri> _sourceURLs;

        public World _parent;

        public FrooxEngine.Record _record;

        public bool BlockAutoRespawn;

        public string _lastName;

        public string _lastUnstrippedName;

        public string _lastStrippedName;

        public SlotBag _slots;

        public UserBag _users;

        public SyncRefDictionary<string, Component> _keys;

        public SyncFieldDictionary<string, int> _keyVersions;

        public List<Slot> _localSlots = new List<Slot>();

        public WorldAction worldInitAction;

        public DataTreeNode worldInitLoad;

        public ReferenceTranslator refTranslator;

        public Dictionary<Type, HashSet<Worker>> _globallyRegisteredComponents = new Dictionary<Type, HashSet<Worker>>();

        public int _graceFullUpdateCycles;

        public int _stageUpdateTimeCount;

        public double[] _minStageUpdateTimeOngoing = new double[Enum.GetValues(typeof(RefreshStage)).Length];

        public double[] _maxStageUpdateTimeOngoing = new double[Enum.GetValues(typeof(RefreshStage)).Length];

        public double[] _avgStageUpdateTimeOngoing = new double[Enum.GetValues(typeof(RefreshStage)).Length];

        public int _lastAudioStreamUnderruns;

        public double sumUpdateTime;

        public int requestedAssets;

        public int loadedAssets;

        public volatile int audioConfigurationChanged;

        public Stopwatch stopwatch = new Stopwatch();

        public Stopwatch stageStopwatch = new Stopwatch();

        public Stopwatch audioStopwatch = new Stopwatch();

        internal bool debugLogUpdateTimes;

        public static string[] updateStageNames = Enum.GetNames(typeof(RefreshStage));

        public DateTime _lastStatsUpdate;

        public Dictionary<string, Component> _locallyRegisteredComponents = new Dictionary<string, Component>();

        public Dictionary<ulong, Dictionary<RefID, ISyncMember>> trashbin;

        public Dictionary<Type, Action<Slot, Component>> _componentAddedEvents = new Dictionary<Type, Action<Slot, Component>>();

        public Dictionary<Type, Action<Slot, Component>> _componentRemovedEvents = new Dictionary<Type, Action<Slot, Component>>();

        public List<IWorldEventReceiver>[] worldEventReceivers;

        public List<FrooxEngine.User> joinedUsers = new List<FrooxEngine.User>();

        public HashSet<FrooxEngine.User> spawnUsers = new HashSet<FrooxEngine.User>();

        public List<FrooxEngine.User> leftUsers = new List<FrooxEngine.User>();

        public bool worldSaved;

        public WorldFocus _lastFocus;

        public static int worldEventTypeCount = Enum.GetValues(typeof(WorldEvent)).Length;

        public Action DisconnectRequestedHook;

        public Action HostConnectionClosedHook;

        public Dictionary<RefID, FrooxEngine.User> _userSnapshot = new Dictionary<RefID, FrooxEngine.User>();

        public FrooxEngine.User _hostUser;

        public Slot _assets;

        public Slot _localAssets;

        public volatile bool _disposed;

        public IWorldConnector Connector { get;  set; }

        public Engine Engine => WorldManager.Engine;

        internal int RandomizationSeed { get; set; }

        internal byte[] Obfuscation_KEY { get; set; }

        internal byte[] Obfuscation_IV { get;  set; }

        
        
    }
}
