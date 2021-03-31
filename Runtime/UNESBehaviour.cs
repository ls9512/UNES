using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Aya.UNES.Controller;
using Aya.UNES.Input;
using Aya.UNES.Renderer;

namespace Aya.UNES
{
    public class UNESBehaviour : MonoBehaviour
    {
        [Header("Render")]
        public RenderTexture RenderTexture;
        public const int GameWidth = 256;
        public const int GameHeight = 240;
        public FilterMode FilterMode = FilterMode.Point;
        public bool LogicThread = true;

        [Header("Input")] 
        public KeyConfig KeyConfig;

        public BaseInput Input { get; set; }
        public IRenderer Renderer { get; set; }
        public uint[] RawBitmap { get; set; } = new uint[GameWidth * GameHeight];
        public bool Ready { get; set; }
        public bool GameStarted { get; set; }

        private bool _rendererRunning = true;
        private IController _controller;
        private Emulator _emu;
        private bool _suspended;
        private Thread _renderThread;
        private int _activeSpeed = 1;

        public void Boot(byte[] romData)
        {
            InitInput();
            InitRenderer();
            BootCartridge(romData);
        }

        public void LoadSaveData(byte[] saveData)
        {
            _emu?.Mapper.LoadSaveData(saveData);
        }

        public byte[] GetSaveData()
        {
            return _emu?.Mapper.GetSaveData();
        }

        private void BootCartridge(byte[] romData)
        {
            _emu = new Emulator(romData, _controller);
            if (LogicThread)
            {
                _renderThread = new Thread(() =>
                {
                    GameStarted = true;
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
        }

        public void OnEnable()
        {
            
        }

        public void OnDisable()
        {
            _rendererRunning = false;
            Renderer?.End();
        }

        public void Update()
        {
            if (!GameStarted) return;
            UpdateInput();
            UpdateRender();
        }

        #endregion

        #region Input

        private void InitInput()
        {
            _controller = new NesController(this);
            Input = new DefaultInput();
        }

        public void UpdateInput()
        {
            Input.HandlerKeyDown(keyCode =>
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

            Input.HandlerKeyUp(keyCode =>
            {
                _controller.ReleaseKey(keyCode);
            });
        }

        #endregion

        #region Render

        public void UpdateRender()
        {
            if (!_rendererRunning) return;
            if (_suspended) return;

            if (!LogicThread)
            {
                _emu.PPU.ProcessFrame();
                RawBitmap = _emu.PPU.RawBitmap;
                Renderer.HandleRender();
            }
            else
            {
                lock (RawBitmap)
                {
                    Renderer.HandleRender();
                }
            }
        }

        private void InitRenderer()
        {
            Renderer?.End();

            Renderer = new UnityRenderer();
            Renderer.Init(this);
        } 

        #endregion
    }
}
