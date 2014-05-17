using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Arcade.Build.FlowStack;
using Arcade.Dsl;
using Arcade.Engine;
using Arcade.Run.Aspects;
using Arcade.Run.Execution;
using Arcade.Run.Messages;
using Arcade.Run.RunVectors;
using Arcade.Tests;
using NUnit.Framework;
using System.Diagnostics;
using System.Reflection;

namespace Windows.Tests.Fixture
{
    [TestFixture]
    public class RuntimeEngineFixture
    {
        private FlowConfiguration[] _flows;
        private RuntimeEngine _sut;
        private RuntimeConfiguration _runtimeConfig;
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private volatile string _lastTriggerValue;

        [SetUp]
        public void SetUp()
        {
            _mre.Reset();

            _runtimeConfig = new RuntimeConfiguration();
            _runtimeConfig.For<IAmAnInterface>().Use(() => new InterfaceImplementation());
            _runtimeConfig.AddAspect(new TracePreExecutionAdvice(Console.WriteLine));

            _flows = new[]
                         {
                             Flow.StartWith<InitialOutflow, string>("SimpleFlow1")
                                 .ContinueWithEbc<ContinueFlow, string, int>(() => new ContinueFlow())
                                 .ContinueWithEbc<CheckFlow, int, bool>(() => new CheckFlow())
                                 .ContinueWithEbc<SomeOtherFlow, bool, string>(() => new SomeOtherFlow())
                                 .Exit(),
                             Flow.StartWith<InitialOutflow, string>("ContinuationFlow1")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .ContinueWithContinuation<IntToStringContinuation, int, string>()
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .Exit(),
                             Flow.StartWith<InitialOutflow, string>("FunctionFlow1")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .ContinueWithFunction<int, string>(i => i.ToString())
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .Exit(),
                             Flow.StartWith<ContinueFlow, string, int>("SimpleFlow2")
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .ContinueWithEbc<SomeOtherFlow, bool, string>()
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("BranchFlow1")
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .BranchWhen(b => !b, configurer =>
                                                      configurer
                                                          .ContinueWithEbc<FlowOutputDependingOnInput, bool, string>(
                                                              () =>
                                                              new FlowOutputDependingOnInput("On Main", "On Branch"))
                                                          .JoinOnExit())
                                 .ContinueWithEbc<FlowOutputDependingOnInput, bool, string>(
                                     () => new FlowOutputDependingOnInput("On Main", "On Branch"))
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("BranchFlow2")
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .BranchWhen(b => b, configurer =>
                                                     configurer
                                                         .ContinueWithEbc<FlowOutputDependingOnInput, bool, string>(
                                                             () =>
                                                             new FlowOutputDependingOnInput("On Branch", "On Main"))
                                                         .JoinAt("Rejoin"))
                                 .ContinueWithEbc<FlowOutputDependingOnInput, bool, string>(
                                     () => new FlowOutputDependingOnInput("On Branch", "On Main"))
                                 .JoinOrContinueWithEbc<ContinueFlow, string, int>("Rejoin")
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("InvalidBranchFlow")
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .BranchWhen(b => b, configurer =>
                                                     configurer.ContinueWithFunction(i => 3.4)
                                                               .JoinAt("Rejoin"))
                                 .ContinueWithEbc<FlowOutputDependingOnInput, bool, string>(
                                     () => new FlowOutputDependingOnInput("On Branch", "On Main"))
                                 .JoinOrContinueWithEbc<ContinueFlow, string, int>("Rejoin")
                                 .Exit(),

//                             Flow.StartWith<InitialOutflow, string>("ScatterGather")
//                                 .ContinueWithEbc<StringSplitter, string, IEnumerable<char>>()
//                                 .Scatter<char, string>(configurer => configurer
//                                                                          .ContinueWithEbc<CharShifter, char, char>()
//                                                                          .GatherTo<StringFromChars, char, string>())
//                                 .Exit(),

                             Flow.StartWith<InitialOutflow, string>("ContinueWithNamedFlow")
                                 .ContinueWithNamedFlow<string, string>("SimpleFlow2")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .Exit(),

                             Flow.StartWith<InitialOutflow, string>("FailingFlow")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .ContinueWithEbc<FailingFlow, int, string>()
                                 .ContinueWithNamedFlow<string, string>("SimpleFlow2")
                                 .Exit(),

                             Flow.StartWith<InitialOutflow, string>("SagaFlow1")
                                 .WaitOnPort<string, string>("Port1")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .ContinueWithEbc<SomeOtherFlow, bool, string>()
                                 .Exit(),

                             Flow.StartWith<InitialOutflow, string>("GotoFlow1")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .JoinOrContinueWithFunction(i => ++i, "Goto")
					.BranchWhen(y => y < 12, configurer => configurer
                                                                            .WaitOnPort<int, int>("Port1")
                                                                            .JoinAt("Goto"))
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .Exit(),

                             Flow.StartWith<InitialOutflow, string>("GotoFlow2")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .JoinOrContinueWithFunction(i => ++i, "Goto")
                                 .BranchWhen(y => y < 10, configurer => configurer
                                                                            .WaitOnPort<int, int>("Port1")
                                                                            .JoinAt("Gotoe"))
                                 .ContinueWithEbc<CheckFlow, int, bool>()
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("StackOverflowFlow")
                                 .JoinOrContinueWithFunction(i => i, "Goto")
                                 .BranchWhen(i => i < 10, configurer =>
                                                          configurer
                                                              .JoinAt("Goto"))
                                 .ContinueWithEbc<TranslateNumberToLocaleMonth, int, string>()
                                 .ContinueWithEbc<DependingOnInterfaceFlow, string, string>()
                                 .Exit(),
                             Flow.StartWith<ContinueFlow, string, int>("DependencyInjectedFlow")
                                 .JoinOrContinueWithFunction(i => i, "Goto")
                                 .BranchWhen(i => i < 10, configurer =>
                                                          configurer
                                                              .ContinueWithFunction(i => ++i)
                                                              .JoinAt("Goto"))
                                 .ContinueWithEbc<TranslateNumberToLocaleMonth, int, string>()
                                 .ContinueWithEbc<DependingOnInterfaceFlow, string, string>()
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("GotoFlow3")
                                 .JoinOrContinueWithFunction(i => ++i, "Goto")
                                 .ContinueWithFunction(i =>
                                                           {
                                                               Debug.WriteLine(i.ToString());
                                                               return i;
                                                           })
                                 .GoToJoinpointIf("Goto", i => i < 10)
                                 .ContinueWithEbc<TranslateNumberToLocaleMonth, int, string>()
                                 .ContinueWithEbc<DependingOnInterfaceFlow, string, string>()
                                 .Exit(),

                             Flow.StartWith<ContinueFlow, string, int>("TriggerFlow")
                                 .JoinOrContinueWithFunction(i => ++i, "Goto")
                                 .Trigger("Trigger", i => i.ToString())
                                 .GoToJoinpointIf("Goto", i => i < 10)
                                 .ContinueWithEbc<TranslateNumberToLocaleMonth, int, string>()
                                 .ContinueWithEbc<DependingOnInterfaceFlow, string, string>()
                                 .Exit(),

                                 Flow.StartWith<InitialOutflow, string>("JoinAtTriggerFlow")
                                 .ContinueWithEbc<ContinueFlow, string, int>()
                                 .JoinAtTrigger(i => i.ToString(), "Goto", "Trigger")
                                 .ContinueWithFunction(i => ++i)
                                 .BranchWhen(y => y < 10, configurer => configurer
                                                                            .ContinueWithFunction(i => i)
                                                                            .JoinAt("Goto"))
                                 .ContinueWithFunction(i => i)
                                 .Exit(),

                                 Flow.StartWith<InitialOutflow, string>("ScatterGather")
                                 .ContinueWithEbc<StringSplitter, string, IEnumerable<char>>()
                                 .ScatterTo<IEnumerable<char>, char, char, IEnumerable<char>>(configurer => configurer
                                    .ContinueWithEbc<CharShifter, char, char>()
                                    .Gather())
                                .ContinueWithEbc<StringFromChars, IEnumerable<char>, string>()
                                .Exit(),

                                Flow.StartWith<InitialOutflow, string>("StrictFailingScatterGather")
                                 .ContinueWithEbc<StringSplitter, string, IEnumerable<char>>()
                                 .ScatterTo<IEnumerable<char>, char, char, IEnumerable<char>>(configurer => configurer
                                    .ContinueWithContinuation<FailingCharShifter, char, char>()
                                    .Gather())
                                .ContinueWithEbc<StringFromChars, IEnumerable<char>, string>()
                                .Exit(),

                                Flow.StartWith<InitialOutflow, string>("IndulgentFailingScatterGather")
                                 .ContinueWithEbc<StringSplitter, string, IEnumerable<char>>()
                                 .ScatterTo<IEnumerable<char>, char, char, IEnumerable<char>>(configurer => configurer
                                    .ContinueWithContinuation<FailingCharShifter, char, char>()
                                    .Gather(TreatExceptionsWhenGathering.ContinueFlow))
                                .ContinueWithEbc<StringFromChars, IEnumerable<char>, string>()
                                .Exit(),

                                Flow.StartWithFunc<string, string[]>("WordCountingSubflow", str => str.Split(' '))
                                .ContinueWithFunction(strings => strings.Length)
                                .Exit(),

                                Flow.StartWithFunc<string, string[]>("CharCountingSubflow", s => s.Split(' '))
                                    .ScatterTo<string[], string, int, int[]>(configurer => configurer
                                        .ContinueWithFunction(s => s.ToCharArray())
                                        .ContinueWithFunction(c => c.Length)
                                        .Gather())
                                    .ContinueWithContinuation<Aggregator, int[], int>()
                                    .Exit(),

                                Flow.StartWithFunc<string, string[]>("WordCountingFlow", s => s.Split('.'))
                                .ScatterTo<string[], string, int, int[]>(configurer => configurer
                                    .ContinueWithNamedFlow<string, int>("WordCountingSubflow")
                                    .Gather())
                                .ContinueWithContinuation<Aggregator, int[], int>()
                                .Exit(),

                                Flow.StartWithFunc<string, string[]>("CharCountingFlow", s => s.Split('.'))
                                .ScatterTo<string[], string, int, int[]>(configurer => configurer
                                    .ContinueWithNamedFlow<string, int>("CharCountingSubflow")
                                        .Gather())
                                .ContinueWithContinuation<Aggregator, int[], int>()
                                .Exit(),

                                Flow.StartWithFunc<string, string[]>("CharCountingFullFlow", s => s.Split('.'))
                                .ScatterTo<string[], string, int, int[]>(configurer => configurer
                                    .ContinueWithFunction(s => s.Split(' '))
                                    .ScatterTo<string[], string, int, int[]>(flowConfigurer => flowConfigurer
                                        .ContinueWithFunction(s => s.ToCharArray())
                                        .ContinueWithFunction(c => c.Length)
                                        .Gather())
                                    .ContinueWithContinuation<Aggregator, int[], int>()
                                    .Gather())
                                .ContinueWithContinuation<Aggregator, int[], int>()
                                .Exit(),

                                Flow.StartWithFunc<int[], int[]>("SimpleScatterGather", ints => ints)
                                .ScatterTo<int[], int, string, string[]>(configurer => configurer
                                    .ContinueWithFunction(i => i.ToString())
                                    .Gather())
                                .ContinueWithFunction(String.Concat)
                                .Exit(),

                                Flow.StartWithFunc<Tuple<int, string>, Tuple<int, string>>("StateStoringFlow", tuple => tuple)
                                .WriteState(tuple => tuple.Item1, tuple => tuple.Item2)
                                .ContinueWithFunction(s => s.Length)
                                .ReadState<int, int, Tuple<int, int>>((i, state) => new Tuple<int, int>(i, state))
                                .ContinueWithFunction(tuple => tuple.Item1 + tuple.Item2)
                                .Exit(),

                                Flow.StartWithFunc<int, int[]>("ScatteredStateStoringFlow", i => new[]{i, i, i, i})
                                .ScatterTo<int[], int, int, int[]>(configurer => configurer
                                    .WriteState(i => i)
                                    .ReadState<int, int, int>((i, state) => state)
                                    .ContinueWithFunction(i => ++i)
                                    .WriteState(i => i)
                                    .Gather())
                                .ContinueWithContinuation<Aggregator, int[], int>()
                                .Exit(),

                                Flow.StartWithFunc<string, IEnumerable<string>>("StringToIntConvertingFlow", str => str.Split(','))
                                .WriteState(enumerable => enumerable)
                                .ScatterTo<IEnumerable<string>, string, int, IEnumerable<int>>(configurer => configurer
                                    .ContinueWithFunction(Int32.Parse)
                                    .Gather())
                                .ReadState<IEnumerable<int>, IEnumerable<string>, IEnumerable<int>>((ints, strings) => ints)
                                .Exit(),

                                Flow.StartWithFunc<int, int>("TimeoutFlow", i => (i+3))
                                .ContinueWithContinuation<IntToStringContinuation, int, string>()
                                .ContinueWithContinuation<TimingOutContinuation, string, string>()
                                .Exit(),

                                Flow.StartWith<ContinueFlow, string, int>("SyncFunctionsFlow")
                                .ContinueWithFunction(Functions.IntToString)
                                //.ContinueWithFunction()
                                .Exit()
                         };

            _sut = new RuntimeEngine(_runtimeConfig, _flows);
            _sut.Start();
        }

        [Test]
        public void SimpleFlowWithTask()
        {
            var flowName = "SimpleFlow2";

            var task = _sut.RunFlow<string, string>("Hello World", flowName);

            if (!task.Wait(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within reasonable time.");
            }

            Assert.AreEqual("Hi from SimpleFlow!", task.Result);
        }

        [Test]
        public void TimeoutBugExposure()
        {
            var flowName = "TimeoutFlow";
            var input = 12;
            string result = null;

            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<int>(flowName);

            processor(input);

            if (!_mre.WaitOne(3.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(result);
                Assert.IsNotNull(exception);
            }

            _sut.Stop();
        }

        [Test]
        public void ReadStateOnHigherInheritanceLevel()
        {
            var flowName = "StringToIntConvertingFlow";
            var input = "12,13,14,15,16,17";
            var result = Enumerable.Empty<int>();
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<IEnumerable<int>>(flowName, ints =>
            {
                result = ints;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            processor(input);

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(6, result.Count());
            }

            _sut.Stop();
        }

        [Test]
        public void ScatteringWithNoItemsToScatter()
        {
            var flowName = "SimpleScatterGather";
            var result = "Hello";
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, s =>
            {
                result = s;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<int[]>(flowName);

            processor(new int[0]);

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(String.Empty, result);
            }

            _sut.Stop();
        }

        [Test]
        public void StoreIndividualStateInScatterFlows()
        {
            var flowName = "ScatteredStateStoringFlow";

            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<int>(flowName);

            processor(1);

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(8, result);
            }

            _sut.Stop();
        }

        [Test]
        public void SimpleStateStoringFlow()
        {
            var flowName = "StateStoringFlow";

            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<Tuple<int, string>>(flowName);

            processor(new Tuple<int, string>(10, flowName));

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual((flowName.Length + 10), result);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterGatherWithNoErrorDuringScatter()
        {
            var flowName = "ScatterGather";
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(5.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(9, result.Length);
                Assert.AreNotEqual("Scrambled", result);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterGatherWithErrorDuringScatterAndIndulgentGatherStrategy()
        {
            var flowName = "IndulgentFailingScatterGather";
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(4, result.Length);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterGatherWithErrorDuringScatterAndStrictGatherStrategy()
        {
            var flowName = "StrictFailingScatterGather";
            var result = String.Empty;
            Exception exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            }, ex =>
            {
                Debug.WriteLine("Expected exception!");
                exception = ex;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                Debug.WriteLine("Execution engine exception!");
                Debug.WriteLine(executionException.ToString());
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNotNull(exception);
                Assert.AreEqual(String.Empty, result);
                Assert.AreEqual(typeof(AggregateException), exception.GetType());
                ((AggregateException)exception).Handle(x =>
                {
                    Assert.AreEqual(typeof(ScatterFlowFailedException), x.GetType());
                    Assert.AreEqual(typeof(InvalidOperationException), x.InnerException.GetType());
                    return true;
                });
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterToSubflow()
        {
            var flowName = "WordCountingFlow";
            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            var input = "Aurea prima sata est aetas, quae vindice nullo, sponte sua, sine lege fidem rectumque colebat." +
                        "Poena metusque aberant, nec verba minantia fixo aere legebantur, nec supplex turba timebat iudicis ora sui, sed erant sine vindice tuti." +
                        "nondum caesa suis, peregrinum ut viseret orbem, montibus in liquidas pinus descenderat undas, nullaque mortales praeter sua litora norant;";

            processor(input);

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(55, result);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterToNestedNamedSubflowWithTask()
        {
            var flowName = "CharCountingFlow";

            var input = "Aurea prima sata est aetas, quae vindice nullo, sponte sua, sine lege fidem rectumque colebat." +
                "Poena metusque aberant, nec verba minantia fixo aere legebantur, nec supplex turba timebat iudicis ora sui, sed erant sine vindice tuti." +
                "nondum caesa suis, peregrinum ut viseret orbem, montibus in liquidas pinus descenderat undas, nullaque mortales praeter sua litora norant;";

            RuntimeEngineExecutionException exception = null;

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
            };

            var cts = new CancellationTokenSource();

            var task = _sut.RunFlow<string, int>(input, flowName, cts.Token);

            if (!task.Wait(TimeSpan.FromSeconds(2)))
                Assert.Fail("Failed to complete within reasonable time!");
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(314, task.Result);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterToNestedNamedSubflowWithCancelledTask()
        {
            var flowName = "CharCountingFlow";

            var input = "Aurea prima sata est aetas, quae vindice nullo, sponte sua, sine lege fidem rectumque colebat." +
                "Poena metusque aberant, nec verba minantia fixo aere legebantur, nec supplex turba timebat iudicis ora sui, sed erant sine vindice tuti." +
                "nondum caesa suis, peregrinum ut viseret orbem, montibus in liquidas pinus descenderat undas, nullaque mortales praeter sua litora norant;";

            RuntimeEngineExecutionException exception = null;

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
            };

            var cts = new CancellationTokenSource();

            var task = _sut.RunFlow<string, int>(input, flowName, cts.Token);

            cts.CancelAfter(10);

            try
            {
                task.Wait(1000);
                Assert.IsNull(exception);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
                Debug.WriteLine(ex.ToString());
                Assert.AreEqual(typeof(AggregateException), ex.GetType());
            }

            _sut.Stop();
        }

        [Test]
        public void TaskedScatterGatherWithErrorDuringScatterAndStrictGatherStrategy()
        {
            var flowName = "StrictFailingScatterGather";

            Exception runtimeException = null;

            _sut.ExecutionException += executionException =>
            {
                runtimeException = executionException;
            };


            try
            {
                var task = _sut.RunFlow<string>(flowName);
                task.Wait(TimeSpan.FromSeconds(4));
                Assert.Fail("No exception?!?!");
            }
            catch (AggregateException ex)
            {
                //ex.Handle(exception => { Assert.IsInstanceOf<RuntimeGatherException>(exception);
                //                           return true;
                //});

                Assert.IsNull(runtimeException);
                Assert.Pass();
            }

            //var task = _sut.RunFlow<string>(flowName);

            //var ex = Assert.Catch<AggregateException>(() => { task.Wait(TimeSpan.FromSeconds(2)); });
            //ex.Handle (e => 
            //{
            //    Assert.AreEqual(typeof(RuntimeGatherException), e.GetType());
            //    return true;
            //});
            //Assert.IsNull(runtimeException);

            _sut.Stop();
        }

        [Test]
        public void ScatterToNestedScatterFlow()
        {
            var flowName = "CharCountingFullFlow";

            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            var input = "Aurea prima sata est aetas, quae vindice nullo, sponte sua, sine lege fidem rectumque colebat." +
                "Poena metusque aberant, nec verba minantia fixo aere legebantur, nec supplex turba timebat iudicis ora sui, sed erant sine vindice tuti." +
                "nondum caesa suis, peregrinum ut viseret orbem, montibus in liquidas pinus descenderat undas, nullaque mortales praeter sua litora norant;";

            processor(input);

            if (!_mre.WaitOne(4.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(314, result);
            }

            _sut.Stop();
        }

        [Test]
        public void ScatterToNestedNamedSubflow()
        {
            var flowName = "CharCountingFlow";

            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            _sut.ExecutionException += executionException =>
            {
                exception = executionException;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            var input = "Aurea prima sata est aetas, quae vindice nullo, sponte sua, sine lege fidem rectumque colebat." +
                "Poena metusque aberant, nec verba minantia fixo aere legebantur, nec supplex turba timebat iudicis ora sui, sed erant sine vindice tuti." +
                "nondum caesa suis, peregrinum ut viseret orbem, montibus in liquidas pinus descenderat undas, nullaque mortales praeter sua litora norant;";

            var expected = System.Text.RegularExpressions.Regex.Replace(input, @"\s", "").Length - 2;//account for 2 '.' chars that are split away

            processor(input);

            if (!_mre.WaitOne(4.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(expected, result);
            }

            _sut.Stop();
        }

        [Test]
        public void JoinAtTriggerTest()
        {
            var flowName = "JoinAtTriggerFlow";
            _lastTriggerValue = String.Empty;
            var result = 0;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            _sut.ExecutionException += obj =>
            {
                exception = obj;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor(flowName);

            var trigger = _sut.CreateTrigger<string>("*", "Trigger");

            trigger.When += str =>
            {
                _lastTriggerValue = str;
                Debug.WriteLine("Current number: " + str);
            };

            processor();

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual(10, result);
                Assert.AreEqual("9", _lastTriggerValue);
            }

            _sut.Stop();
        }

        [Test]
        public void StarTriggerTest()
        {
            var flowName = "TriggerFlow";
            var counter = 0;
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += obj =>
            {
                exception = obj;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            var trigger = _sut.CreateTrigger<string>("*", "Trigger");

            trigger.When += str =>
            {
                counter += 1;
                Debug.WriteLine("Current number: " + str);
            };

            processor("Als");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual("Oktober", result);
                Assert.AreEqual(7, counter);
            }

            _sut.Stop();
        }

        [Test]
        public void TriggerFlowTest()
        {
            var flowName = "TriggerFlow";
            var counter = 0;
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            _sut.ExecutionException += obj =>
            {
                exception = obj;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            var trigger = _sut.CreateTrigger<string>(flowName, "Trigger");

            trigger.When += str =>
            {
                counter += 1;
                Debug.WriteLine("Current number: " + str);
            };

            processor("Als");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual("Oktober", result);
                Assert.AreEqual(7, counter);
            }

            _sut.Stop();
        }

        [Test]
        public void StandardGotoJoinpointFlow()
        {
            var flowName = "GotoFlow3";
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.ExecutionException += (obj) =>
            {
                exception = obj;
                _mre.Set();
            };

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Abc");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual("Oktober", result);
            }

            _sut.Stop();
        }

        [Test]
        public void AvoidStackOverflowsInConfiguration()
        {
            var flowName = "StackOverflowFlow";
            RuntimeEngineExecutionException exception = null;

            _sut.ExecutionException += obj =>
            {
                exception = obj;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hallo");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNotNull(exception);
                Assert.AreEqual(typeof(BuildFlowStackException), exception.InnerException.GetType());
            }

            _sut.Stop();
        }

        [Test]
        public void DependencyInjectionViaRuntimeConfiguration()
        {
            var flowName = "DependencyInjectedFlow";
            var result = String.Empty;
            RuntimeEngineExecutionException exception = null;

            _sut.ExecutionException += obj =>
            {
                exception = obj;
                _mre.Set();
            };

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hallo");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.AreEqual("Oktober", result);
            }

            _sut.Stop();
        }

        [Test]
        public void ThrowExceptionWhenBuildingFlowStackFails()
        {
            var flowName = "GotoFlow2";
            RuntimeEngineExecutionException result = null;

            _sut.WaitForResult<int>(flowName, i => { });

            var processor = _sut.CreateProcessor(flowName);

            _sut.ExecutionException += exception =>
            {
                result = exception;
                _mre.Set();
            };

            processor();

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(typeof(BuildFlowStackException), result.InnerException.GetType());
            }

            _sut.Stop();
        }

        [Test]
        public void CheckIfBranchAndJoinCanWorkAsGoto()
        {
            var flowName = "GotoFlow1";
            var counter = 0;
            var result = false;

            _sut.WaitForResult<bool>(flowName, b =>
            {
                result = b;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor(flowName);

            var port = _sut.CreatePort<int, int>(flowName, "Port1");

            port.AssignActor((i, action) =>
            {
                counter++;
                action(i);
            });

            processor();

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsFalse(result);
                Assert.AreEqual(2, counter);
            }

            _sut.Stop();
        }

        [Test]
        public void WhenABranchFlowDoesNotReturnTheRequiredResultType()
        {
            var flowName = "InvalidBranchFlow";
            RuntimeEngineExecutionException result = null;

            _sut.WaitForResult<int>(flowName, i => { });

            var processor = _sut.CreateProcessor<string>(flowName);

            _sut.ExecutionException += exception =>
            {
                result = exception;
                _mre.Set();
            };

            processor("Hello");

            if (!_mre.WaitOne(2.Seconds()))
            {
                Assert.Fail("Failed to complete within reasonable time!");
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(typeof(InputTypeNotMatchingOutputTypeException), result.InnerException.GetType());
            }

            _sut.Stop();
        }

        [Test]
        public void TestSagaFlow1()
        {
            var flowName = "SagaFlow1";
            int counter = 0;

            _sut.WaitForResult<string>(flowName, str => _mre.Set());

            var port = _sut.CreatePort<string, string>(flowName, "Port1");

            port.AssignActor((s, action) =>
            {
                counter++;
                action("This is my action!");
            });

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.AreNotEqual(0, counter);
            }

            port.Dispose();
            _sut.Stop();
        }

        [Test]
        public void TestFunctionFlow()
        {
            var flowName = "FunctionFlow1";
            int result = 0;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                Debug.Write(Thread.CurrentThread.ManagedThreadId);
                _mre.Set();
            });

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.AreEqual(1, result);
            }

            _sut.Stop();
        }

        [Test]
        public void TestContinuationFlow()
        {
            var flowName = "ContinuationFlow1";
            int result = 0;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                Debug.Write(Thread.CurrentThread.ManagedThreadId);
                _mre.Set();
            });

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.AreEqual(1, result);
            }

            _sut.Stop();
        }

        [Test]
        public void TestFailingFlow()
        {
            FlowFailedMessage result = null;

            _sut.RegisterObserver<FlowFailedMessage>(msg =>
            {
                result = msg;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor("FailingFlow");

            processor();


            if (!_mre.WaitOne(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual("FailingFlow", result.RunId.ToString());
                Assert.AreEqual(typeof(InvalidOperationException), result.Exception.GetType());
            }

            _sut.Stop();
        }

        [Test]
        public void RunFlowWithSubflow()
        {
            var flowName = "ContinueWithNamedFlow";
            Debug.Write(Thread.CurrentThread.ManagedThreadId);

            bool? result = null;

            _sut.WaitForResult<bool>(flowName, b =>
            {
                result = b;
                Debug.Write(Thread.CurrentThread.ManagedThreadId);
                _mre.Set();
            });

            Exception exception = null;

            _sut.ExecutionException += ex =>
            {
                exception = ex;
                _mre.Set();
            };

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsNull(exception);
                Assert.IsTrue(result.HasValue);
                Assert.IsTrue(result.Value);
            }

            _sut.Stop();
        }

        //        [Test]
        //        public void ScatterGatherFlow()
        //        {
        //            var processor = _sut.CreateProcessor("ScatterGather");
        //
        //            processor();
        //
        //            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
        //            {
        //                Assert.Fail("Failed to complete within 2 seconds");
        //            }
        //            else
        //            {
        //                Assert.IsNotNull(_result);
        //                Assert.AreEqual("Strizzi", _result);
        //            }
        //        }

        [Test]
        public void ExecuteBranchFlow2OnBranch()
        {
            var flowName = "BranchFlow2";
            int result = 0;
            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hello");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.AreEqual(9, result);
            }

            _sut.Stop();
        }

        [Test]
        public void ExecuteBranchFlow2OnMain()
        {
            var flowName = "BranchFlow2";

            int result = 0;

            _sut.WaitForResult<int>(flowName, i =>
            {
                result = i;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hell");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.AreEqual(7, result);
            }

            _sut.Stop();
        }

        [Test]
        public void BuildBranchFlow1()
        {
            var flowName = "BranchFlow1";
            string result = String.Empty;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hello");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsFalse(String.IsNullOrEmpty(result));
                Assert.AreEqual("On Main", result);
            }

            _sut.Stop();
        }

        [Test]
        public void RunIntoBranchOfBranchFlow1()
        {
            var flowName = "BranchFlow1";
            string result = String.Empty;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hell");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsFalse(String.IsNullOrEmpty(result));
                Assert.AreEqual("On Branch", result);
            }

            _sut.Stop();
        }

        [Test]
        public void ThrowExceptionIfFlowNotFound()
        {
            Assert.Throws<FlowNotFoundException>(() => _sut.CreateProcessor("UnknownFlow"));
        }

        [Test]
        public void BuildSimpleFlow1()
        {
            var flowName = "SimpleFlow1";
            var result = String.Empty;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor(flowName);

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsFalse(String.IsNullOrEmpty(result));
                Assert.AreEqual("Hi from SimpleFlow!", result);
            }
        }

        [Test]
        public void BuildSimpleFlow2()
        {
            var flowName = "SimpleFlow2";
            var result = String.Empty;

            _sut.WaitForResult<string>(flowName, str =>
            {
                result = str;
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>(flowName);

            processor("Hello");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Failed to complete within 2 seconds");
            }
            else
            {
                Assert.IsFalse(String.IsNullOrEmpty(result));
                Assert.AreEqual("Hi from SimpleFlow!", result);
            }
        }

        [Test]
        public void MyRuntimeEngineTest1()
        {
            //            var re = new MyRuntimeEngine(new RuntimeConfiguration(), _flows);
            //            re.Start();

            _sut.RegisterObserver<FlowFailedMessage>(evt =>
            {
                Debug.WriteLine("Observing a failed flow with name: " + evt.RunId + " reason: " + evt.Exception.ToString());
                _mre.Set();
            });

            _sut.RegisterObserver<FlowCompleteMessage>(evt =>
            {
                Debug.WriteLine("Observing a completed flow with name: " + evt.RunId + " result: " + evt.Result.Value);
                _mre.Set();
            });

            var processor = _sut.CreateProcessor("SimpleFlow1");

            processor();

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Test did not complete within reasonable time!");
            }
            else
            {
                Assert.Pass();
            }

            _sut.Stop();
        }

        [Test]
        public void MyRuntimeEngineTest2()
        {
            _sut.RegisterObserver<FlowFailedMessage>(evt =>
            {
                Debug.WriteLine("Observing a failed flow with name: " + evt.RunId + " reason: " + evt.Exception.ToString());
                _mre.Set();
            });

            _sut.RegisterObserver<FlowCompleteMessage>(evt =>
            {
                Debug.WriteLine("Observing a completed flow with name: " + evt.RunId + " result: " + evt.Result.Value);
                _mre.Set();
            });

            var processor = _sut.CreateProcessor<string>("SimpleFlow2");

            processor("Hello World");

            if (!_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                Assert.Fail("Test did not complete within reasonable time!");
            }
            else
            {
                Assert.Pass();
            }

            _sut.Stop();
        }

        [Test]
        public void ReflectiveEventWiring()
        {
            var ebc = new ContinueFlow();
            var bc = new BoxingContinuation<int>();
            bc.AttachContinuation(x => _mre.Set());
            var input = (object)"Hello World";

            var evt = ebc.GetType().GetEvent("Result");

            var invoker = Delegate.CreateDelegate(evt.EventHandlerType, bc, bc.GetType().GetMethod("Process"));

            try
            {
                evt.AddEventHandler(ebc, invoker);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            _mre.Reset();

            ebc.GetType().GetMethod("Process").Invoke(ebc, BindingFlags.InvokeMethod, null, new[] { input }, CultureInfo.InvariantCulture);

            if (_mre.WaitOne(TimeSpan.FromSeconds(2)))
            {
                evt.RemoveEventHandler(ebc, invoker);
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
    }

    public interface ICompletionEvent
    {
        event Action Completed;
    }

    public sealed class ResultStore<T> : ICompletionEvent
    {
        private T _result;

        public ResultStore()
        {
            _result = default(T);
            Completed += () => { };
        }

        public void SetResult(T result)
        {
            _result = result;
            Completed();
        }

        public T Result { get { return _result; } }

        #region ICompletionEvent implementation

        public event Action Completed;

        #endregion
    }

    public sealed class CompletionListener : ICompletionEvent
    {
        public CompletionListener()
        {
            Completed += () => { };
        }

        public void SetComplete()
        {
            Completed();
        }

        #region ICompletionEvent implementation
        public event Action Completed;
        #endregion
    }


}