using System;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Serialization;

namespace Nico
{
    [DisallowMultipleComponent]
    public class ClientManager : MonoBehaviour
    {
        #region Singleton

        public static ClientManager singleton { get; private set; }
        public bool dontDestroyOnLoad = true;
        public bool runInBackground = true;

        private bool _init_singleton()
        {
            if (!Application.isPlaying)
            {
                throw new Exception("ClientManager must be initialized in play mode");
            }

            if (singleton == this)
            {
                return true;
            }

            if (singleton != null)
            {
                Destroy(gameObject);
                return false;
            }

            if (dontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            singleton = this;

            ProtoHandler.InitBuildInReader();
            return true;
        }

        /// <summary>
        /// 进入游戏前 重置上一次游戏的网络状态
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void _reset_statics()
        {
            if (singleton != null)
            {
                singleton.NetStop();
            }

            singleton = null;
        }

        #endregion

        IClientTransportGetter  _getter;

        public NetClient client { get; private set; }
        public bool connected => client.connected;
        public string address = "localhost";


        protected void Awake()
        {
            if (!_init_singleton()) return;
            if (singleton != this) return;
            _getter = GetComponent<IClientTransportGetter>();
            ClientTransport transport = _getter.GetClient();
            client = new NetClient(transport, address);
            NetworkLoop.onEarlyUpdate += client.OnEarlyUpdate;
            NetworkLoop.onLateUpdate += client.OnLateUpdate;
        }


        private void OnDestroy()
        {
            if (singleton != this) return;
            NetworkLoop.onEarlyUpdate -= client.OnEarlyUpdate;
            NetworkLoop.onLateUpdate -= client.OnLateUpdate;
            client.Stop();
            singleton = null;
        }

        /// <summary>
        /// 退出游戏时，停止网络传输，重置静态变量
        /// </summary>
        public void OnApplicationQuit()
        {
            NetStop();
            _reset_statics();
        }


        public void NetStart()
        {
            client.Start();
            Application.runInBackground = runInBackground;
        }


        public void NetStop() => client.Stop();


        public void Send<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>
            => client.Send(msg, type, channelId);
    }
}