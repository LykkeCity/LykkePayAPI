using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayAPI.Models.Merchant
{
    public class BaseAssetItemModel
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }
    }
}
