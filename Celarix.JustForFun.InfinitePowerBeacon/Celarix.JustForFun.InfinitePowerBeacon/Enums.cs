using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon
{
	internal enum RecipeCostType
	{
		Nothing,
		Smelting,
		Compression,
		Hypercompression,
		Mining,
		Killing
	}

	internal enum MiningTier
	{
		Anything,
		WoodOrGold,
		StoneOrCopper,
		IronOrEmerald,
		Diamond,
		Netherite
	}

	internal enum CouponAvailability
	{
		NotYetEarned,
		Earned,
		Used
	}

	internal enum CouponLevel
	{
		NoCoupon,
		Low,
		High
	}
}
