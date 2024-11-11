using NUnit.Framework;
using Assets._2RGuide.Runtime;

namespace Assets.Tests.EditModeTests
{
    public class PriorityQueueTests
    {
        [Test]
        public void ItemsQueuedInPriority()
        {
            var queue = new PriorityQueue<string, float>();

            queue.Enqueue("a", 105.0f);
            queue.Enqueue("b", 5.0f);
            queue.Enqueue("c", 7.0f);

            Assert.AreEqual(3, queue.Count);
            Assert.AreEqual("b", queue.Dequeue());
            Assert.AreEqual("c", queue.Dequeue());
            Assert.AreEqual("a", queue.Dequeue());
        }
    }
}