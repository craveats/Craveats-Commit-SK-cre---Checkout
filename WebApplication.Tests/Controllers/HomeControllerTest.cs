using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Generic.Obfuscation.TripleDES;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication;
using WebApplication.Controllers;

namespace WebApplication.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            string org1 = "1", org2 = "one", org3 = "this is some data";
            byte[] key = null, iv = null;

            Assert.AreEqual(org1, DataSecurityTripleDES.GetPlainText(
                DataSecurityTripleDES.GetEncryptedText(org1/*, out key, out iv*/)/*, key, iv*/));
            Assert.AreEqual(org2, DataSecurityTripleDES.GetPlainText(
                DataSecurityTripleDES.GetEncryptedText(org2/*, out key, out iv*/)/*, key, iv*/));
            Assert.AreEqual(org3, DataSecurityTripleDES.GetPlainText(
                DataSecurityTripleDES.GetEncryptedText(org3/*, out key, out iv*/)/*, key, iv*/));
            
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
