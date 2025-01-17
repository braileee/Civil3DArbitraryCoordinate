using Autofac;
using Civil3DArbitraryCoordinate.ViewModels;
using Civil3DArbitraryCoordinate.Views;
using Prism.Events;

namespace Civil3DArbitraryCoordinate.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MainViewViewModel>().AsSelf();
            builder.RegisterType<MainView>().AsSelf();

            return builder.Build();
        }
    }
}
