using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace UnitTests.Builder
{
    public class ContactBuilder
    {
        private ContactCto _contact = new ContactCto();

        public ContactBuilder() { }

        public ContactBuilder(string contact, Uri roleId, string role)
        {
            WithContactAdress(contact);
            WithRoleId(roleId);
            WithRole(role);
        }

        public ContactCto Build()
        {
            return _contact;
        }

        public ContactBuilder GenerateSampleData()
        {
            WithContactAdress("marcus.davies@bayer.com");
            WithRoleId(new Uri("https://pid.bayer.com/kos/19050/hasContactPerson"));
            WithRole("Contact Person");

            return this;
        }

        public ContactBuilder WithContactAdress(string email)
        {
            _contact.EmailAddress = email;
            return this;
        }

        public ContactBuilder WithRoleId(Uri roleId)
        {
            _contact.TypeUri = roleId;
            return this;
        }

        public ContactBuilder WithRole(string role)
        {
            _contact.TypeLabel = role;
            return this;
        }
    }
}
