using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayAPI.Validation;

namespace Lykke.Service.PayAPI.Models.Mobile.Cashout
{
    /// <summary>
    /// Cashout operation details
    /// </summary>
    public class CashoutModel
    {
        /// <summary>
        /// Gets or sets asset id
        /// </summary>
        [Required]
        public string AssetId { get; set; }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [Range(Double.Epsilon, Double.MaxValue, ErrorMessage = "Cashout amount must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets desired cashout asset
        /// </summary>
        [Required]
        [CashoutAssetExists]
        public string DesiredCashoutAsset { get; set; }
    }
}
