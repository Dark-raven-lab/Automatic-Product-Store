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
            public bool UseMarkup { get; set; } // флаг, использовать ли наценку (если false, берётся фиксированная SalePrice).
            public int MarkupPercent { get; set; } // индивидуальный процент для товара (если 0, а UseMarkup = true, то применяется глобальный).

            public MyItem(int MaxAmount = 0, int BuyPrice = 1, bool AlowBuy = false, int SalePrice = 2, bool AlowSale = false,
                  TradeModel storeMode = TradeModel.Storage, bool useMarkup = false, int markupPercent = 0)
            {
                this.MaxAmount = MaxAmount;
                this.BuyPrice = BuyPrice;
                this.SalePrice = SalePrice;
                this.AlowBuy = AlowBuy;
                this.AlowSale = AlowSale;
                Mode = storeMode;
                UseMarkup = useMarkup;
                MarkupPercent = markupPercent;
            }

            public int GetEffectiveSalePrice(int globalMarkupPercent)
            {
                if (UseMarkup)
                {
                    int percent = MarkupPercent > 0 ? MarkupPercent : globalMarkupPercent;
                    return BuyPrice * (100 + percent) / 100;
                }
                return SalePrice;
            }
        }
    }
}
