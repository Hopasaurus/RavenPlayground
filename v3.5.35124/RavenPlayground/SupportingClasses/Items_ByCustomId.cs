using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

// ReSharper disable InconsistentNaming

namespace RavenPlayground.SupportingClasses
{
  public class Items_ByCustomId : AbstractIndexCreationTask<Item>
  {
    public Items_ByCustomId()
    {
      Map = items => from item in items
        select new
        {
          item.ItemId,
          item.CustomId
        };
      Index(persons => persons.CustomId, FieldIndexing.Analyzed);
    }
  }
}