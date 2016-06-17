using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client;
using Raven.Client.Embedded;
using RavenPlayground.SupportingClasses;
using Raven.Client.Linq;

namespace RavenPlayground
{
  [TestClass]
  public class QueryDocumentsForMatchingFieldTest
  {
    private static EmbeddableDocumentStore _store;
    private IDocumentSession _session;

    [AssemblyInitialize]
    public static void SetUp(TestContext context)
    {
      _store = new EmbeddableDocumentStore { RunInMemory = true };
      _store.Initialize();

      //var version = _store.DatabaseCommands.GlobalAdmin.GetBuildNumber();
      //Console.WriteLine("RavenDB Product Version: {0}", version.ProductVersion);
      //Console.WriteLine("RavenDB Build Version: {0}", version.BuildVersion);

      //Console.WriteLine("Creating Index");
      new Items_ByItemId().Execute(_store);
      new Items_ByCustomId().Execute(_store);

      //Console.WriteLine("Adding transformer");
      new Transform_Items().Execute(_store);
      new Transform_ItemsAlternate().Execute(_store);

      using (var session = _store.OpenSession())
      {
        Helper.SetupTestData(session);
      }
    }

    [TestInitialize]
    public void SetUpTest()
    {
      _session = _store.OpenSession();
    }

    [TestCleanup]
    public void Cleanup()
    {
      _session.Dispose();
    }

    [TestMethod]
    public void test_AllTestDataIsStoredAndRetrievable()
    {
      //This test passes under 3.0.30143
      //This test passes under 3.5.35124

      var items = _session.Query<Item>().ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(20, items.Count);
    }

    [TestMethod]
    public void test_QueryByField_RetrievesDocumentWithMatchingField_NoDashOrSpace()
    {
      //This test passes under 3.0.30143
      //This test passes under 3.5.35124

      var items = _session.Query<Item>().Where(i => i.CustomId == "aA1").ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("aA1", items[0].CustomId);
      Assert.AreEqual("itemId4", items[0].ItemId);
    }

    [TestMethod]
    public void test_QueryByField_RetrievesDocumentWithMatchingField_WithDashAndSpace()
    {
      //This test passes under 3.0.30143
      //This test FAILS under 3.5.35124 (zero results returned)

      var items = _session.Query<Item>().Where(i => i.CustomId == "a-A 1").ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("a-A 1", items[0].CustomId);
      Assert.AreEqual("itemId1", items[0].ItemId);
    }

    [TestMethod]
    public void test_QueryByListOfFields()
    {
      //This test passes under 3.0.30143
      //This test FAILS under 3.5.35124 ("a-A 1" is omitted from results)

      var fieldList = new List<string>
      {
        "a-A 1",
        "a-A1",
        "aA 1",
        "aA1"
      };

      var items = _session.Query<Item>().Where(i => i.CustomId.In(fieldList)).ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(4, items.Count);
      Assert.AreEqual("a-A 1", items.Single(item => item.ItemId == "itemId1").CustomId);
      Assert.AreEqual("a-A1",  items.Single(item => item.ItemId == "itemId2").CustomId);
      Assert.AreEqual("aA 1",  items.Single(item => item.ItemId == "itemId3").CustomId);
      Assert.AreEqual("aA1",   items.Single(item => item.ItemId == "itemId4").CustomId);
    }

    [TestMethod]
    public void test_QueryByListOfFieldsCaseIsInsensitive()
    {
      //This test passes under 3.0.30143
      //This test FAILS under 3.5.35124 ("a-a 2" is omitted from results)


      //being case insensitive surprised me, so I wanted to show that in a test.
      // note that after reading docs, I am less surprised.

      var fieldList = new List<string>
      {
        "a-A 2",
        "a-A2",
        "aA 2",
        "aA2"
      };

      var items = _session.Query<Item>().Where(i => i.CustomId.In(fieldList)).ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(4, items.Count);
      Assert.AreEqual("a-a 2", items.Single(item => item.ItemId == "itemId5").CustomId);
      Assert.AreEqual("a-a2",  items.Single(item => item.ItemId == "itemId6").CustomId);
      Assert.AreEqual("aa 2",  items.Single(item => item.ItemId == "itemId7").CustomId);
      Assert.AreEqual("aa2",   items.Single(item => item.ItemId == "itemId8").CustomId);
    }



    //These next tests fiddle with transforms, which is not the issue I was looking at, but found while in the area:
    [TestMethod]
    public void test_Query_WithTransformAfterWhereClause_FindsItemWithMatchingCustomId()
    {
      //This test passes under 3.0.30143
      //This test passes under 3.5.35124

      //This works as I would expect it to.

      var items = _session.Query<Item>()
        .Where(i => i.CustomId == "a-A1")
        .TransformWith<Transform_Items, TransformedItem>()
        .ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("a-A1", items.Single(item => item.ItemId == "itemId2").Cid);
    }

    [TestMethod]
    public void test_Query_WithTransformBeforeWhereClause_FindsItemWithMatchingCustomId()
    {
      //This test fails under 3.0.30143  (finds zero items)
      //This test FAILS under 3.5.35124 (finds zero items)

      //This does not work as I expect it to.
      //If this was not meant to work, I would expect that I would not be able to reference "Cid" in the where clause.

      var items = _session.Query<Item>()
        .TransformWith<Transform_Items, TransformedItem>()
        .Where(i => i.Cid == "a-A1")
        .ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("a-A1", items.Single(item => item.ItemId == "itemId2").Cid);
    }

    [TestMethod]
    public void test_Query_WithTransformBeforeWhereClause_FindsItemWithMatchingCustomId_Alternate()
    {
      //This test passes under 3.0.30143
      //This test passes under 3.5.35124


      //This does work, but I think I may be misunderstanding transformers
      // it seems to be reaching past the transformed item to the original item.

      // Note that the alternate transformer alters the CustomId 
      //  this is to find out if the where is looking at the query result or the transform result.

      // I used an "Alternate" transformer which keeps the "CustomId" field named the same as in the source class (Item)

      var items = _session.Query<Item>()
        .TransformWith<Transform_ItemsAlternate, TransformedItemAlternate>()
        .Where(i => i.CustomId == "a-A1")
        .ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("junk", items.Single(item => item.ItemId == "itemId2").CustomId);
    }

/*
    [TestMethod]
    public void test_Query_WithTransformBeforeWhereClause_FindsItemWithMatchingCustomId_AlternateWithOriginalFieldName()
    {
      //This does not build.  Based on my other findings, if Where were meant to look at the original item
      // I would expect this to build and work.
      // If where is meant to look at the transformed item, then not building this is fine.

      var items = _session.Query<Item>()
        .TransformWith<Transform_Items, TransformedItem>()
        .Where(i => i.CustomId == "a-A1")
        .ToList();

      items.ForEach(Helper.PrintItem);

      Assert.AreEqual(1, items.Count);
      Assert.AreEqual("a-A1", items.Single(item => item.ItemId == "itemId2").CustomId);
    }
*/

  }
}

