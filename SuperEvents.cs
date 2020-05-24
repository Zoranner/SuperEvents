//============================================================
// Project: DigitalFactory
// Author: Zoranner@ZORANNER
// Datetime: 2019-05-10 11:08:26
// Description: TODO >> This is a script Description.
//============================================================

using System;
using System.Collections.Generic;

namespace Zoranner.SuperEvents
{
    [Serializable]
    public abstract class SuperEventBase<TDelegate> : ISuperEventLinkBase<TDelegate> where TDelegate : class
    {
        /// <inheritdoc />
        /// <summary>
        /// How many persistent listeners does this instance currently have?
        /// </summary>
        public uint ListenerCount => _Count;

        /// <inheritdoc />
        /// <summary>
        /// How many one-time listeners does this instance currently have?
        /// After dispatch, all current one-time listeners are automatically removed.
        /// </summary>
        public uint OneTimeListenersCount => _OnceCount;

        protected bool _HasLink;

        protected TDelegate[] _Listeners = new TDelegate[1];
        protected uint _Count;
        protected uint _Cap = 1;

        protected TDelegate[] _ListenersOnce;
        protected uint _OnceCount;
        protected uint _OnceCap;

        protected IndexOutOfRangeException _IndexOutOfRangeException = new IndexOutOfRangeException(
            "Fewer listeners than expected. See guidelines in SuperEvent.cs on using RemoveListener and RemoveAll within SuperEvent listeners.");

#if SIGTRAP_RELAY_DBG
        /// <summary>
        /// If true, SuperEventDebugger will automatically record all listener addition and removal on all SuperEvents.
        /// This allows a dump of all SuperEvent data to aid diagnosis of lapsed listeners etc.
        /// </summary>
        public static bool recordDebugData
        {
            get { return _SuperEventDebugger.recordDebugData; }
            set { _SuperEventDebugger.recordDebugData = value; }
        }
        /// <summary>
        /// Output a log of all existing SuperEvents and their listeners.
        /// </summary>
        /// <returns>The listeners.</returns>
        public static string LogSuperEvents ()
        {
            return _SuperEventDebugger.LogSuperEvents ();
        }
        /// <summary>
        /// Output a log of any and all SuperEvents specified object is currently subscribed to.
        /// </summary>
        /// <returns>The listeners.</returns>
        /// <param name="observer">Owner of listeners.</param>
        public static string LogSuperEvents (object observer)
        {
            return _SuperEventDebugger.LogSuperEvents (observer);
        }
#endif

        #region API

        /// <summary>
        /// Is this delegate already a persistent listener?
        /// Does NOT query one-time listeners.
        /// </summary>
        /// <param name="listener">Listener.</param>
        public bool Contains(TDelegate listener)
        {
            return Contains(_Listeners, _Count, listener);
        }

        /// <summary>
        /// Adds a persistent listener.
        /// </summary>
        /// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        public bool AddListener(TDelegate listener, bool allowDuplicates = false)
        {
            if (!allowDuplicates && Contains(listener))
            {
                return false;
            }

            if (_Count == _Cap)
            {
                _Cap *= 2;
                _Listeners = Expand(_Listeners, _Cap, _Count);
            }

            _Listeners[_Count] = listener;
            ++_Count;
#if SIGTRAP_RELAY_DBG
            _SuperEventDebugger.DebugAddListener (this, listener);
#endif

            return true;
        }

        /// <summary>
        /// Adds listener and creates a SuperEventBinding between the listener and the SuperEvent.
        /// The SuperEventBinding can be used to enable/disable the listener.
        /// </summary>
        /// <returns>A new SuperEventBinding instance if successful, <c>null</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
        public ISuperEventBinding BindListener(TDelegate listener, bool allowDuplicates = false)
        {
            return AddListener(listener, allowDuplicates)
                ? new SuperEventBinding<TDelegate>(this, listener, allowDuplicates, true)
                : null;
        }

        /// <summary>
        /// Adds a one-time listener.
        /// These listeners are removed after one Dispatch.
        /// </summary>
        /// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
        /// <param name="listener">Listener.</param>
        /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
        public bool AddOnce(TDelegate listener, bool allowDuplicates = false)
        {
            if (!allowDuplicates && Contains(_ListenersOnce, _OnceCount, listener))
            {
                return false;
            }

            if (_OnceCount == _OnceCap)
            {
                if (_OnceCap == 0)
                {
                    _OnceCap = 1;
                }
                else
                {
                    _OnceCap *= 2;
                }

                _ListenersOnce = Expand(_ListenersOnce, _OnceCap, _OnceCount);
            }

            _ListenersOnce[_OnceCount] = listener;
            ++_OnceCount;
#if SIGTRAP_RELAY_DBG
            _SuperEventDebugger.DebugAddListener (this, listener);
#endif
            return true;
        }

        /// <summary>
        /// Removes a persistent listener, if present.
        /// </summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        public bool RemoveListener(TDelegate listener)
        {
            var result = false;
            for (uint i = 0; i < _Count; ++i)
            {
                if (!_Listeners[i].Equals(listener))
                {
                    continue;
                }

                RemoveAt(i);
                result = true;
                break;
            }
#if SIGTRAP_RELAY_DBG
            if (result) _SuperEventDebugger.DebugRemListener (this, listener);
#endif
            return result;
        }

        /// <summary>
        /// Removes a one-time listener, if present.
        /// </summary>
        /// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
        /// <param name="listener">Listener.</param>
        public bool RemoveOnce(TDelegate listener)
        {
            var result = false;
            for (uint i = 0; i < _OnceCount; ++i)
            {
                if (!_ListenersOnce[i].Equals(listener))
                {
                    continue;
                }

                RemoveOnceAt(i);
                result = true;
                break;
            }
#if SIGTRAP_RELAY_DBG
            if (result) _SuperEventDebugger.DebugRemListener (this, listener);
#endif
            return result;
        }

        #endregion

        /// <summary>
        /// Removes all listeners.
        /// </summary>
        /// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
        /// <param name="removeOneTimeListeners">If set to <c>true</c>, remove one-time listeners.</param>
        public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = true)
        {
            if (removePersistentListeners)
            {
                // No count check since array always present and RemoveAll
                // expected to be used when user knows there are listeners
#if SIGTRAP_RELAY_DBG
                for (int i = 0; i < _listeners.Length; ++i)
                {
                    _SuperEventDebugger.DebugRemListener (this, _listeners[i]);
                }
#endif
                Array.Clear(_Listeners, 0, (int) _Cap);
                _Count = 0;
            }

            if (removeOneTimeListeners && _OnceCount > 0)
            {
                // Count check because array lazily instantiated
                Array.Clear(_ListenersOnce, 0, (int) _OnceCap);
                _OnceCount = 0;
            }
        }

        #region Internal

        protected void RemoveAt(uint i)
        {
            _Count = RemoveAt(_Listeners, _Count, i);
        }

        protected void RemoveOnceAt(uint i)
        {
            _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i);
        }

        protected uint RemoveAt(TDelegate[] arr, uint count, uint i)
        {
            --count;
            for (var j = i; j < count; ++j)
            {
                arr[j] = arr[j + 1];
            }

            arr[count] = null;
            return count;
        }

        private bool Contains(TDelegate[] arr, uint c, TDelegate d)
        {
            for (uint i = 0; i < c; ++i)
            {
                if (arr[i].Equals(d))
                {
                    return true;
                }
            }

            return false;
        }

        private TDelegate[] Expand(IReadOnlyList<TDelegate> arr, uint cap, uint count)
        {
            var newArr = new TDelegate[cap];
            for (var i = 0; i < count; ++i)
            {
                newArr[i] = arr[i];
            }

            return newArr;
        }

        #endregion
    }

    #region Implementations

    public class SuperEvent : SuperEventBase<Action>, ISuperEventLink
    {
        private ISuperEventLink _Link;

        /// <summary>
        /// Get an ISuperEventLink object that wraps this SuperEvent without allowing Dispatch.
        /// Provides a safe interface for classes outside the SuperEvent's "owner".
        /// </summary>
        public ISuperEventLink Link
        {
            get
            {
                if (_HasLink)
                {
                    return _Link;
                }

                _Link = new SuperEventLink(this);
                _HasLink = true;

                return _Link;
            }
        }

        public void Dispatch()
        {
            // Persistent listeners
            // Reversal allows self-removal during dispatch (doesn't0 skip next listener)
            // Reversal allows safe addition during dispatch (doesn't0 fire immediately)
            for (var i = _Count; i > 0; --i)
            {
                if (i > _Count)
                {
                    throw _IndexOutOfRangeException;
                }

                if (_Listeners[i - 1] != null)
                {
                    _Listeners[i - 1]();
                }
                else
                {
                    RemoveAt(i - 1);
                }
            }

            // One-time listeners - reversed for safe addition and auto-removal
            for (var i = _OnceCount; i > 0; --i)
            {
                if (_ListenersOnce[i - 1] == null)
                {
                    continue;
                }

                var listener = _ListenersOnce[i - 1];
                listener();
                // Check for self-removal before auto-removing
                if (_ListenersOnce[i - 1] == listener)
                {
#if SIGTRAP_RELAY_DBG
                    _SuperEventDebugger.DebugRemListener (this, _listenersOnce[i - 1]);
#endif
                    _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i - 1);
                }
            }
        }
    }

    public class SuperEvent<T0> : SuperEventBase<Action<T0>>, ISuperEventLink<T0>
    {
        private ISuperEventLink<T0> _Link;

        /// <summary>
        /// Get an ISuperEventLink object that wraps this SuperEvent without allowing Dispatch.
        /// Provides a safe interface for classes outside the SuperEvent's "owner".
        /// </summary>
        public ISuperEventLink<T0> Link
        {
            get
            {
                if (_HasLink)
                {
                    return _Link;
                }

                _Link = new SuperEventLink<T0>(this);
                _HasLink = true;

                return _Link;
            }
        }

        public void Dispatch(T0 t0)
        {
            for (var i = _Count; i > 0; --i)
            {
                if (i > _Count)
                {
                    throw _IndexOutOfRangeException;
                }

                if (_Listeners[i - 1] != null)
                {
                    _Listeners[i - 1](t0);
                }
                else
                {
                    RemoveAt(i - 1);
                }
            }

            for (var i = _OnceCount; i > 0; --i)
            {
                if (_ListenersOnce[i - 1] == null)
                {
                    continue;
                }

                var l = _ListenersOnce[i - 1];
                l(t0);
                if (_ListenersOnce[i - 1] == l)
                {
#if SIGTRAP_RELAY_DBG
                    _SuperEventDebugger.DebugRemListener (this, _listenersOnce[i - 1]);
#endif
                    _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i - 1);
                }
            }
        }
    }

    public class SuperEvent<T0, T1> : SuperEventBase<Action<T0, T1>>, ISuperEventLink<T0, T1>
    {
        private ISuperEventLink<T0, T1> _Link;

        /// <summary>
        /// Get an ISuperEventLink object that wraps this SuperEvent without allowing Dispatch.
        /// Provides a safe interface for classes outside the SuperEvent's "owner".
        /// </summary>
        public ISuperEventLink<T0, T1> Link
        {
            get
            {
                if (_HasLink)
                {
                    return _Link;
                }

                _Link = new SuperEventLink<T0, T1>(this);
                _HasLink = true;

                return _Link;
            }
        }

        public void Dispatch(T0 t0, T1 t1)
        {
            for (var i = _Count; i > 0; --i)
            {
                if (i > _Count)
                {
                    throw _IndexOutOfRangeException;
                }

                if (_Listeners[i - 1] != null)
                {
                    _Listeners[i - 1](t0, t1);
                }
                else
                {
                    RemoveAt(i - 1);
                }
            }

            for (var i = _OnceCount; i > 0; --i)
            {
                if (_ListenersOnce[i - 1] == null)
                {
                    continue;
                }

                var l = _ListenersOnce[i - 1];
                l(t0, t1);
                if (_ListenersOnce[i - 1] == l)
                {
#if SIGTRAP_RELAY_DBG
                    _SuperEventDebugger.DebugRemListener (this, _listenersOnce[i - 1]);
#endif
                    _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i - 1);
                }
            }
        }
    }

    public class SuperEvent<T0, T1, T2> : SuperEventBase<Action<T0, T1, T2>>, ISuperEventLink<T0, T1, T2>
    {
        private ISuperEventLink<T0, T1, T2> _Link;

        /// <summary>
        /// Get an ISuperEventLink object that wraps this SuperEvent without allowing Dispatch.
        /// Provides a safe interface for classes outside the SuperEvent's "owner".
        /// </summary>
        public ISuperEventLink<T0, T1, T2> Link
        {
            get
            {
                if (_HasLink)
                {
                    return _Link;
                }

                _Link = new SuperEventLink<T0, T1, T2>(this);
                _HasLink = true;

                return _Link;
            }
        }

        public void Dispatch(T0 t0, T1 t1, T2 t2)
        {
            for (var i = _Count; i > 0; --i)
            {
                if (i > _Count)
                {
                    throw _IndexOutOfRangeException;
                }

                if (_Listeners[i - 1] != null)
                {
                    _Listeners[i - 1](t0, t1, t2);
                }
                else
                {
                    RemoveAt(i - 1);
                }
            }

            for (var i = _OnceCount; i > 0; --i)
            {
                if (_ListenersOnce[i - 1] == null)
                {
                    continue;
                }

                var l = _ListenersOnce[i - 1];
                l(t0, t1, t2);
                if (_ListenersOnce[i - 1] == l)
                {
#if SIGTRAP_RELAY_DBG
                    _SuperEventDebugger.DebugRemListener (this, _listenersOnce[i - 1]);
#endif
                    _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i - 1);
                }
            }
        }
    }

    public class SuperEvent<T0, T1, T2, T3> : SuperEventBase<Action<T0, T1, T2, T3>>, ISuperEventLink<T0, T1, T2, T3>
    {
        private ISuperEventLink<T0, T1, T2, T3> _Link;

        /// <summary>
        /// Get an ISuperEventLink object that wraps this SuperEvent without allowing Dispatch.
        /// Provides a safe interface for classes outside the SuperEvent's "owner".
        /// </summary>
        public ISuperEventLink<T0, T1, T2, T3> Link
        {
            get
            {
                if (_HasLink)
                {
                    return _Link;
                }

                _Link = new SuperEventLink<T0, T1, T2, T3>(this);
                _HasLink = true;

                return _Link;
            }
        }

        public void Dispatch(T0 t0, T1 u, T2 v, T3 t3)
        {
            for (var i = _Count; i > 0; --i)
            {
                if (i > _Count)
                {
                    throw _IndexOutOfRangeException;
                }

                if (_Listeners[i - 1] != null)
                {
                    _Listeners[i - 1](t0, u, v, t3);
                }
                else
                {
                    RemoveAt(i - 1);
                }
            }

            for (var i = _OnceCount; i > 0; --i)
            {
                if (_ListenersOnce[i - 1] == null)
                {
                    continue;
                }

                var l = _ListenersOnce[i - 1];
                l(t0, u, v, t3);
                if (_ListenersOnce[i - 1] == l)
                {
#if SIGTRAP_RELAY_DBG
                    _SuperEventDebugger.DebugRemListener (this, _listenersOnce[i - 1]);
#endif
                    _OnceCount = RemoveAt(_ListenersOnce, _OnceCount, i - 1);
                }
            }
        }
    }

    #endregion
}