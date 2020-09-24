using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace UnitTests.Builder
{
    public class ColidEntryContactBuilder
    {
        private ColidEntryContactsCto _cep = new ColidEntryContactsCto();

        public ColidEntryContactBuilder()
        {
            _cep.Contacts = new List<ContactCto>();
        }

        public ColidEntryContactsCto Build()
        {
            return _cep;
        }

        public ColidEntryContactBuilder GenerateSampleData()
        {
            WithPidUri(new Uri("https://pid.bayer.com/URI5020-2"));
            WithLabel("ID5020");

            var cgContact = new ContactBuilder("anotheruser@bayer.com", new Uri("https://pid.bayer.com/kos/19050/hasConsumerGroupContactPerson"), "Consumer group contact").Build();
            WithCGContact(cgContact);
            
            var cp1 = new ContactBuilder().GenerateSampleData().Build();
            var cp2 = new ContactBuilder("christian.kaubisch.ext@bayer.com", new Uri("https://pid.bayer.com/kos/19050/author"), "Author").Build();
            var cp3 = new ContactBuilder("tim.odenthal.ext@bayer.com", new Uri("https://pid.bayer.com/kos/19050/lastChangeUser"), "Last Change User").Build();

            _cep.Contacts.Append(cp1);
            _cep.Contacts.Append(cp2);
            _cep.Contacts.Append(cp3);

            return this;
        }

        public ColidEntryContactBuilder WithPidUri(Uri pidUri)
        {
            _cep.PidUri = pidUri;
            return this;
        }

        public ColidEntryContactBuilder WithLabel(string label)
        {
            _cep.Label = label;
            return this;
        }

        public ColidEntryContactBuilder WithCGContact(ContactCto contact)
        {
            _cep.ConsumerGroupContact = contact;
            return this;
        }

        public ColidEntryContactBuilder FilterByEmail(string email)
        {
            _cep.Contacts = _cep.Contacts.Where(t => t.EmailAddress != email);

            return this;
        }
    }
}
