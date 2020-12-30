using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Timer // Просто таймер отсчёта времени
        {
            DateTime _stopTime;
            /// <summary>
            /// Остаток времени таймера
            /// </summary>
            internal string RestTime
            {
                get
                {
                    if (Launched)
                    {
                        TimeSpan lastTime = _stopTime - DateTime.Now;
                        return $"{lastTime.Hours}ч {lastTime.Minutes}м {lastTime.Seconds}с";
                    }
                    return "не запущен";
                }
            }
            /// <summary>
            /// Состояние таймера [Не обновляет сам таймер]
            /// </summary>
            internal bool Launched { get; private set; }
            /// <summary>
            /// Длительность отсчёта (в секундах)
            /// </summary>
            internal int Countdown { get; set; }
            /// <summary>
            /// Таймер обратного отсчёта
            /// </summary>
            internal Timer(int Seconds, bool start) { Countdown = Seconds; if (start) Start(); }
            /// <summary>
            /// Запуск таймера
            /// </summary>
            internal void Start() { Launched = true; _stopTime = DateTime.Now.AddSeconds(Countdown); }
            /// <summary>
            /// Остановка таймера и сброс отсчёта
            /// </summary>
            internal void Stop() { Launched = false; }
            /// <summary>
            /// Проверяет таймер и если он вышел - вернёт ЕДИНОЖДЫ true и остановит отсчёт
            /// </summary>
            internal bool IsFinsh()
            {
                if (Launched)
                {
                    if (DateTime.Now >= _stopTime) { Stop(); return true; }
                }
                return false;
            }
            /// <summary>
            /// Проверяет остановлен ли таймер и возвращает true если остановлен
            /// </summary>
            /// <returns></returns>
            internal bool IsOut()
            {
                IsFinsh(); return !Launched;
            }
        }
    }
}
