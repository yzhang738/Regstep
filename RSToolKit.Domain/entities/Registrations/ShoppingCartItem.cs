using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{

    public class ShoppingCartItem
    {
        private string pr_name = "";
        private Guid pr_uid = Guid.Empty;
        public int Quanity { get; set; }
        public decimal Ammount { get; set; }
        public string Name
        {
            get
            {
                return pr_name;
            }
        }
        public Guid UId
        {
            get
            {
                return pr_uid;
            }
        }

        public ShoppingCartItem(Components.Component component)
        {
            Quanity = 1;
            Ammount = 0.00m;
            pr_name = component.LabelText;
            pr_uid = component.UId;
        }

        public ShoppingCartItem(string label, decimal amount, int quanity = 1)
        {
            Quanity = 1;
            Ammount = amount;
            pr_uid = Guid.Empty;
            pr_name = label;
        }

    }

}
