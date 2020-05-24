//============================================================
// Project: DigitalFactory
// Author: Zoranner@ZORANNER
// Datetime: 2019-05-10 11:08:26
// Description: TODO >> This is a script Description.
//============================================================


namespace Zoranner.SuperEvents
{
    #region Interface

    public interface ISuperEventBinding
    {
        /// <summary>
        /// Is the listener currently subscribed to the SuperEvent?
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Should enabling the binding add the listener to the SuperEvent if already added elsewhere?
        /// </summary>
        bool AllowDuplicates { get; set; }

        /// <summary>
        /// How many persistent listeners does the bound SuperEvent currently have?
        /// </summary>
        uint ListenerCount { get; }

        /// <summary>
        /// Enable or disable the listener on the bound SuperEvent.
        /// </summary>
        /// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
        bool Enable(bool enable);
    }

    #endregion

    #region Implementation

    public class SuperEventBinding<TDelegate> : ISuperEventBinding where TDelegate : class
    {
        private ISuperEventLinkBase<TDelegate> SuperEvent { get; }
        private TDelegate Listener { get; }

        #region Constructors

        // Private empty constructor to force use of params
        private SuperEventBinding()
        {
        }

        public SuperEventBinding(ISuperEventLinkBase<TDelegate> relay, TDelegate listener, bool allowDuplicates,
            bool isListening) : this()
        {
            SuperEvent = relay;
            Listener = listener;
            AllowDuplicates = allowDuplicates;
            Enabled = isListening;
        }

        #endregion

        #region ISuperEventBinding implementation

        /// <inheritdoc>
        /// <summary>
        /// Is the listener currently subscribed to the SuperEvent?
        /// </summary>
        /// </inheritdoc>
        public bool Enabled { get; private set; }

        /// <inheritdoc>
        /// <summary>
        /// Should enabling the binding add the listener to the SuperEvent if already added elsewhere?
        /// </summary>
        /// </inheritdoc>
        public bool AllowDuplicates { get; set; }

        /// <inheritdoc>
        /// <summary>
        /// How many persistent listeners does the bound SuperEvent currently have?
        /// </summary>
        /// </inheritdoc>
        public uint ListenerCount => SuperEvent.ListenerCount;

        /// <inheritdoc>
        /// <summary>
        /// Enable or disable the listener on the bound SuperEvent.
        /// </summary>
        /// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
        /// </inheritdoc>
        public bool Enable(bool enable)
        {
            if (enable)
            {
                if (Enabled)
                {
                    return false;
                }

                if (!SuperEvent.AddListener(Listener, AllowDuplicates))
                {
                    return false;
                }

                Enabled = true;
                return true;
            }

            if (!Enabled)
            {
                return false;
            }

            if (!SuperEvent.RemoveListener(Listener))
            {
                return false;
            }

            Enabled = false;
            return true;
        }

        #endregion
    }

    #endregion
}