using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Invoice
{
    public static class Extensions
    {
        public static IReadOnlyList<FilterItemModel> ToListOfFilterItems(this IReadOnlyDictionary<string, string> dictionary)
        {
            var result = new List<FilterItemModel>();

            if (dictionary == null)
                return result;

            foreach (var item in dictionary)
            {
                result.Add(new FilterItemModel
                {
                    Id = item.Key,
                    Value = item.Value
                });
            }

            return result;
        }
    }
}
