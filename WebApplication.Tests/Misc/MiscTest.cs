using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using Generic.Obfuscation.SHA1;
using System.Linq;

namespace WebApplication.Tests.Misc
{
    /// <summary>
    /// Summary description for MiscTest
    /// </summary>
    [TestClass]
    public class MiscTest
    {
        public MiscTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //
        }

        [TestMethod]
        public void TextGetHashedText() {
            SHA1HashProvider sHA1HashProvider = new SHA1HashProvider();
            //string clrText = "Test123$";
            //string hashedText = sHA1HashProvider.SecureSHA1(clrText);

            //byte[] baLeft = new byte[4], 
            //    baRight = new byte[4];
            //RandomNumberGenerator rngInstance = RandomNumberGenerator.Create();
            //rngInstance.GetBytes(baLeft);
            //rngInstance.GetBytes(baRight);

            //string left = BitConverter.ToString(baLeft).Replace("-",""), 
            //    right = BitConverter.ToString(baRight).Replace("-", ""),  
            //    tempHashText = sHA1HashProvider.HashSHA1(clrText + left + right),
            //    finalBlock = left+tempHashText+right;

            string org = "Test123$",
                hashedText = sHA1HashProvider.SecureSHA1(org);

            string
                rSS = sHA1HashProvider.GetRandomHexString(9);
            string
                rSE = new string(sHA1HashProvider.GetRandomHexString(9).Reverse().ToArray());// new string(rSS.Reverse().ToArray());

            //byte[] baLeft = new byte[4],
            //   baRight = new byte[4];
            //RandomNumberGenerator rngInstance = RandomNumberGenerator.Create();
            //rngInstance.GetBytes(baLeft);
            //rngInstance.GetBytes(baRight);

            //string rSS = System.BitConverter.ToString(baLeft).Replace("-", ""),
            //    rSE = System.BitConverter.ToString(baRight).Replace("-", "");

            string
                cTWPATH = sHA1HashProvider.HashSHA1(org + rSS + rSE),
                replica = rSS + cTWPATH + rSE
                ;

            Assert.AreSame(hashedText, replica);
        }
    }
}
