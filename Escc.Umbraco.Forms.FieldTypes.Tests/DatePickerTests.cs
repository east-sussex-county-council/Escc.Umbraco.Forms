using System;
using System.Collections.Generic;
using Escc.EastSussexGovUK.Umbraco.Forms.FieldTypes;
using NUnit.Framework;

namespace Escc.Umbraco.Forms.FieldTypes.Tests
{
    [TestFixture]
    public class DatePickerTests
    {
        [Test]
        public void ValidDateIsAccepted()
        {
            var validator = new DatePicker();
            var errorMessages = new List<string>();

            validator.ValidateDate("1753-01-01 00:00", errorMessages);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void InvalidDateIsRejected()
        {
            var validator = new DatePicker();
            var errorMessages = new List<string>();

            validator.ValidateDate("1752-12-31 23:59", errorMessages);

            Assert.AreEqual(1, errorMessages.Count);
        }
    }
}
