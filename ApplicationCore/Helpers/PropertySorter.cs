using System;
using System.Collections;
using System.ComponentModel;

namespace FDS.Core.SettingsMgmt
{

    /// <summary>
    /// Adapted from : http://www.codeproject.com/Articles/6611/Ordering-Items-in-the-Property-Grid
    /// Paul Tingey 2004 
    //  The Code Project Open License (CPOL)
    /// </summary>
    public class PropertySorter : ExpandableObjectConverter
    {
        #region Methods

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
            Attribute[] attributes)
        {
            //
            // This override returns a list of properties in order
            //
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(value, attributes);
            var orderedProperties = new ArrayList();
            foreach (PropertyDescriptor pd in pdc)
            {
                Attribute attribute = pd.Attributes[typeof (PropertyOrderAttribute)];
                if (attribute != null)
                {
                    //
                    // If the attribute is found, then create an pair object to hold it
                    //
                    var poa = (PropertyOrderAttribute) attribute;
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, poa.Order));
                }
                else
                {
                    //
                    // If no order attribute is specifed then given it an order of 0
                    //
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, 0));
                }
            }
            //
            // Perform the actual order using the value PropertyOrderPair classes
            // implementation of IComparable to sort
            //
            orderedProperties.Sort();
            //
            // Build a string list of the ordered names
            //
            var propertyNames = new ArrayList();
            foreach (PropertyOrderPair pop in orderedProperties)
            {
                propertyNames.Add(pop.Name);
            }
            //
            // Pass in the ordered list for the PropertyDescriptorCollection to sort by
            //
            return pdc.Sort((string[]) propertyNames.ToArray(typeof (string)));
        }

        #endregion
    }

    #region Helper Class - PropertyOrderAttribute

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyOrderAttribute : Attribute
    {
        //
        // Simple attribute to allow the order of a property to be specified
        //
        private readonly int _order;

        public PropertyOrderAttribute(int order)
        {
            _order = order;
        }

        public int Order
        {
            get { return _order; }
        }
    }

    #endregion

    #region Helper Class - PropertyOrderPair

    public class PropertyOrderPair : IComparable
    {
        private readonly string _name;
        private readonly int _order;

        public PropertyOrderPair(string name, int order)
        {
            _order = order;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public int CompareTo(object obj)
        {
            //
            // Sort the pair objects by ordering by order value
            // Equal values get the same rank
            //
            int otherOrder = ((PropertyOrderPair) obj)._order;
            if (otherOrder == _order)
            {
                //
                // If order not specified, sort by name
                //
                string otherName = ((PropertyOrderPair) obj)._name;
                return string.Compare(_name, otherName);
            }
            if (otherOrder > _order)
            {
                return -1;
            }
            return 1;
        }
    }

    #endregion
}
