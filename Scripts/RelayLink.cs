using System;
using Sigtrap.Relays;

namespace Sigtrap.Relays.Link {
	#region Interfaces
	public interface IRelayLinkBase<TDelegate> where TDelegate:class {
		/// <summary>
		/// How many listeners does this intance currently have?
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
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		void AddListener(TDelegate listener, bool allowDuplicates = false);
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		void AddOnce(TDelegate listener, bool allowDuplicates = false);

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
	public interface IRelayLink : IRelayLinkBase<System.Action> {}
	public interface IRelayLink<T> : IRelayLinkBase<System.Action<T>> {}
	public interface IRelayLink<T, U> : IRelayLinkBase<System.Action<T, U>> {}
	public interface IRelayLink<T, U, V> : IRelayLinkBase<System.Action<T, U, V>> {}
	public interface IRelayLink<T, U, V, W> : IRelayLinkBase<System.Action<T, U, V, W>> {}
	#endregion

	#region Classes
	public abstract class RelayLinkBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate:class {
		protected RelayBase<TDelegate> _relay;

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
		/// <summary>
		/// Adds a persistent listener.
		/// </summary>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public void AddListener(TDelegate listener, bool allowDuplicates = false){
			_relay.AddListener(listener, allowDuplicates);
		}
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		public void AddOnce(TDelegate listener, bool allowDuplicates = false){
			_relay.AddOnce(listener, allowDuplicates);
		}
		/// <summary>
		/// Removes a persistent listener, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveListener(TDelegate listener){
			return _relay.RemoveListener(listener);
		}
		/// <summary>
		/// Removes all listeners.
		/// </summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
		public void RemoveAll(bool removePersistentListeners = true, bool removeOneTimeListeners = false){
			_relay.RemoveAll(removePersistentListeners, removeOneTimeListeners);
		}
		#endregion
	}

	public class RelayLink : RelayLinkBase<Action>, IRelayLink {
		public static RelayLink CreateInstance(Relay relay){
			var result = new RelayLink();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T> : RelayLinkBase<Action<T>>, IRelayLink<T> {
		public static RelayLink<T> CreateInstance(Relay<T> relay){
			var result = new RelayLink<T>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U> : RelayLinkBase<Action<T, U>>, IRelayLink<T, U> {
		public static RelayLink<T, U> CreateInstance(Relay<T, U> relay){
			var result = new RelayLink<T, U>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U, V> : RelayLinkBase<Action<T, U, V>>, IRelayLink<T, U, V> {
		public static RelayLink<T, U, V> CreateInstance(Relay<T, U, V> relay){
			var result = new RelayLink<T, U, V>();
			result._relay = relay;
			return result;
		}
	}
	public class RelayLink<T, U, V, W> : RelayLinkBase<Action<T, U, V, W>>, IRelayLink<T, U, V, W> {
		public static RelayLink<T, U, V, W> CreateInstance(Relay<T, U, V, W> relay){
			var result = new RelayLink<T, U, V, W>();
			result._relay = relay;
			return result;
		}
	}
	#endregion
}