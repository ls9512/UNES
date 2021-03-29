using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using Aya.UNes.Controller;
using Aya.UNes.Renderer;
using Debug = UnityEngine.Debug;

namespace Aya.UNes
{
    public class UNes : MonoBehaviour
    {
        public string RomFile;
        public RenderTexture RenderTexture;

        public const int GameWidth = 256;
        public const int GameHeight = 240;
        public FilterMode FilterMode = FilterMode.Point;
        public bool RenderThread = true;


        private bool _rendererRunning = true;
        private readonly IController _controller = new NesController();

        private Emulator _emu;
        private bool _suspended;

        private readonly Type[] _possibleRendererList = { typeof(UnityRenderer) };
        private readonly List<IRenderer> _availableRendererList = new List<IRenderer>();

        public IRenderer Renderer { get; set; }
        public uint[] RawBitmap { get; set; } = new uint[GameWidth * GameHeight];
        public bool Ready { get; set; }
        public bool GameStarted { get; set; }

        private Thread _renderThread;
        private int _activeSpeed = 1;

        private void BootCartridge(string rom)
        {
            var bytes = Resources.Load<TextAsset>(RomFile).bytes;
            _emu = new Emulator(bytes, _controller);

            if (RenderThread)
            {
                _renderThread = new Thread(() =>
                {
                    GameStarted = true;
                    Console.WriteLine(_emu.Cartridge);
                    var s = new Stopwatch();
                    var s0 = new Stopwatch();
                    while (_rendererRunning)
                    {
                        if (_suspended)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        s.Restart();
                        for (var i = 0; i < 60 && !_suspended; i++)
                        {
                            s0.Restart();
                            lock (RawBitmap)
                            {
                                _emu.PPU.ProcessFrame();
                                RawBitmap = _emu.PPU.RawBitmap;
                            }

                            s0.Stop();
                            Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0) / _activeSpeed);
                        }

                        s.Stop();
                        Console.WriteLine($"60 frames in {s.ElapsedMilliseconds}ms");
                    }
                });

                _renderThread.Start();
            }
            else
            {
                GameStarted = true;
            }
        }

        #region Monobehaviour

        public void Awake()
        {
            FindRenderer();
            SetRenderer(_availableRendererList.Last());
        }

        public void OnEnable()
        {
            BootCartridge(RomFile);
        }

        public void OnDisable()
        {
            _rendererRunning = false;
            // _emu?.Save();
        }

        public void Update()
        {
            if (!GameStarted) return;
            UpdateInput();
            UpdateRender();
        }

        #endregion

        #region Input

        public void UpdateInput()
        {
            HandlerKeyDown(keyCode =>
            {
                switch (keyCode)
                {
                    case KeyCode.F2:
                        _suspended = false;
                        break;
                    case KeyCode.F3:
                        _suspended = true;
                        break;
                    default:
                        _controller.PressKey(keyCode);
                        break;
                }
            });

            HandlerKeyUp(keyCode =>
            {
                _controller.ReleaseKey(keyCode);
            });
        }

        private KeyCode[] _keyCodes
        {
            get
            {
                if (_keyCodeCache == null)
                {
                    var array = Enum.GetValues(typeof(KeyCode));
                    _keyCodeCache = new KeyCode[array.Length];
                    for (var i = 0; i < array.Length; i++)
                    {
                        _keyCodeCache[i] = (KeyCode) array.GetValue(i);
                    }
                }

                return _keyCodeCache;
            }
        }
        private KeyCode[] _keyCodeCache;

        public void HandlerKeyDown(Action<KeyCode> onKeyDown)
        {
            foreach (var keyCode in _keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    onKeyDown(keyCode);
                }
            }
        }

        public void HandlerKeyUp(Action<KeyCode> onKeyUp)
        {
            foreach (var keyCode in _keyCodes)
            {
                if (Input.GetKeyUp(keyCode))
                {
                    onKeyUp(keyCode);
                }
            }
        }

        #endregion

        #region Render

        public void UpdateRender()
        {
            if (!RenderThread)
            {
                if (!_rendererRunning) return;
                if (_suspended) return;

                _emu.PPU.ProcessFrame();
                RawBitmap = _emu.PPU.RawBitmap;
                Renderer.Draw();
            }
            else
            {
                if (!_rendererRunning) return;
                if (_suspended) return;

                lock (RawBitmap)
                {
                    Renderer.Draw();
                }
            }
        }

        private void SetRenderer(IRenderer render)
        {
            if (Renderer == render) return;
            Renderer?.EndRendering();
            Renderer = render;
            render.InitRendering(this);
        }

        private void FindRenderer()
        {
            foreach (var renderType in _possibleRendererList)
            {
                try
                {
                    var renderer = (IRenderer)Activator.CreateInstance(renderType);
                    renderer.InitRendering(this);
                    renderer.EndRendering();
                    _availableRendererList.Add(renderer);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{renderType} failed to initialize");
                }
            }
        } 

        #endregion

        public void Open(string file)
        {
            try
            {
                BootCartridge(file);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading ROM file; either corrupt or unsupported");
            }
        }
    }
}
