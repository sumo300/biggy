using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Biggy.Core;
using Biggy.Data.Json;
using Demo.Models;
using System.IO;
using System.Security.Cryptography;
using Biggy.Extensions;

namespace Demo {
  public class Playground {

    //JsonDbCore _db = new JsonDbCore("Playground");


    public void Run() {

      //var artistStore = new JsonStore<Artist>(new JsonDbCore("Chinook"));
      //var memoryArtists = new BiggyList<Artist>();
      //memoryArtists.Add(artistStore.TryLoadData());

      //var single = memoryArtists.FirstOrDefault();
      //single.Name = "Updated";
      //memoryArtists.Update(single);

      //memoryArtists.Clear();

      //memoryArtists.ItemAdded += memoryArtists_ItemAdded;
      //memoryArtists.Add(new Artist { ArtistId = 500, Name = "My Added Artist" });
      var _db = new JsonDbCore("workUnit");
      var artistList = new BiggyList<Artist>(_db.CreateStoreFor<Artist>());
      var syncDictionary = new Dictionary<Artist, IDictionary<string, object>>();
      for (int i = 1; i <= 10; i++) {
        var artist = new Artist { ArtistId = i, Name = "Artist " + i };
        artistList.Add(artist);
      }

      var workUnit = new BiggyWorkUnit<Artist>(artistList);
      workUnit.Add(new Artist { ArtistId = 100, Name = "AddedSingle" });
      var updateMe = workUnit.FirstOrDefault();
      updateMe.Name = "Updated";
      workUnit.Update(updateMe);
      workUnit.SubmitChanges();
      
    }





    void memoryArtists_ItemAdded(object sender, BiggyEventArgs<Artist> e) {
      Console.WriteLine(e.Item.Name);
    }
  }
}
