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
            /// <summary>
            /// Цена покупки товара
            /// </summary>
            internal int BuyPrice { get; set; }
            /// <summary>
            /// Цена продажи товара
            /// </summary>
            internal int SalePrice { get; set; }
            /// <summary>
            /// Колличество товара на складе
            /// </summary>
            internal int Amount { get; set; } = 0;
            /// <summary>
            /// Максимальное колличество товара на складе
            /// </summary>
            internal int MaxAmount { get; set; }
            /// <summary>
            /// Разрешена продажа
            /// </summary>
            internal bool AlowSale { get; set; }
            /// <summary>
            /// Разрешена закупка
            /// </summary>
            internal bool AlowBuy { get; set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="MaxAmount">Минимальная граница</param>
            /// <param name="BuyPrice">Цена покупки товара</param>
            /// <param name="BuyPrice">Разрешить покупку ?</param>
            /// <param name="SalePrice">Цена продажи товара</param>
            /// <param name="AlowSale">Разрешить продажу ?</param>
            internal MyItem(int MaxAmount = 0, int BuyPrice = 1, bool AlowBuy = false, int SalePrice = 2, bool AlowSale = false)
            {
                this.MaxAmount = MaxAmount;
                this.BuyPrice = BuyPrice;
                this.SalePrice = SalePrice;
                this.AlowBuy = AlowBuy;
                this.AlowSale = AlowSale;
            }
        }
    }
}
