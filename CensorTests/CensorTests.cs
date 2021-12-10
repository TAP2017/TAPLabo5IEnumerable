using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Censor;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;
using NUnit.Framework;

namespace CensorTests {
    // Da usare come stub invece di Moq
    // oppure come mock per behavior test in cui
    // si verifica che ogni elemento sia letto una sola volta
    [TestFixture]
    public class MoqTest{
        [Test]
        public void A(){
            var currentString = "pippo";
            var length = 5;
            var step = 0;
            var current = new Mock<I>();
            current.Setup(x => x.Message).Returns(()=>currentString);
            var cursor = new Mock<IEnumerator<I>>();
            cursor.Setup(x => x.Current).Returns(current.Object);
            cursor.Setup(x => x.MoveNext()).Returns(()=>step<length).Callback(() => { currentString += "X";
                step++;
            });
            var sequence = new Mock<IEnumerable<I>>();
            sequence.Setup(x => x.GetEnumerator()).Returns(cursor.Object);
            foreach (var x in sequence.Object){
                Console.WriteLine(x.Message);
            }

            var result = CensorClass.Censor(sequence.Object, "pluto");
            Assert.That(result.Count()==length);
        }
    }
    internal class MyI : I{
        private readonly string _message;

        public int MessageRead{ get; private set; }

        internal MyI(string msg){
            _message = msg;
            MessageRead = 0;
        }

        public string Message{
            get{
                MessageRead++;
                return _message; }
        }
    }
    [TestFixture]
    public class CensorTests {
        [TestCase()]
        [TestCase("pippo")]
        [TestCase("pippo","pluto")]
        [TestCase("pippo","pluto","paperino")]
        public void NothingToFilter(params string[] theMsgSequence){
            var theSequence = new Mock<I>[theMsgSequence.Length];
            for (int i = 0; i < theMsgSequence.Length; i++){
                theSequence[i] = new Mock<I>();
                theSequence[i].Setup(x => x.Message).Returns(theMsgSequence[i]);
            }

            var enumerable = theSequence.Select(m => m.Object);
            var result = CensorClass.Censor(enumerable, "kkzt");
            Assert.That(result,Is.EqualTo(enumerable));
        }
        /*
         *Approccio privo di senso: EqualTo va in loop
         * ..e non c'è modo di verificare elemento a elemento l'uguaglianza
         */
        public void NothingToFilterInfinite(){
            IEnumerable<I> Infinite(){
                while (true){
                    var a = new Mock<I>();
                    a.Setup(x => x.Message).Returns("puffo");
                    yield return a.Object;
                }
            }

            var enumerable = Infinite();
            var result = CensorClass.Censor(enumerable, "kkzt");
            Assert.That(result, Is.EqualTo(enumerable));
        }

        [TestCase(10)]
        public void NothingToFilterInfiniteApprox(int approx){
            var theSequence = CreateFiniteSequence(approx);

            IEnumerable<I> Infinite(){
                    for (int i = 0; i < approx; i++)
                        yield return theSequence[i];
                    while (true){
                        var a = new Mock<I>();
                        a.Setup(x => x.Message).Returns("puffo");
                        yield return a.Object;
                    }}

                var result = CensorClass.Censor(Infinite(), "kkzt").Take(approx);
                Assert.That(result,Is.EqualTo(theSequence));
            }
        private static I[] CreateFiniteSequence(string[] messages){
            var theSequence = new I[messages.Length];
            for (int i = 0; i < messages.Length; i++){
                var a = new Mock<I>();
                a.Setup(x => x.Message).Returns(messages[i]);
                theSequence[i] = a.Object;
            }

            return theSequence;
        }
        private static I[] CreateFiniteSequence(int approx){
            var theSequence = new I[approx];
            for (int i = 0; i < approx; i++){
                var a = new Mock<I>();
                a.Setup(x => x.Message).Returns("puffo");
                theSequence[i] = a.Object;
            }

            return theSequence;
        }

        // La sequenza di input contiene un unico elemento che contiene badWord in mezzo al messaggio
        [Test]
        public void OneMatchInTheMiddle(){
            var badWord = "vai a quel paese";
            var mock = new Mock<I>();
            mock.Setup(x => x.Message).Returns($"jkashgahlkjgh {badWord}uihsehghek");
            var result = CensorClass.Censor(new[]{ mock.Object }, badWord);
            Assert.That(result,Is.Empty);
        }
        // individua ed elimina da una sequenza più elementi che contengono la bad word
        [Test]
        public void FilterManyElements(){
            var badWord = "xxx";
            var inputSequenceMsg = new[]
                { "puffo", "topolino", $"prima{badWord}dopo", "paperino", $"{badWord}", $"{badWord} in testa" };
            var myInput = CreateFiniteSequence(inputSequenceMsg);
            //guardare sintassi per slicing e provare a fare refactoring
            var expected = new I[3];
            expected[0] = myInput[0];
            expected[1] = myInput[1];
            expected[2] = myInput[3];
            var result = CensorClass.Censor(myInput, badWord);
            Assert.That(result,Is.EqualTo(expected));
        }

        [Test]
        public void OnNullSequenceThrows(){
            Assert.That(()=>CensorClass.Censor(null,"puffo"),Throws.TypeOf<ArgumentNullException>());
        }
        [Test]
        public void OnSequenceWithNullThrows(){
            var myInput = CreateFiniteSequence(5);
            myInput[2] = null;
            Assert.That(()=>CensorClass.Censor(myInput,"puffo").Take(3).Count(),Throws.TypeOf<ArgumentNullException>());
        }

    }
}
