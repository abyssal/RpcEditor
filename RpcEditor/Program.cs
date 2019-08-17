﻿using DiscordRPC;
using System;
using System.Diagnostics;
using Terminal.Gui;

namespace RpcEditor
{
    public class Program
    {
        private DiscordRpcClient _client;

        private readonly Window _window;

        private readonly Label _applicationId;
        private readonly TextField _applicationId_Input;
        private readonly Label _applicationId_Error;
        private bool _appId_Errored = false;

        private readonly Label _state;
        private readonly Label _currentPresence_Name;

        private readonly Button _connect;

        private ulong _appId;

        public Program()
        {
            Application.Init();
            _applicationId = new Label("Discord application ID: ")
            {
                X = 3,
                Y = 2
            };

            _applicationId_Input = new TextField("")
            {
                X = Pos.Right(_applicationId),
                Y = Pos.Top(_applicationId),
                Width = 40,
            };

            _applicationId_Error = new Label("Invalid application ID")
            {
                X = Pos.Right(_applicationId_Input),
                Y = Pos.Top(_applicationId_Input),
                Width = 25,
                TextColor = Terminal.Gui.Attribute.Make(Color.Red, Color.Black)
            };

            _applicationId_Input.Changed += ApplicationIdInput_Changed;

            _window = new Window("Discord Rich Presence Editor")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _connect = new Button("Connect")
            {
                X = 3,
                Y = Pos.Bottom(_applicationId),

                Width = 10
            };
            _connect.Clicked += Connect_Clicked;

            _state = new Label("Not connected")
            {
                X = 3,
                Y = Pos.Bottom(_connect) + 4
            };

            _currentPresence_Name = new Label("")
            {
                X = 3,
                Y = Pos.Bottom(_state) + 4
            };

            _editWindow = new Window("Edit Presence")
            {
                X = 0,
                Y = Pos.Bottom(_currentPresence_Name)
            };
            _enterPresenceName_Input = new TextField("")
            {
                X = Pos.Right(_enterPresenceName),
                Y = 0,
                Width = 30
            };

            _enterPresenceState = new Label("State:")
            {
                X = 0,
                Y = Pos.Bottom(_enterPresenceName)
            };

            _enterPresenceState_Input = new TextField("")
            {
                X = Pos.Right(_enterPresenceState),
                Y = Pos.Bottom(_enterPresenceName),
                Width = 30
            };

            _sendPresence = new Button("Update")
            {
                X = 0,
                Y = Pos.Bottom(_enterPresenceState)
            };

            _clearPresence = new Button("Clear")
            {
                X = Pos.Right(_sendPresence),
                Y = Pos.Bottom(_enterPresenceState)
            };

            _sendPresence.Clicked += UpdatePresence_Clicked;

            _clearPresence.Clicked += ClearPresence_Clicked;

            _editWindow.Add(_enterPresenceName, _enterPresenceName_Input, _enterPresenceState, _enterPresenceState_Input, _sendPresence, _clearPresence);
        }

        public void Run()
        {
            var top = Application.Top;
            top.Add(_window);
            
            _window.Add(_applicationId, _applicationId_Input, _applicationId_Error, _connect,
                _state, _currentPresence_Name, _editWindow);

            Application.Run();
        }

        public static void Main(string[] args) => new Program().Run();

        private void ApplicationIdInput_Changed(object sender, EventArgs e)
        {
            _appId_Errored = !ulong.TryParse(_applicationId_Input.Text.ToString(), out _appId);
            if (_appId_Errored)
            {
                _applicationId_Error.Text = "Invalid application ID";
            } else
            {
                _applicationId_Error.Text = "";
            }

        }

        private void Connect_Clicked()
        {
            if (_appId_Errored) return;
            _client = new DiscordRpcClient(_appId.ToString());
            _client.Initialize();

            _client.OnReady += _client_OnReady;
            _client.OnPresenceUpdate += _client_OnPresenceUpdate;
        }

        private void _client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            _currentPresence_Name.Text = _client.CurrentPresence?.Details ?? "None";
        }

        private Window _editWindow;
        private Label _enterPresenceName = new Label("Name:");
        private TextField _enterPresenceName_Input;

        private Label _enterPresenceState;
        private TextField _enterPresenceState_Input;

        private Button _sendPresence;
        private Button _clearPresence;

        private void _client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            _state.Text = $"Ready. Connected as {args.User} with RPC version {args.Version}.";
        }

        private void UpdatePresence_Clicked()
        {
            _client.SetPresence(new RichPresence
            {
                Details = _enterPresenceName_Input.Text.ToString(),
                State = _enterPresenceState_Input.Text.ToString()
            });
        }

        private void ClearPresence_Clicked()
        {
            _client.ClearPresence();
        }
    }
}
