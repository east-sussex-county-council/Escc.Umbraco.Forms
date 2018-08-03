using Escc.Umbraco.Forms.FieldTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Umbraco.Forms.FieldTypes.Tests
{
    [TestFixture]
    public class EmailFieldTests
    {
        [TestCase("just a name")]
        [TestCase("first.last@example.org.")]
        [TestCase("first.last.@example.org")]
        [TestCase("first.last@example..org")]
        [TestCase("first.last@o'no.org")]
        public void InvalidEmailIsRejected(string email)
        {
            var validator = new Email();

            var isValid = validator.ValidateEmailAddress(email);

            Assert.IsFalse(isValid);
        }

        [TestCase("first.last@example.org")]
        [TestCase("first.o'yes@example.org")]
        public void ValidEmailIsAccepted(string email)
        {
            var validator = new Email();

            var isValid = validator.ValidateEmailAddress(email);

            Assert.IsTrue(isValid);
        }
    }
}
