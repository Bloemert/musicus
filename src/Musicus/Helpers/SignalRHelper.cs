﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Musicus.Abstractions.Models;
using Musicus.Managers;
using Musicus.Models;

namespace Musicus.Helpers
{
	public class SignalRHelper
	{
		private IHubContext<MusicusHub> _hubContext;
		private readonly PlayerManager _playerManager;

		protected IMusicServiceStatus DefaultStatus => new MusicServiceStatus
		{
			Artist = "Musicus",
			Track = "Let there be songs to fill the air",
			TrackSource = TrackSource.Spotify,
			AlbumArtWork = "http://icons.iconarchive.com/icons/webalys/kameleon.pics/64/Cloud-Music-icon.png"
		};

		public SignalRHelper(IHubContext<MusicusHub> hubContext, PlayerManager playerManager)
		{
			_hubContext = hubContext;
			_playerManager = playerManager;
		}

		public void SetVolume(float volume)
			=> _hubContext.Clients.All.SendAsync("SetVolume", volume);

		public void SetStatus(IMusicServiceStatus status)
			=> SetStatus(status.Artist, status.Track, status.Current, status.Length, status.AlbumArtWork, status.IsPlaying, status.TrackSource);

		public void SetStatus(string artist, string track, double current, double length, string albumArtWork, bool play, TrackSource? trackSource)
			=> _hubContext.Clients.All.SendAsync(
				"SetStatus",
				new
				{
					Artist = artist,
					Track = track,
					Current = current,
					Length = length,
					AlbumArtWork = albumArtWork,
					Play = play,
					TrackSource = trackSource
				});

		public void SetPlaylist(IList<Track> playlist)
			=> _hubContext.Clients.All.SendAsync("SetQueue", playlist);

		public void StartStatusUpdate()
		{
			var t = new Thread(async () =>
			{
				while (true)
				{
					await StatusUpdateAsync();

					Thread.Sleep(1000);
					//your infinite loop 
				}
			});

			t.Start();
		}

		public async Task StatusUpdateAsync()
		{
			var currentTrack = Playlist.GetCurrentTrack();

			if (currentTrack != null)
			{
				var status = await _playerManager.GetStatusAsync(currentTrack);

				SetStatus(status);
			}
			else
			{
				SetStatus(DefaultStatus);
			}

			SetPlaylist(Playlist.GetPlaylist());
		}
	}
}
