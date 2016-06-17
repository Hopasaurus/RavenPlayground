using System.Linq;
using Raven.Client.Indexes;

// ReSharper disable InconsistentNaming

namespace RavenPlayground.SupportingClasses
{
  public class Transform_ItemsAlternate : AbstractTransformerCreationTask<Item>
  {
    public Transform_ItemsAlternate()
    {
      TransformResults = items => from item in items
        select new
        {
          item.ItemId,
          CustomId = "junk",
        };
    }
  }
}