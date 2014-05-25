using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Biggy.Extensions;


namespace Biggy.SqlCe {
  public static class CmdEx {

    public static void SetNewParameterValues(this DbCommand command, object[] newValues) {
      for (int i = 0; i < command.Parameters.Count; ++i) {
        command.Parameters[i].Value = newValues[i]; // reuse cmd object, copy new values
      }
    }

    public static object[] GetInsertParamValues<T>(this T insertedItem, DbColumnMapping pkMap) {
      var expando = insertedItem.ToExpando();
      var settings = (IDictionary<string, object>)expando;
      var mappedPkPropertyName = pkMap.PropertyName;
      if (pkMap.IsAutoIncementing) {
        var col = settings.FirstOrDefault(x => x.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase));
        settings.Remove(col);
      }

      return settings.Values.ToArray();
    }

    public static object[] GetUpdateParamValues<T>(this T updatedItem, DbColumnMapping pkMap) {
      var expando = updatedItem.ToExpando();
      var settings = (IDictionary<string, object>)expando;
      var mappedPkPropertyName = pkMap.PropertyName;

      var args = (from item in settings
                  where !item.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase)
                     && item.Value != null
                  select item.Value).ToList();
      if (args.Any()) {
        //add the key
        args.Add(settings.First(x => x.Key.Equals(mappedPkPropertyName, StringComparison.OrdinalIgnoreCase))
                        .Value);
      } else {
        throw new InvalidOperationException("No parsable object was sent in - could not divine any name/value pairs");
      }
      return args.ToArray();
    }
  }
}
