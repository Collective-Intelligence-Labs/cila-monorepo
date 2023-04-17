using Microsoft.Extensions.DependencyInjection;

namespace Cila
{
    public interface IServiceLocator
    {
        T GetService<T>();
        object GetService(Type t);
    }

    public class ServiceLocator : IServiceLocator
    {
        private readonly ServiceProvider _serviceProvider;

        public ServiceLocator(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type t)
        {
            return _serviceProvider.GetService(t);
        }

        public T GetService<T>() => _serviceProvider.GetService<T>();
    }
}