using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.Models;

namespace Demo {
  public class SampleData {

    public static List<ArtistDocument> GetSampleArtistDocuments(int qty = 1000, int qtyAlbumsPerArtist = 5, int qtyTracksPerAlbum = 8) {
      var newArtists = new List<ArtistDocument>();
      int albumIdIndex = 1;
      int trackIdIndex = 1;
      for (int i = 1; i <= qty; i++) {
        var newArtist = new ArtistDocument { ArtistDocumentId = i, Name = "New Artist " + i };
        for (int j = 1; j <= qtyAlbumsPerArtist; j++) {
          var newAlbum = new AlbumDocument { AlbumId = albumIdIndex, ArtistId = newArtist.ArtistDocumentId, Title = newArtist.Name + ": Super Awesome Album #" + i };
          for (int k = 1; k <= qtyTracksPerAlbum; k++) {
            var newTrack = new Track { TrackId = trackIdIndex, AlbumId = newAlbum.AlbumId, Name = "Album " + newAlbum.Title + "/Track " + i };
            newAlbum.Tracks.Add(newTrack);
            trackIdIndex++;
          }
          newArtist.Albums.Add(newAlbum);
          albumIdIndex++;
        }
        newArtists.Add(newArtist);
      }
      return newArtists;
    }

    public static List<Artist> GetSampleArtists(int qty = 1000) {
      var newArtists = new List<Artist>();
      for (int i = 1; i <= qty; i++) {
        newArtists.Add(new Artist { ArtistId = i, Name = "New Artist " + i });
      }
      return newArtists;
    }

    public static List<Album> GetSampleAlbums(int qtyPerArtist = 5) {
      var sampleArtists = GetSampleArtists();
      var sampleAlbums = new List<Album>();
      int albumIdIndex = 1;
      foreach (var artist in sampleArtists) {
        var artistAlbums = new List<Album>();
        for (int i = 1; i <= qtyPerArtist; i++) {
          artistAlbums.Add(new Album { AlbumId = albumIdIndex, ArtistId = artist.ArtistId, Title = artist.Name + ": Super Awesome Album #" + i });
          albumIdIndex++;
        }
        sampleAlbums.AddRange(artistAlbums);
      }
      return sampleAlbums;
    }

    public static List<Track> GetSampleTracks(int qtyPerAlbum = 8) {
      var sampleAlbums = GetSampleAlbums();
      var sampleTracks = new List<Track>();
      int trackIdIndex = 1;
      foreach (var album in sampleAlbums) {
        var albumTracks = new List<Track>();
        for (int i = 1; i <= qtyPerAlbum; i++) {
          albumTracks.Add(new Track { TrackId = trackIdIndex, AlbumId = album.AlbumId, Name = "Album " + album.Title + "/Track " + i });
          trackIdIndex++;
        }
        sampleTracks.AddRange(albumTracks);
      }
      return sampleTracks;
    }


  }
}
