using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace MvvmScarletToolkit
{
#pragma warning disable S125 // Sections of code should not be "commented out"
    // Josh Smiths PropertyObserver https://joshsmithonwpf.wordpress.com/2009/07/11/one-way-to-avoid-messy-propertychanged-event-handling/

    //* Sample:
    //*

    //* PropertyObserver<T> _observer;
    //*
    //* public T()
    //* {
    //* _observer = new PropertyObserver<T>(this T)
    //*      .RegisterHandler(n => n.Property1, n => someAction)
    //*      .RegisterHandler(n => n.Property2, n => anotherAction)
    //*
#pragma warning restore S125 // Sections of code should not be "commented out"

    /// <summary>
    /// Monitors the PropertyChanged event of an object that implements INotifyPropertyChanged,
    /// and executes callback methods (i.e. handlers) registered for properties of that object.
    /// </summary>
    /// <typeparam name="TPropertySource">The type of object to monitor for property changes.</typeparam>
    public class PropertyObserver<TPropertySource> : IWeakEventListener where TPropertySource : INotifyPropertyChanged
    {
        private readonly Dictionary<string, Action<TPropertySource>> _propertyNameToHandlerMap;
        private readonly WeakReference _propertySourceRef;

        /// <summary>
        /// Initializes a new instance of PropertyObserver, which
        /// observes the 'propertySource' object for property changes.
        /// </summary>
        /// <param name="propertySource">The object to monitor for property changes.</param>
        public PropertyObserver(TPropertySource propertySource)
        {
            if (propertySource == null)
                throw new ArgumentNullException(nameof(propertySource));

            _propertySourceRef = new WeakReference(propertySource);
            _propertyNameToHandlerMap = new Dictionary<string, Action<TPropertySource>>();
        }

        /// <summary>
        /// Registers a callback to be invoked when the PropertyChanged event has been raised for the specified property.
        /// </summary>
        /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
        /// <param name="handler">The callback to invoke when the property has changed.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> RegisterHandler(Expression<Func<TPropertySource, object>> expression, Action<TPropertySource> handler)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            string propertyName = GetPropertyName(expression);
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("'expression' did not provide a property name.");

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            TPropertySource propertySource = GetPropertySource();
            if (propertySource != null)
            {
                _propertyNameToHandlerMap[propertyName] = handler;
                PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
            }

            return this;
        }

        /// <summary>
        /// Removes the callback associated with the specified property.
        /// </summary>
        /// <param name="propertyName">A lambda expression like 'n => n.PropertyName'.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "can't be null");

            string propertyName = GetPropertyName(expression);
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("'expression' did not provide a property name.");

            TPropertySource propertySource = GetPropertySource();
            if (propertySource != null && _propertyNameToHandlerMap.ContainsKey(propertyName))
            {
                _propertyNameToHandlerMap.Remove(propertyName);
                PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);

            }

            return this;
        }

        private string GetPropertyName(Expression<Func<TPropertySource, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
                memberExpression = lambda.Body as MemberExpression;

            Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;

                return propertyInfo.Name;
            }

            return null;
        }

        private TPropertySource GetPropertySource()
        {
            try
            {
                return (TPropertySource)_propertySourceRef.Target;
            }
            catch
            {
                return default;
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                var propertyName = ((PropertyChangedEventArgs)e).PropertyName;
                var propertySource = (TPropertySource)sender;

                if (string.IsNullOrEmpty(propertyName))
                {
                    // When the property name is empty, all properties are considered to be invalidated.
                    // Iterate over a copy of the list of handlers, in case a handler is registered by a callback.
                    foreach (var handler in _propertyNameToHandlerMap.Values.ToArray())
                        handler(propertySource);

                    return true;
                }
                else
                {
                    if (_propertyNameToHandlerMap.TryGetValue(propertyName, out Action<TPropertySource> handler))
                    {
                        handler(propertySource);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
