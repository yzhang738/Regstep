using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities
{
    public class ShoppingCart
    {
        public Registrant Registrant { get; set; }
        public Form Form { get; set; }
        public string Confirmation { get; set; }
        public List<ShoppingCartItem> Items { get; set; }

        public ShoppingCart(Form form, Registrant registrant)
        {
            Confirmation = "";
            Items = new List<ShoppingCartItem>();
            Form = form;
            Registrant = registrant;
        }

        public ShoppingCart()
        {
            Confirmation = "";
            Items = new List<ShoppingCartItem>();
        }

        /// <summary>
        /// Gets the amount of all registration items selected with no taxes or promotion codes.
        /// </summary>
        /// <returns>A decimal representing a monetary amount.</returns>
        public decimal Total()
        {
            var total = 0.00m;
            foreach (var item in Items)
            {
                total += item.Ammount * item.Quanity;
            }
            return Math.Round(total);
        }

        /// <summary>
        /// Gets the amount of all registrations items including promotion codes but not taxes.
        /// </summary>
        /// <returns>A decimal representing a monetary value.</returns>
        public decimal PreTaxes()
        {
            var total = Total();
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
            {
                total -= promotion.Code.Amount;
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
            {
                total += promotion.Code.Amount;
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
            {
                total *= promotion.Code.Amount;
            }
            return Math.Round(total);
        }

        /// <summary>
        /// Gets the tax amount only.
        /// </summary>
        /// <returns>A decimal representing the tax amount.</returns>
        public decimal Taxes()
        {
            if (Registrant.Data.Where(d => d.Component.Variable.Value == "__NoTax" && d.Value == "true").Count() == 0)
                return Math.Round(PreTaxes() * (Form.Tax.HasValue ? (Form.Tax.Value) : 0));
            return 0.00m;
        }

        /// <summary>
        /// Gets the total of items, promotion codes, and taxes.
        /// </summary>
        /// <returns>A decimal representing a monetary amount.</returns>
        public decimal Calculate()
        {
            var total = Total();
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
            {
                total -= promotion.Code.Amount;
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
            {
                total += promotion.Code.Amount;
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
            {
                total *= promotion.Code.Amount;
            }
            if (Registrant.Data.Where(d => d.Component.Variable.Value == "__NoTax" && d.Value == "true").Count() == 0)
                total += total * (Form.Tax.HasValue ? (Form.Tax.Value) : 0);
            return Math.Round(total);
        }

        public CartHistory ToCart()
        {
            var history = new CartHistory();
            foreach (var item in Items)
            {
                history.Items.Add(new CartHistoryItem()
                    {
                        Name = item.Name,
                        Ammount = item.Ammount,
                        Quanity = item.Quanity
                    });
            }
            history.BeforeDiscount = Total();
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
            {
                history.Items.Add(new CartHistoryItem()
                    {
                        Name = "Promotion Code: " + promotion.Code.Code,
                        Ammount = promotion.Code.Amount * -1,
                        Quanity = 1
                    });
                history.Promotions += (promotion.Code.Amount * -1);
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
            {
                history.Items.Add(new CartHistoryItem()
                {
                    Name = "Promotion Code: " + promotion.Code.Code,
                    Ammount = promotion.Code.Amount * -1,
                    Quanity = 1
                });
                history.Promotions += promotion.Code.Amount;
            }
            foreach (var promotion in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
            {
                var promotionAmmount = history.AfterTaxes * (1 - promotion.Code.Amount);
                history.Items.Add(new CartHistoryItem()
                    {
                        Name = "Promotion COde: " + promotion.Code.Code,
                        Ammount = promotionAmmount * -1,
                        Quanity = 1
                    });
                history.Promotions += promotionAmmount * -1;
            }
            history.BeforeTaxes = history.BeforeDiscount + history.Promotions;
            history.AfterTaxes = history.BeforeTaxes + (Total() * (Form.Tax.HasValue ? (Form.Tax.Value) : 1));
            history.Total = Calculate();
            return history;
        }
    }

    public class CartHistory
    {
        public List<CartHistoryItem> Items { get; set; }
        public decimal BeforeDiscount { get; set; }
        public decimal BeforeTaxes { get; set; }
        public decimal AfterTaxes { get; set; }
        public decimal Promotions { get; set; }
        public decimal Total { get; set; }
        
        public CartHistory()
        {
            Items = new List<CartHistoryItem>();
            BeforeDiscount = AfterTaxes = Promotions = 0m;
        }
    }

    public class CartHistoryItem
    {
        public string Name { get; set; }
        public decimal Ammount { get; set; }
        public int Quanity { get; set; }

        public CartHistoryItem()
        {
            Name = "";
            Quanity = 0;
            Ammount = 0m;
        }
    }
}
