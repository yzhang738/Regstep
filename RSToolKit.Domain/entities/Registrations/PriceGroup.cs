using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using RSToolKit.Domain.Entities.Components;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

//COMPLETE
namespace RSToolKit.Domain.Entities
{
    public class PriceGroup : IRSData, IFormItem
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key, ForeignKey("Component")]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [CascadeDelete]
        public virtual List<Prices> Prices { get; set; }
        [CascadeDelete]
        public virtual List<PriceStyle> Styles { get; set; }

        public virtual Component Component { get; set; }
        
        public PriceGroup()
        {
            Prices = new List<Prices>();
            Styles = new List<PriceStyle>();
        }

        public static PriceGroup New(FormsRepository repository, Component component, Company company, User user, string name = null, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new PriceGroup()
            {
                Name = name ?? "New Price Group - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "."
            };
            component.PriceGroup = node;
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return Component.GetForm();
        }

        public INode GetNode()
        {
            return Component.GetNode();
        }

        public decimal? GetPrice(Registrant reg)
        {
            decimal? price = 0.00m;
            if (Prices.Count < 1)
            {
                return null;
            }
            for (int i = 0; i < Prices.Count; i++)
            {
                if (Prices[i].Audiences.Contains(reg.Audience) || Prices[i].Audiences.Count == 0)
                {
                    DateTime now = reg.DateCreated.Date;
                    for (int ind = 0; ind < Prices[i].Price.Count; ind++)
                    {
                        if (reg.DateCreated.CompareTo(Prices[i].Price[ind].Start) >= 0)
                        {
                            price = Prices[i].Price[ind].Amount;
                        }
                    }
                    break;
                }
            }
            return price;
        }

        protected static decimal? GetItemPrice<Titem>(Titem item, RegistrantData dataPoint)
            where Titem : IComponentItem
        {
            var registrant = dataPoint.Registrant;
            // This is the date of the unbroken registration date for the specific item.
            var priceDate = registrant.DateModified;
            // Now we sort old registration data and iterate through them to find the right date for use with pricing.
            foreach (var o_reg in registrant.OldRegistrations.OrderByDescending(r => r.DateModified))
            {
                var t_dataPoint = o_reg.Data.FirstOrDefault(d => d.VariableUId == dataPoint.VariableUId);
                if (t_dataPoint == null)
                    break;
                var t_value = t_dataPoint.Value;
                if (dataPoint.Component is IComponentMultipleSelection)
                {
                    var selection = JsonConvert.DeserializeObject<List<Guid>>(t_value);
                    if (selection.Contains(item.UId))
                        priceDate = o_reg.DateModified;
                    else
                        break;
                }
                else if (t_value == item.UId.ToString() && o_reg.DateCreated != DateTimeOffset.MinValue)
                    priceDate = o_reg.DateModified;
                else
                    break;
            }
            // We need to initialize the price.
            decimal? price = null;
            // No we find the price;
            Prices priceSet = null;
            if (item.PriceGroup != null)
            {
                // First we check with the item pricing.
                // We see if an audience match is found.
                priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                if (priceSet == null)
                    // No audience match so we check for a generic price.
                    priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
            }
            if (priceSet == null)
            {
                // We did not find a price for the item.  We move onto the item parent;
                if (item.Parent.PriceGroup != null)
                {
                    priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
            }
            // We should have a priceSet now.
            if (priceSet == null)
                // There was no price set so we there is no price for this registrant.  We return null;
                return null;
            // Now that we have a price set, we sort the prices by there start date.
            priceSet.Price.Sort((a, b) => a.Start.CompareTo(b.Start));
            foreach (var p in priceSet.Price)
                // Now we check the prices compared to the price start date and select the last match.
                if (priceDate > p.Start)
                    price = p.Amount;
                else
                    break;
            // We return the price.
            return price;
        }

        /// <summary>
        /// Gets the price the registrant has to pay for this component;
        /// </summary>
        /// <param name="component">The component to run the price for.</param>
        /// <param name="registrant">The registrant to get the price for.</param>
        /// <returns>A nullable decimal price.</returns>
        public static decimal? DisplayPrice(IComponent component, Registrant registrant)
        {
            var priceDate = registrant.DateModified;
            var item = component as IComponentItem;
            if (item != null)
                priceDate = registrant.GetModifiedDate(item);

            // We need to initialize the price.
            decimal? price = null;
            Prices priceSet = null;
            if (item != null)
            {
                // We are running this on an item. So we need to get the item price or parent price if none exists.
                if (item.PriceGroup != null)
                {
                    // First we check with the item pricing.
                    // We see if an audience match is found.
                    priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
                if (priceSet == null)
                {
                    // We did not find a price for the item.  We move onto the item parent;
                    if (item.Parent.PriceGroup != null)
                    {
                        // We see if an audience match is found.
                        priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                        if (priceSet == null)
                            // No audience match so we check for a generic price.
                            priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                    }
                }
            }
            else
            {
                // It is not a item holder, so we just return the price.
                if (component.PriceGroup != null)
                {
                    // First we check with the item pricing.
                    // We see if an audience match is found.
                    priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
            }
            if (priceSet == null)
                // No price so we return null;
                return null;
            // Now that we have a price set, we sort the prices by there start date.
            priceSet.Price.Sort((a, b) => a.Start.CompareTo(b.Start));
            foreach (var p in priceSet.Price)
                // Now we check the prices compared to the price start date and select the last match.
                if (priceDate > p.Start)
                    price = p.Amount;
                else
                    break;
            // We return the price.
            return price;
        }

        /// <summary>
        /// Gets the price the registrant has to pay for this component;
        /// </summary>
        /// <param name="component">The component to run the price for.</param>
        /// <param name="registrant">The registrant to get the price for.</param>
        /// <returns>A nullable decimal price.</returns>
        protected static decimal? _DisplayPrice(IComponent component, Registrant registrant)
        {
            IComponentItem item = null;
            if (component is FreeText || component is Input)
                // Inputs and free texts don't have a price so we return null.
                return null;
            if (component is IComponentItem)
            {
                item = component as IComponentItem;
                component = (component as IComponentItem).Parent;
                // This is a component item we need to check if it and parent do not have prices set.
                if (item.PriceGroup == null || item.PriceGroup.Prices.Count == 0)
                    if (component.PriceGroup == null || component.PriceGroup.Prices.Count == 0)
                        return null;
            }
            else if (component.PriceGroup == null || component.PriceGroup.Prices.Count == 0)
                return null;

            // This is the date of the unbroken registration date for the specific item.
            var priceDate = registrant.DateModified;
            // Now we sort old registration data and iterate through them to find the right date for use with pricing.
            registrant.OldRegistrations.Sort((a, b) => b.DateCreated.CompareTo(b.DateCreated));
            foreach (var o_reg in registrant.OldRegistrations)
            {
                var t_data = o_reg.Data.Where(d => d.VariableUId.HasValue && d.VariableUId == component.UId).FirstOrDefault();
                var t_value = t_data != null ? t_data.Value : "";
                if (component is IComponentMultipleSelection)
                {
                    if (String.IsNullOrWhiteSpace(t_value))
                        t_value = "[]";
                    var selection = JsonConvert.DeserializeObject<List<Guid>>(t_value) ?? new List<Guid>();
                    if (selection.Contains(item.UId))
                        priceDate = o_reg.DateCreated;
                    else
                        break;
                }
                else if (t_value == item.UId.ToString())
                    priceDate = o_reg.DateCreated;
                else
                    break;
            }
            // We need to initialize the price.
            decimal? price = null;
            Prices priceSet = null;
            if (item != null)
            {
                // We are running this on an item. So we need to get the item price or parent price if none exists.
                if (item.PriceGroup != null)
                {
                    // First we check with the item pricing.
                    // We see if an audience match is found.
                    priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = item.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
                if (priceSet == null)
                {
                    // We did not find a price for the item.  We move onto the item parent;
                    if (item.Parent.PriceGroup != null)
                    {
                        // We see if an audience match is found.
                        priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                        if (priceSet == null)
                            // No audience match so we check for a generic price.
                            priceSet = item.Parent.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                    }
                }
            }
            else
            {
                // It is not a item holder, so we just return the price.
                if (component.PriceGroup != null)
                {
                    // First we check with the item pricing.
                    // We see if an audience match is found.
                    priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
            }
            if (priceSet == null)
                // No price so we return null;
                return null;
            // Now that we have a price set, we sort the prices by there start date.
            priceSet.Price.Sort((a, b) => a.Start.CompareTo(b.Start));
            foreach (var p in priceSet.Price)
                // Now we check the prices compared to the price start date and select the last match.
                if (priceDate > p.Start)
                    price = p.Amount;
                else
                    break;
            // We return the price.
            return price;
        }

        /// <summary>
        /// Gets the price the registrant has to pay for this component;
        /// </summary>
        /// <typeparam name="Titem">The component type.</typeparam>
        /// <param name="component">The component to run the price for.</param>
        /// <param name="registrant">The registrant to get the price for.</param>
        /// <returns>A nullable decimal price.</returns>
        public static decimal? GetPrice<Titem>(Titem component, Registrant registrant)
            where Titem : IComponent
        {
            if (component == null)
                return null;
            if (component is IComponentItem)
                // We don't get prices from items.  Only from Components.
                return null;
            if (component is FreeText || component is Input)
                // Inputs and free texts don't have a price so we return null.
                return null;
            if (!(component is IComponentItemParent) && (component.PriceGroup == null || component.PriceGroup.Prices.Count == 0))
                return null;

            // Now we grab the datapoint
            var dataPoint = registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
            if (dataPoint == null)
                // The datapoint does not exist so we know they won't be charged for this item.
                return null;
            if (dataPoint.Empty())
                // The datapoint existed, but there is no selection so we again know there is no charge.
                return null;
            
            if (component is IComponentMultipleSelection)
            {
                decimal? t_price = null;
                // This is a special component that allows multiple selections.  We have to iterate over each item and get the price.
                var ms_comp = (IComponentMultipleSelection)component;
                foreach (var item in ms_comp.Children)
                {
                    var t_itemPrice = GetItemPrice(item, dataPoint);
                    if (!t_price.HasValue)
                        t_price = t_itemPrice;
                    else
                        t_price += t_itemPrice ?? 0.00m;
                }
                return t_price;
            }

            // Now all we have left is single selection components;
            // This is the date of the unbroken registration date for the specific item.
            var priceDate = registrant.DateModified;
            // Now we sort old registration data and iterate through them to find the right date for use with pricing.
            foreach (var o_reg in registrant.OldRegistrations.OrderByDescending(r => r.DateModified))
            {
                var t_data = o_reg.Data.Where(d => d.VariableUId.HasValue && d.VariableUId == component.UId).FirstOrDefault();
                var t_value = t_data != null ? t_data.Value : "";
                if (t_value == dataPoint.Value)
                    priceDate = o_reg.DateModified;
                else
                    break;
            }
            // We need to initialize the price.
            decimal? price = null;
            // First we run for components that hold items.
            if (component is IComponentItemParent)
            {
                var item = (component as IComponentItemParent).Children.Where(i => i.UId.ToString() == dataPoint.Value).FirstOrDefault();
                if (item == null)
                    // The item did not exist, so we return no price.
                    return null;
                return GetItemPrice(item, dataPoint);
            }
            else
            {
                // It is not a item holder, so we just return the price.
                Prices priceSet = null;
                if (component.PriceGroup != null)
                {
                    // First we check with the item pricing.
                    // We see if an audience match is found.
                    priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Contains(registrant.Audience)).FirstOrDefault();
                    if (priceSet == null)
                        // No audience match so we check for a generic price.
                        priceSet = component.PriceGroup.Prices.Where(p => p.Audiences.Count == 0).FirstOrDefault();
                }
                if (priceSet == null)
                    // No price so we return null;
                    return null;
                // Now that we have a price set, we sort the prices by there start date.
                priceSet.Price.Sort((a, b) => a.Start.CompareTo(b.Start));
                foreach (var p in priceSet.Price)
                    // Now we check the prices compared to the price start date and select the last match.
                    if (priceDate > p.Start)
                        price = p.Amount;
                    else
                        break;
                // We return the price.
                return price;
            }
        }

        public PriceGroup DeepCopy(IComponent component, IEnumerable<Audience> audiences = null)
        {
            audiences = audiences ?? new List<Audience>();
            var pg = new PriceGroup();
            component.PriceGroup = pg;
            pg.UId = component.UId;
            pg.DateCreated = DateTimeOffset.UtcNow;
            pg.DateModified = pg.DateCreated;
            pg.Name = Name;
            foreach (var prices in Prices)
            {
                var prs = prices.DeepCopy(pg, audiences);
            }
            return pg;
        }

    }
}
