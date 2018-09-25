using Discord;
using Discord.WebSocket;
using FoxBot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxBot.App.Services
{
    /// <summary>
    /// This service requires that your bot is being run by a daemon that handles
    /// Exit Code 1 as a restart
    /// </summary>
    public class ReliabilityService
    {
        #region Configuration
        /// <summary>
        /// Duration to wait on a reconnect before restarting
        /// </summary>
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Attempt to reset the client instead of just rebooting?
        /// </summary>
        private static readonly bool _attemptReset = true;

        private static readonly LogSeverity _debug = LogSeverity.Debug;
        private static readonly LogSeverity _info = LogSeverity.Info;
        private static readonly LogSeverity _critical = LogSeverity.Critical;

        private readonly DiscordSocketClient _discord;
        private readonly LogHelper _logHelper;

        private CancellationTokenSource _cts;

        private const string LogSource = "Reliability";

        #endregion

        public ReliabilityService(DiscordSocketClient discord, LogHelper logHelper)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _logHelper = logHelper;

            _discord.Connected += ConnectedAsync;
            _discord.Disconnected += DisconnectedAsync;
        }

        public Task ConnectedAsync()
        {
            // Cancel all previous state checks and reset the CancelToken - client is back online
            _ = _logHelper.DebugAsync("Client reconected, resetting cancel tokens...", LogSource);
            _ = Task.Delay(_timeout, _cts.Token).ContinueWith(async _ =>
            {
                await _logHelper.DebugAsync("Timeout expired. Continuing to check client state...", LogSource);
                await CheckStateAsync();
                await _logHelper.DebugAsync("State came back okay", LogSource);
            });

            return Task.CompletedTask;
        }

        public Task DisconnectedAsync(Exception _e)
        {
            // Check the state after <timeout> to see if we reconnected
            _ = _logHelper.InfoAsync("Client disconnected, started timeout task...", LogSource);
            _ = Task.Delay(_timeout, _cts.Token).ContinueWith(async _ =>
            {
                await _logHelper.DebugAsync("Timeout epxired, continuing to check client state...", LogSource);
                await CheckStateAsync();
                await _logHelper.DebugAsync("State came back okay", LogSource);
            });

            return Task.CompletedTask;
        }

        private async Task CheckStateAsync()
        {
            // Client reconnected, no need to reset
            if (_discord.ConnectionState == ConnectionState.Connected)
            {
                return;
            }

            if (_attemptReset)
            {
                await _logHelper.InfoAsync("Attempting to reset the client", LogSource);

                var timeout = Task.Delay(_timeout);
                var connect = _discord.StartAsync();
                var task = await Task.WhenAny(timeout, connect);

                if (task == timeout)
                {
                    await _logHelper.CriticalAsync("Client reset timed out (task deadlocked?). Killing process.", LogSource);
                    FailFast();
                }

                else if (connect.IsFaulted)
                {
                    await _logHelper.CriticalAsync("Client reset faulted, killing process", LogSource, connect.Exception);
                }

                else if (connect.IsCompletedSuccessfully)
                {
                    await _logHelper.InfoAsync("Client reset successfully!", LogSource);
                }

                return;
            }

            await _logHelper.CriticalAsync("Client did not reconnect in time, killing process", LogSource);
            FailFast();
        }

        private void FailFast() => Environment.Exit(1);

        // Logging Helpers

    }
}
