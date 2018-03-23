using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.PayAPI.Tests
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void ApiVersionAttributeEnsure()
        {
            var controllerTypes = typeof(Startup).Assembly
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Controller)))
                .ToList();


            IEnumerable<Type> failedControllers = controllerTypes
                .Where(x => Attribute.GetCustomAttribute(x, typeof(ApiVersionAttribute)) == null)
                .ToList();

            if (failedControllers.Any())
            {
                throw new Exception($"These controllers don't have ApiVersion attribute: {string.Join(',', failedControllers.Select(x => x.Name))}");
            }

            Assert.IsTrue(true);
        }
    }
}
