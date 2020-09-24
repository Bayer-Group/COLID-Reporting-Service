using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using COLID.Graph.TripleStore.DataModels.Base;
using COLID.Graph.TripleStore.Extensions;

namespace UnitTests.Builder
{
    public abstract class AbstractEntityBuilder<T> where T : Entity
    {
        protected IDictionary<string, List<dynamic>> _prop = new Dictionary<string, List<dynamic>>();

        public abstract T Build();

        protected void CreateOrOverwriteProperty(string identifier, dynamic content)
        {
            _prop.AddOrUpdate(identifier, new List<dynamic>() { content });
        }

        protected void CreateOrOverwriteMultiProperty(string identifier, List<dynamic> content)
        {
            _prop.AddOrUpdate(identifier, content);
        }

        protected void CheckArgument(string value, string regex)
        {
            var match = Regex.Match(value, regex);
            if (!match.Success)
            {
                throw new ArgumentException(string.Format("Passed argument {0} doesn't match with the valid regex pattern {1}", value, regex));
            }
        }
    }
}
