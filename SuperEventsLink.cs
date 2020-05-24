//============================================================
// Project: DigitalFactory
// Author: Zoranner@ZORANNER
// Datetime: 2019-05-10 11:08:26
// Description: TODO >> This is a script Description.
//============================================================

using System;

namespace Zoranner.SuperEvents
{
    #region Interfaces

    public interface ISuperEventLinkBase<in TDelegate> where TDelegate : class
    {
        /// <summary>
        /// How many persistent listeners does this instance currently have?
        /// </summary>
        uint ListenerCount { get; }

        /// <summary>
        /// How many one-time listeners does this instance currently have?
        /// After dispatch, all current one-time listeners are automatically removed.
        /// </summary>
        uint OneTimeListenersCount { get; }

        /// <summary>
        /// Is this delegate already a persistent listener?
        /// Does NOT query one-time listeners.
        /// </summary>
        /// <param name="listener">Listener.</param>
        bool Contains(TDelegate listener);

        /// <summary>
        /// Adds a persistent listener.
        /// </summary>
        /// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        bool AddListener(TDelegate listener, bool allowDuplicates = false);

        /// <summary>
        /// Adds listener and creates a SuperEventBinding between the listener and the SuperEvent.
        /// The SuperEventBinding can be used to enable/disable the listener.
        /// </summary>
        /// <returns>A new SuperEventBinding instance if successful, <c>null</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        ISuperEventBinding BindListener(TDelegate listener, bool allowDuplicates = false);

        /// <summary>
        /// Adds a one-time listener.
        /// These listeners are removed after one Dispatch.
        /// </summary>
        /// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
        /// <param name="listener">Listener.</param>
        /// ///
        /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
        bool AddOnce(TDelegate listener, bool allowDuplicates = false);

        /// <summary>
        /// Removes a persistent listener, if present.
        /// </summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        bool RemoveListener(TDelegate listener);

        /// <summary>
        /// Removes a listener added with AddOnce, if present.
        /// </summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        bool RemoveOnce(TDelegate listener);

        /// <summary>
        /// Removes all listeners.
        /// </summary>
        /// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
        /// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
        void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = true);
    }

    public interface ISuperEventLink : ISuperEventLinkBase<Action>
    {
    }

    public interface ISuperEventLink<out T0> : ISuperEventLinkBase<Action<T0>>
    {
    }

    public interface ISuperEventLink<out T0, out T1> : ISuperEventLinkBase<Action<T0, T1>>
    {
    }

    public interface ISuperEventLink<out T0, out T1, out T2> : ISuperEventLinkBase<Action<T0, T1, T2>>
    {
    }

    public interface ISuperEventLink<out T0, out T1, out T2, out T3> : ISuperEventLinkBase<Action<T0, T1, T2, T3>>
    {
    }

    #endregion

    #region Implementation

    public abstract class SuperEventLinkBase<TDelegate> : ISuperEventLinkBase<TDelegate> where TDelegate : class
    {
        private readonly SuperEventBase<TDelegate> _SuperEvent;

        #region Constructors

        private SuperEventLinkBase()
        {
        } // Private empty constructor to force use of params

        protected SuperEventLinkBase(SuperEventBase<TDelegate> relay)
        {
            _SuperEvent = relay;
        }

        #endregion

        #region ISuperEventLinkBase implementation

        public uint ListenerCount => _SuperEvent.ListenerCount;

        public uint OneTimeListenersCount => _SuperEvent.OneTimeListenersCount;

        /// <inheritdoc>
        /// <summary>
        /// Is this delegate already a persistent listener?
        /// Does NOT query one-time listeners.
        /// </summary>
        /// <param name="listener">Listener.</param>
        /// </inheritdoc>
        public bool Contains(TDelegate listener)
        {
            return _SuperEvent.Contains(listener);
        }

        /// <inheritdoc>
        /// <summary>Adds a persistent listener.</summary>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        /// </inheritdoc>
        public bool AddListener(TDelegate listener, bool allowDuplicates = false)
        {
            return _SuperEvent.AddListener(listener, allowDuplicates);
        }

        /// <inheritdoc>
        /// <summary>
        /// Adds listener and creates a SuperEventBinding between the listener and the SuperEvent.
        /// The SuperEventBinding can be used to enable/disable the listener.
        /// </summary>
        /// <returns>A new SuperEventBinding instance if successful, <c>null</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        /// </inheritdoc>
        public ISuperEventBinding BindListener(TDelegate listener, bool allowDuplicates = false)
        {
            return _SuperEvent.BindListener(listener, allowDuplicates);
        }

        /// <inheritdoc>
        /// <summary>
        /// Adds a one-time listener.
        /// These listeners are removed after one Dispatch.
        /// </summary>
        /// <param name="listener">Listener.</param>
        /// ///
        /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
        /// </inheritdoc>
        public bool AddOnce(TDelegate listener, bool allowDuplicates = false)
        {
            return _SuperEvent.AddOnce(listener, allowDuplicates);
        }

        /// <inheritdoc>
        /// <summary>Removes a persistent listener, if present.</summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        /// </inheritdoc>
        public bool RemoveListener(TDelegate listener)
        {
            return _SuperEvent.RemoveListener(listener);
        }

        /// <inheritdoc>
        /// <summary>Removes a listener added with AddOnce, if present.</summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        /// </inheritdoc>
        public bool RemoveOnce(TDelegate listener)
        {
            return _SuperEvent.RemoveOnce(listener);
        }

        /// <inheritdoc>
        /// <summary>Removes all listeners.</summary>
        /// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
        /// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
        /// </inheritdoc>
        public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = true)
        {
            _SuperEvent.RemoveAll(removePersistentListeners, removeOneTimeListeners);
        }

        #endregion
    }

    public class SuperEventLink : SuperEventLinkBase<Action>, ISuperEventLink
    {
        public SuperEventLink(SuperEventBase<Action> relay) : base(relay)
        {
        }
    }

    public class SuperEventLink<T0> : SuperEventLinkBase<Action<T0>>, ISuperEventLink<T0>
    {
        public SuperEventLink(SuperEventBase<Action<T0>> relay) : base(relay)
        {
        }
    }

    public class SuperEventLink<T0, T1> : SuperEventLinkBase<Action<T0, T1>>, ISuperEventLink<T0, T1>
    {
        public SuperEventLink(SuperEventBase<Action<T0, T1>> relay) : base(relay)
        {
        }
    }

    public class SuperEventLink<T0, T1, T2> : SuperEventLinkBase<Action<T0, T1, T2>>, ISuperEventLink<T0, T1, T2>
    {
        public SuperEventLink(SuperEventBase<Action<T0, T1, T2>> relay) : base(relay)
        {
        }
    }

    public class SuperEventLink<T0, T1, T2, T3> : SuperEventLinkBase<Action<T0, T1, T2, T3>>,
        ISuperEventLink<T0, T1, T2, T3>
    {
        public SuperEventLink(SuperEventBase<Action<T0, T1, T2, T3>> relay) : base(relay)
        {
        }
    }

    #endregion
}