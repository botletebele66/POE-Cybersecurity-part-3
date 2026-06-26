using System;
using System.IO;
using System.Media;

namespace Cybrog.Engine
{
    /// <summary>
    /// Plays Cybrog's greeting audio clip on startup using the built-in
    /// <see cref="SoundPlayer"/> (no extra NuGet dependency needed for a single WAV
    /// playback). Wrapped in defensive error handling so a missing or locked audio
    /// file never crashes the application — it just silently skips playback and
    /// logs the reason via <see cref="LastError"/>.
    /// </summary>
    public class AudioManager
    {
        private readonly string _audioFilePath;
        public string? LastError { get; private set; }

        public AudioManager(string relativeResourcePath = "Resources/Greeting AI .wav")
        {
            _audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeResourcePath);
        }

        /// <summary>Plays the greeting clip asynchronously (non-blocking). Returns true if playback started.</summary>
        public bool PlayGreeting()
        {
            try
            {
                if (!File.Exists(_audioFilePath))
                {
                    LastError = $"Audio file not found at: {_audioFilePath}";
                    return false;
                }

                using var player = new SoundPlayer(_audioFilePath);
                player.Play(); // asynchronous — does not block the UI thread
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }
    }
}
