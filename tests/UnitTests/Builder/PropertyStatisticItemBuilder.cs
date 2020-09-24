using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.UnitTests.Builder
{
    public class PropertyStatisticItemBuilder
    {
        private PropertyStatisticItem _statisticItem;

        public PropertyStatisticItemBuilder()
        {
            _statisticItem = new PropertyStatisticItem();
        }

        public PropertyStatisticItemBuilder(string key, string value, string property, string total = null)
        {
            _statisticItem = new PropertyStatisticItem(key, value, property, total);
        }

        public PropertyStatisticItem Build()
        {
            return _statisticItem;
        }

        public PropertyStatisticItemBuilder WithKey(string key)
        {
            _statisticItem.Key = key;
            return this;
        }

        public PropertyStatisticItemBuilder WithValue(string value)
        {
            _statisticItem.Value = value;
            return this;
        }

        public PropertyStatisticItemBuilder WithProperty(string property)
        {
            _statisticItem.Property = property;
            return this;
        }

        public PropertyStatisticItemBuilder WithTotalItems(string total)
        {
            _statisticItem.Total = total;
            return this;
        }
    }
}
