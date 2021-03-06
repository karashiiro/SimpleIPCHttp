﻿using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleIPCHttp.Tests
{
    [TestFixture]
    public class IpcInterfaceBenchmarks
    {
        private const int Iterations = 10000;

        // ReSharper disable InconsistentNaming
        private IpcInterface i1;
        private IpcInterface i2;
        // ReSharper restore InconsistentNaming

        [Test]
        public async Task SendMessage_AvgTimeIsBelow1Ms()
        {
            i1 = new IpcInterface();
            i2 = new IpcInterface(i1.PartnerPort, i1.Port);
            var stopwatch = new Stopwatch();

            i1.On<DummyClass>(dummyClass => {});

            for (var i = 0; i < Iterations; i++)
            {
                stopwatch.Start();
                await i2.SendMessage(new DummyClass());
                stopwatch.Stop();
            }

            var averageMs = stopwatch.ElapsedMilliseconds / Iterations;
            Assert.IsTrue(averageMs <= 1, "Expected <=1ms, got {0}ms", averageMs);
        }

        [Test]
        public async Task ReceiveMessage_AvgTimeIsBelow1Ms()
        {
            i1 = new IpcInterface();
            i2 = new IpcInterface(i1.PartnerPort, i1.Port);
            var dummyClass = new DummyClass();
            var stopwatch = new Stopwatch();

            var spinLock = true;
            i1.On<DummyClass>(dc =>
            {
                stopwatch.Stop();
                spinLock = false;
            });

            for (var i = 0; i < Iterations; i++)
            {
                await i2.SendMessage(dummyClass);
                stopwatch.Start();
                while (spinLock)
                    await Task.Delay(1);
                spinLock = true;
            }

            var averageMs = stopwatch.ElapsedMilliseconds / Iterations;
            Assert.IsTrue(averageMs <= 1, "Expected <=1ms, got {0}ms", averageMs);
        }
    }
}
