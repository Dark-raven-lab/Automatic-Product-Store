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
        public class Timer
        {
            DateTime _stopTime;
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
            internal bool Launched { get; private set; }
            internal int Countdown { get; set; }
            internal Timer(int Seconds, bool start) { Countdown = Seconds; if (start) Start(); }
            internal void Start() { Launched = true; _stopTime = DateTime.Now.AddSeconds(Countdown); }
            internal void Stop() { Launched = false; }
            internal bool IsFinsh()
            {
                if (Launched && DateTime.Now >= _stopTime) { Stop(); return true; }
                return false;
            }
            internal bool IsOut() { IsFinsh(); return !Launched; }
        }
    }
}
