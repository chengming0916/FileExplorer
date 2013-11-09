using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;
using NUnit.Framework;

namespace FileExplorer.UnitTests
{
    [TestFixture]
    public class UtilTest
    {
        public class C
        {
            public string Value { get { return "C"; } }
        }

        public class B
        {
            public B() { C = new C(); }
            public C C { get; set; }
            public string Value { get { return "B"; } }
        }

        public class A
        {
            public A() { B = new B(); }
            public B B { get; set; }
            public string Value { get { return "A"; } }
        }


        [Test]
        public static void PropertyPathHelper_Test()
        {
            var a = new A();

            object valueA = PropertyPathHelper.GetValueFromPropertyInfo(a, "Value");
            object valueB = PropertyPathHelper.GetValueFromPropertyInfo(a, "B.Value");
            object valueC = PropertyPathHelper.GetValueFromPropertyInfo(a, "B.C.Value");
            bool cachedA = 
                PropertyPathHelper._cacheDic.ContainsKey(new Tuple<Type, string>(typeof(A), "Value"));
            bool cachedC =
                PropertyPathHelper._cacheDic.ContainsKey(new Tuple<Type, string>(typeof(C), "Value"));


            Assert.AreEqual("A", valueA);
            Assert.AreEqual("B", valueB);
            Assert.AreEqual("C", valueC);
            Assert.AreEqual("C", valueC);

            Assert.IsTrue(cachedA);
            Assert.IsTrue(cachedC);
        }
    }
}
