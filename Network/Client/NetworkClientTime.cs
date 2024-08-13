using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network.Client
{
    public class NetworkClientTime : ISystem, IOnInit<NetworkClient>
    {
        /// <summary>
        /// 客户端的本地时间
        /// </summary>
        public static long LocalTimeTicks => DateTime.UtcNow.Ticks;

        public int Rtt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rtt;
        }

        private NetworkQuality _quality;
        public event Action<NetworkQuality> OnQualityChanged;
        private int _rtt;
        private readonly IClientSocket _socket;
        private readonly NetworkBufferPool _pool;


        public NetworkClientTime(IClientSocket socket, NetworkBufferPool pool)
        {
            _socket = socket;
            _pool = pool;
            OnQualityChanged = delegate { };
        }

        public void OnInit()
        {
        }

        public void OnInit(NetworkClient t)
        {
            t.Register<PingMessage>(OnReceivePing);
            t.Register<RttMessage>(OnReceiveRtt);
        }

        /// <summary>
        /// 客户端收到服务器Ping消息时调用,立刻回复Pong消息
        /// </summary>
        /// <param name="pingMessage"></param>
        public void OnReceivePing(PingMessage pingMessage)
        {
            PongMessage pongMessage = new PongMessage(ref pingMessage);
            _socket.Send(pongMessage, _pool);
        }


        public void OnReceiveRtt(RttMessage obj)
        {
            _rtt = obj.rttMs;
            NetworkQuality quality = Utils.CalculateQuality(obj.rttMs);

            if (_quality != quality)
            {
                _quality = quality;
                OnQualityChanged(_quality);
            }

            TimeSpan span = TimeSpan.FromMilliseconds(obj.rttMs);
            NetworkLogger.Info($"rtt: {span}-{span.Milliseconds}ms , quality: {_quality}");
        }

        public void Dispose()
        {
        }
    }
}