using System;
using System.Collections.Generic;
using System.Text;

#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif 

namespace Csla.Test.ValidationRules
{
    [TestClass()]
    public class ValidationTests
    {
        [TestMethod()]
        public void TestValidationRulesWithPrivateMember()
        {
            //works now because we are calling ValidationRules.CheckRules() in DataPortal_Create
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager root = HasRulesManager.NewHasRulesManager();
            Assert.AreEqual("<new>", root.Name);
            Assert.AreEqual(true, root.IsValid, "should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);

            root.BeginEdit();
            root.Name = "";
            root.CancelEdit();

            Assert.AreEqual("<new>", root.Name);
            Assert.AreEqual(true, root.IsValid, "should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);

            root.BeginEdit();
            root.Name = "";
            root.ApplyEdit();

            Assert.AreEqual("", root.Name);
            Assert.AreEqual(false, root.IsValid);
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            Assert.AreEqual("Name required", root.BrokenRulesCollection[0].Description);
        }

        [TestMethod()]
        public void TestValidationRulesWithPublicProperty()
        {
            //should work since ValidationRules.CheckRules() is called in DataPortal_Create
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager2 root = HasRulesManager2.NewHasRulesManager2();
            Assert.AreEqual("<new>", root.Name);
            Assert.AreEqual(true, root.IsValid, "should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);

            root.BeginEdit();
            root.Name = "";
            root.CancelEdit();

            Assert.AreEqual("<new>", root.Name);
            Assert.AreEqual(true, root.IsValid, "should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);

            root.BeginEdit();
            root.Name = "";
            root.ApplyEdit();

            Assert.AreEqual("", root.Name);
            Assert.AreEqual(false, root.IsValid);
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            Assert.AreEqual("Name required", root.BrokenRulesCollection[0].Description);
        }

        [TestMethod()]
        public void TestValidationRulesAfterClone()
        {
            //this test uses HasRulesManager2, which assigns criteria._name to its public
            //property in DataPortal_Create.  If it used HasRulesManager, it would fail
            //the first assert, but pass the others
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager2 root = HasRulesManager2.NewHasRulesManager2();
            Assert.AreEqual(true, root.IsValid);
            root.BeginEdit();
            root.Name = "";
            root.ApplyEdit();

            Assert.AreEqual(false, root.IsValid);
            HasRulesManager2 rootClone = root.Clone();
            Assert.AreEqual(false, rootClone.IsValid);

            rootClone.Name = "something";
            Assert.AreEqual(true, rootClone.IsValid);
        }

        [TestMethod()]
        public void BreakRequiredRule()
        {
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager root = HasRulesManager.NewHasRulesManager();
            root.Name = "";

            Assert.AreEqual(false, root.IsValid, "should not be valid");
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            Assert.AreEqual("Name required", root.BrokenRulesCollection[0].Description);
        }

        [TestMethod()]
        public void BreakLengthRule()
        {
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager root = HasRulesManager.NewHasRulesManager();
            root.Name = "12345678901";
            Assert.AreEqual(false, root.IsValid, "should not be valid");
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            //Assert.AreEqual("Name too long", root.GetBrokenRulesCollection[0].Description);
            Assert.AreEqual("The value for Name is too long", root.BrokenRulesCollection[0].Description);

            root.Name = "1234567890";
            Assert.AreEqual(true, root.IsValid, "should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);
        }

        [TestMethod()]
        public void BreakLengthRuleAndClone()
        {
            Csla.ApplicationContext.GlobalContext.Clear();
            HasRulesManager root = HasRulesManager.NewHasRulesManager();
            root.Name = "12345678901";
            Assert.AreEqual(false, root.IsValid, "should not be valid");
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            //Assert.AreEqual("Name too long", root.GetBrokenRulesCollection[0].Description;
            Assert.AreEqual("The value for Name is too long", root.BrokenRulesCollection[0].Description);

            root = (HasRulesManager)(root.Clone());
            Assert.AreEqual(false, root.IsValid, "should not be valid");
            Assert.AreEqual(1, root.BrokenRulesCollection.Count);
            //Assert.AreEqual("Name too long", root.GetBrokenRulesCollection[0].Description;
            Assert.AreEqual("The value for Name is too long", root.BrokenRulesCollection[0].Description);

            root.Name = "1234567890";
            Assert.AreEqual(true, root.IsValid, "Should be valid");
            Assert.AreEqual(0, root.BrokenRulesCollection.Count);
        }
    }
}
