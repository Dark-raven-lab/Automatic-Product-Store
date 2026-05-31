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
        public class MyItem
        {
            internal int BuyPrice { get; set; }
            internal int SalePrice { get; set; }
            internal int Amount { get; set; } = 0;
            internal int MaxAmount { get; set; }
            internal bool AlowSale { get; set; }
            internal bool AlowBuy { get; set; }
            public TradeModel Mode { get; set; }

            public MyItem(int MaxAmount = 0, int BuyPrice = 1, bool AlowBuy = false, int SalePrice = 2, bool AlowSale = false, TradeModel storeMode = TradeModel.Storage)
            {
                this.MaxAmount = MaxAmount;
                this.BuyPrice = BuyPrice;
                this.SalePrice = SalePrice;
                this.AlowBuy = AlowBuy;
                this.AlowSale = AlowSale;
                Mode = storeMode;
            }
        }
    }
}
