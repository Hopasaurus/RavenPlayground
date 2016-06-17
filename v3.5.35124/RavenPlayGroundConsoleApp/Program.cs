using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Linq;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverQueried.Local

namespace RavenPlayGroundConsoleApp
{
  class Program
  {
    static void Main()
    {
      Console.WriteLine("Check Property Against List Demo.");

      using (var store = new EmbeddableDocumentStore { RunInMemory = true })
      {
        Console.WriteLine("Initilizing Store");

        store.Initialize();

        var version = store.DatabaseCommands.GlobalAdmin.GetBuildNumber();
        Console.WriteLine("RavenDB Product Version: {0}", version.ProductVersion);
        Console.WriteLine("RavenDB Build Version: {0}", version.BuildVersion);

        using (var session = store.OpenSession())
        {
          SetupItems(session);
          //        }
          //
          //        using (var session = store.OpenSession())
          //        {
          QueryAllItems(session);
          QueryItemsInList(session);
          WaitForKey();
        }
      }
    }

    private static void SetupItems(IDocumentSession session)
    {
      Console.WriteLine("Storing Items");

      StoreItem(session, "itemId1", "a-A 1");  //this will not be found in v3.5
      StoreItem(session, "itemId2", "a-A1");
      StoreItem(session, "itemId3", "aA 1");
      StoreItem(session, "itemId4", "aA1");
      StoreItem(session, "itemId5", " .");  //just a space did not work in either version so tried another punctiation in combination with space
      StoreItem(session, "itemId6", "-");
      StoreItem(session, "itemId7", " -");  //this will not be found in v3.5
      StoreItem(session, "itemId8", "- ");  //this will not be found in v3.5

      Console.WriteLine("Saving changes");
      session.SaveChanges();

      Console.WriteLine("Taking a nap while indexing happens");
      while (session.Advanced.DocumentStore.DatabaseCommands.GetStatistics().StaleIndexes.Length != 0)
      {
        Thread.Sleep(10);
      }
    }

    private static void StoreItem(IDocumentSession session, string itemId, string customId)
    {
      var item = new Item { ItemId = itemId, CustomId = customId };
      session.Store(item);
    }

    private static void QueryAllItems(IDocumentSession session)
    {
      //this is just a sanity check to make sure setup is correct.
      Console.WriteLine("Querying all items");
      var allItems = session.Query<Item>().ToList();

      if (allItems.Count != 8)
        Console.WriteLine("XXXXXXXXXXXXXX  Error: expected 8 items in list, actual is: {0}", allItems.Count);

      Console.WriteLine("There are {0} items in the all items list:", allItems.Count);
      allItems.ForEach(PrintItem);

      Console.WriteLine("");
    }

    private static void QueryItemsInList(IDocumentSession session)
    {
      //under 3.0.30143 this pulls back four items as expected.

      Console.WriteLine("Querying items in list");

      var itemList = new List<string>
      {
        "a-a 1",
        "a-a1",
        "aa 1",
        "aa1",
        " .",
        "-",
        " -",
        "- "
      };

      var itemsInList = session.Query<Item>()
        .Where(i => i.CustomId.In(itemList));

      CheckListIsCorrect("No Index", itemsInList, itemList);
    }

    private static void CheckListIsCorrect(string message, IRavenQueryable<Item> itemsInList, List<string> itemList)
    {
      //Console.WriteLine($"Checking {message}");
      var itemListCount = itemsInList.Count();

      if (itemListCount != itemList.Count)
        Console.WriteLine("XXXXXXXXXXXXXX  Error: Expected {0} items in list, actual is: {1}", itemList.Count, itemListCount);

      Console.WriteLine("There are {0} items in the some items list:", itemListCount);
      itemsInList.ForEach(PrintItem);
      Console.WriteLine("");
    }

    private static void PrintItem(Item item)
    {
      Console.WriteLine("ItemId: {0} -  CustomId: {1}", item.ItemId, item.CustomId);
    }

    private static void WaitForKey()
    {
      Console.WriteLine("Press any key to quit...");
      Console.ReadKey(true);
    }
  }

  public class Item
  {
    public string ItemId { get; set; }
    public string CustomId { get; set; }
  }
}
