using System;
using Sigtrap.Relays;

#region Interfaces
namespace Sigtrap.Relays {
	public interface IRelayLinkBase<TDelegate> where TDelegate:class {
		/// <summary>
		/// How many persistent listeners does this intance currently have?
		/// </summary>
		uint listenerCount {get;}
		/// <summary>
		/// How many one-time listeners does this intance currently have?
		/// After dispatch, all current one-time listeners are automatically removed.
		/// </summary>
		uint oneTimeListenersCount {get;}

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
		/// Adds listener and creates a RelayBinding between the listener and the Relay.
		/// The RelayBinding can be used to enable/disable the listener.
		/// </summary>
		/// <returns>A new RelayBinding instance if successful, <c>null</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		IRelayBinding<TDelegate> BindListener(TDelegate listener, bool allowDuplicates=false);
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		bool AddOnce(TDelegate listener, bool allowDuplicates = false);

		/// <summary>
		/// Removes a persistent listener, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		bool RemoveListener(TDelegate listener);
		/// <summary>
		/// Removes all listeners.
		/// </summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
		void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = false);
	}
	public interface IRelayLink : IRelayLinkBase<Action> {}
	public interface IRelayLink<T> : IRelayLinkBase<Action<T>> {}
	public interface IRelayLink<T, U> : IRelayLinkBase<Action<T, U>> {}
	public interface IRelayLink<T, U, V> : IRelayLinkBase<Action<T, U, V>> {}
	public interface IRelayLink<T, U, V, W> : IRelayLinkBase<Action<T, U, V, W>> {}	
}
#endregion

#region Implementation
namespace Sigtrap.Relays.Link {
	public abstract class RelayLinkBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate:class {
		protected RelayBase<TDelegate> _relay;

		#region Constructors
		private RelayLinkBase(){}	// Private empty constructor to force use of params
		public RelayLinkBase(RelayBase<TDelegate> relay){
			_relay = relay;
		}
		#endregion

		#region IRelayLinkBase implementation
		public uint listenerCount {get {return _relay.listenerCount;}}
		public uint oneTimeListenersCount {get {return _relay.oneTimeListenersCount;}}

		/// <summary>
		/// Is this delegate already a persistent listener?
		/// Does NOT query one-time listeners.
		/// </summary>
		/// <param name="listener">Listener.</param>
		public bool Contains(TDelegate listener){
			return _relay.Contains(listener);
		}
		/// <summary>Adds a persistent listener.</summary>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public bool AddListener(TDelegate listener, bool allowDuplicates = false){
			return _relay.AddListener(listener, allowDuplicates);
		}
		/// <summary>
		/// Adds listener and creates a RelayBinding between the listener and the Relay.
		/// The RelayBinding can be used to enable/disable the listener.
		/// </summary>
		/// <returns>A new RelayBinding instance if successful, <c>null</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public IRelayBinding<TDelegate> BindListener(TDelegate listener, bool allowDuplicates=false){
			return _relay.BindListener(listener, allowDuplicates);
		}
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		public bool AddOnce(TDelegate listener, bool allowDuplicates = false){
			return _relay.AddOnce(listener, allowDuplicates);
		}
		/// <summary>Removes a persistent listener, if present.</summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveListener(TDelegate listener){
			return _relay.RemoveListener(listener);
		}
		/// <summary>Removes all listeners.</summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
		public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = false){
			_relay.RemoveAll(removePersistentListeners, removeOneTimeListeners);
		}
		#endregion
	}

	public class RelayLink : RelayLinkBase<Action>, IRelayLink {
		public RelayLink(RelayBase<Action> relay) : base(relay){}
	}
	public class RelayLink<T> : RelayLinkBase<Action<T>>, IRelayLink<T> {
		public RelayLink(RelayBase<Action<T>> relay) : base(relay){}
	}
	public class RelayLink<T, U> : RelayLinkBase<Action<T, U>>, IRelayLink<T, U> {
		public RelayLink(RelayBase<Action<T,U>> relay) : base(relay){}
	}
	public class RelayLink<T, U, V> : RelayLinkBase<Action<T, U, V>>, IRelayLink<T, U, V> {
		public RelayLink(RelayBase<Action<T,U,V>> relay) : base(relay){}
	}
	public class RelayLink<T, U, V, W> : RelayLinkBase<Action<T, U, V, W>>, IRelayLink<T, U, V, W> {
		public RelayLink(RelayBase<Action<T,U,V,W>> relay) : base(relay){}
	}
}
#endregion
