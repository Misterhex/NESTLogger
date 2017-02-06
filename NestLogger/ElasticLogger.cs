using System;

namespace NESTLogger
{
    internal class ElasticLogger<T> : IElasticLogger<T> where T : ElasticModelBase
    {
        private readonly IObserver<T> _observer;

        internal ElasticLogger(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException("observer");

            _observer = observer;
        }

        public void Log(params T[] values)
        {
            if (values == null) return;

            foreach (var v in values)
                _observer.OnNext(v);
        }
    }
}