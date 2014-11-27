﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using LanguageExt;
using LanguageExt.List;
using LanguageExt.Prelude;
using LanguageExt.Process;

namespace LanguageExtTests
{
    [TestFixture]
    public class ProcessTests
    {
        [Test]
        public void SpawnProcess()
        {
            restart();

            string value = null;
            var pid = spawn<string>("SpawnProcess", msg => value = msg );

            tell(pid, "hello, world");

            Thread.Sleep(200);
            Assert.IsTrue(value == "hello, world");

            kill(pid);
        }

        [Test]
        public void SpawnErrorSurviveProcess()
        {
            restart();

            int value = 0;
            int count = 0;

            var pid = spawn<string>("SpawnAnErrorProcess", _ =>
                {
                    if (count++ == 0)
                        throw new Exception("fail");
                    else
                        value = count;
                });

            tell(pid, "msg");
            tell(pid, "msg");
            tell(pid, "msg");

            Thread.Sleep(400);
            Assert.IsTrue(value == 3);

            kill(pid);
        }

        [Test]
        public void SpawnAndKillProcess()
        {
            restart();

            string value = null;
            var pid = spawn<string>("SpawnAndKillProcess", msg => value = msg);
            tell(pid, "1");

            Thread.Sleep(100);

            kill(pid);

            Thread.Sleep(100);

            tell(pid, "2");

            Thread.Sleep(100);

            Assert.IsTrue(value == "1");
            Assert.IsTrue(length(children()) == 0);
        }

        [Test]
        public void SpawnAndKillHierarchy()
        {
            restart();

            string value = null;
            ProcessId parentId;

            var pid = spawn<Unit, string>("SpawnAndKillHierarchy.TopLevel", 
                () =>
                    {
                        parentId = parent();

                        spawn<string>("SpawnAndKillHierarchy.ChildLevel", msg => value = msg);
                        return unit;
                    },
                (state, msg) =>
                    {
                        value = msg;
                        return state;
                    }
            );

            tell(pid, "1");

            Thread.Sleep(100);

            kill(pid);

            Thread.Sleep(100);

            tell(pid, "2");

            Thread.Sleep(100);

            Assert.IsTrue(value == "1");
            Assert.IsTrue(length(children()) == 0);
        }
    }
}