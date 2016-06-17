using System.Linq;
using Raven.Client.Indexes;

// ReSharper disable InconsistentNaming

namespace RavenPlayground.SupportingClasses
{
  public class Transform_Items : AbstractTransformerCreationTask<Item>
  {
    public Transform_Items()
    {
      TransformResults = items => from item in items
        select new
        {
          item.ItemId,
          Cid = item.CustomId
        };
    }
  }
}