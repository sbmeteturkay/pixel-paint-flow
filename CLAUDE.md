Assets/
├── _Project/
│   ├── Features/                        # Oyun mekaniği — her özellik kendi klasöründe
│   │   └── Player/                      # Örnek feature
│   │       ├── Scripts/
│   │       ├── Prefabs/
│   │       ├── Data/                    # ScriptableObject asset'leri
│   │       ├── Animations/
│   │       └── Tests/
│   │
│   ├── Core/                            # Oyunun iskeleti — oyuna özel hiçbir şey içermez
│   │   ├── Installers/
│   │   │   ├── GameLifetimeScope.cs     # VContainer root scope
│   │   │   └── AppLifetimeScope.cs      # Oyuna özel bind'lar (template'den çekilmez)
│   │   ├── EventSystem/
│   │   │   └── Events/
│   │   │       ├── PlayerEvents.cs      # MessagePipe event tipleri (struct)
│   │   │       ├── GameStateEvents.cs
│   │   │       └── ...
│   │   ├── GameManager/
│   │   ├── AudioService/
│   │   ├── SaveSystem/
│   │   └── SceneManagement/
│   │
│   ├── Shared/                          # 2+ feature'ın kullandığı ortak kod
│   │   ├── Scripts/
│   │   │   ├── Extensions/
│   │   │   ├── Utilities/
│   │   │   │   ├── ObjectPool.cs
│   │   │   │   └── Timer.cs
│   │   │   └── Interfaces/
│   │   │       ├── IDamageable.cs
│   │   │       ├── ICollectible.cs
│   │   │       └── IPoolable.cs
│   │   └── Data/
│   │
│   └── Settings/                        # Unity config asset'leri (kod değil)
│       ├── InputSystem.inputactions
│       ├── UniversalRenderPipeline.asset
│       └── AudioMixer.mixer
│
├── Art/
├── Audio/
├── Scenes/
│   ├── Boot.unity
│   ├── MainMenu.unity
│   ├── Levels/
│   └── _Dev/
└── Plugins/