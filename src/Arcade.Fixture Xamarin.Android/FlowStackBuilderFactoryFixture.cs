using Arcade.Build.FlowStack;
using Arcade.Build.RunVectors;
using Arcade.Dsl;
using Arcade.Engine;
using Arcade.Tests;
using NUnit.Framework;

namespace Windows.Tests.Fixture
{
    [TestFixture]
    public class FlowStackBuilderFactoryFixture
    {
        private FlowConfiguration[] _flows;
        private RuntimeConfiguration _runtimeConfig;
        private IFlowStackBuilderFactory _sut;

        [SetUp]
        public void Setup()
        {
            _runtimeConfig = new RuntimeConfiguration();

            _flows = new[]
            {
                Flow.StartWith<InitialOutflow, string>("SimpleFlow1")
                .ContinueWithEbc<ContinueFlow, string, int>(()=>new ContinueFlow())
                .ContinueWithEbc<CheckFlow, int, bool>(()=> new CheckFlow())
                .ContinueWithEbc<SomeOtherFlow, bool, string>(()=>new SomeOtherFlow())
                .Exit(),
                
                Flow.StartWith<ContinueFlow, string, int>("SimpleFlow2")
                .ContinueWithEbc<CheckFlow, int, bool>()
                .ContinueWithEbc<SomeOtherFlow, bool, string>()
                .Exit()
            };

            _sut = new FlowStackBuilderFactory(new InstanceFactory(_runtimeConfig), _flows, 5.Seconds());
        }

        [Test]
        public void UseSimpleFlowStackBuilderTheFirstTime()
        {
            var stackBuilder = _sut.CreateFlowStackBuilder("SimpleFlow1");
            Assert.AreEqual(typeof(SimpleFlowStackBuilder), stackBuilder.GetType());
        }

        [Test]
        public void UseCacheFlowStackBuilderIfStackHasAlreadyBeenBuilt()
        {
            var stackBuilder = _sut.CreateFlowStackBuilder("SimpleFlow1");
            var stack = stackBuilder.BuildUpFlowStack();
            stackBuilder = _sut.CreateFlowStackBuilder("SimpleFlow1");
            Assert.AreEqual(typeof(CachedFlowStackBuilder), stackBuilder.GetType());
        }

        [Test]
        public void NotExistingFlowReturnsFalse()
        {
            Assert.IsFalse(_sut.CanCreateFlowStackBuilderForFlowName("UnknownFlow"));
        }
    }

}