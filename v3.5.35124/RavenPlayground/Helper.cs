using System;
using System.Threading;
using Raven.Client;
using RavenPlayground.SupportingClasses;

namespace RavenPlayground
{
  public class Helper
  {
    public static void SetupTestData(IDocumentSession session)
    {
      StoreItem(session, "itemId1", "a-A 1");
      StoreItem(session, "itemId2", "a-A1");
      StoreItem(session, "itemId3", "aA 1");
      StoreItem(session, "itemId4", "aA1");

      StoreItem(session, "itemId5", "a-a 2");
      StoreItem(session, "itemId6", "a-a2");
      StoreItem(session, "itemId7", "aa 2");
      StoreItem(session, "itemId8", "aa2");

      StoreItem(session, "itemId9", "a-A xx");
      StoreItem(session, "itemId10", "a-Axx");
      StoreItem(session, "itemId11", "aA xx");
      StoreItem(session, "itemId12", "aAxx");

      StoreItem(session, "itemId13", "a-A 9");
      StoreItem(session, "itemId14", "a-A 9");
      StoreItem(session, "itemId15", "aA 9");
      StoreItem(session, "itemId16", "aA9");

      StoreItem(session, "itemId17", "a-A 42");
      StoreItem(session, "itemId18", "a-A 42");
      StoreItem(session, "itemId19", "aA 42");
      StoreItem(session, "itemId20", "aA42");

      session.SaveChanges();

      WaitForFreshIndexes(session);
    }

    private static void WaitForFreshIndexes(IDocumentSession session)
    {
      while (session.Advanced.DocumentStore.DatabaseCommands.GetStatistics().StaleIndexes.Length != 0)
      {
        Thread.Sleep(10);
      }
    }

    public static void StoreItem(IDocumentSession session, string itemId, string customId)
    {
      var item = new Item { ItemId = itemId, CustomId = customId };
      session.Store(item);
    }

    public static void PrintItem(Item item)
    {
      Console.WriteLine("ItemId: {0} -  CustomId: {1}", item.ItemId, item.CustomId);
    }

    public static void PrintItem(TransformedItem item)
    {
      Console.WriteLine("ItemId: {0} -  cid: {1}", item.ItemId, item.Cid);
    }
    public static void PrintItem(TransformedItemAlternate item)
    {
      Console.WriteLine("ItemId: {0} -  cid: {1}", item.ItemId, item.CustomId);
    }
  }
}